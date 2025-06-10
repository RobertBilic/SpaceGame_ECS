using SpaceGame.Combat.Components;
using SpaceGame.Detection.Component;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace SpaceGame.Combat.Fleets
{
    [UpdateInGroup(typeof(CombatInitializationGroup))]
    public partial struct FormFleetSystem : ISystem
    {
        private NativeList<Entity> processedEntities;
        private NativeList<Entity> currentlyProcessedEntities;

        ComponentLookup<SpatialDatabase> spatialDatabaseLookup;
        BufferLookup<SpatialDatabaseElement> spatialDatabaseElementLookup;
        BufferLookup<SpatialDatabaseCell> spatialDatabaseCellLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            spatialDatabaseLookup = state.GetComponentLookup<SpatialDatabase>(true);
            spatialDatabaseElementLookup = state.GetBufferLookup<SpatialDatabaseElement>(true);
            spatialDatabaseCellLookup = state.GetBufferLookup<SpatialDatabaseCell>(true);
            processedEntities = new NativeList<Entity>(Allocator.Persistent);
            currentlyProcessedEntities = new NativeList<Entity>(Allocator.Persistent);
            state.RequireForUpdate<FormFleetTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<SpatialDatabaseSingleton>(out SpatialDatabaseSingleton spatialDatabaseSingleton))
                return;

            processedEntities.Clear();
            var ecb = new EntityCommandBuffer(Allocator.Persistent);


            spatialDatabaseLookup.Update(ref state);
            spatialDatabaseElementLookup.Update(ref state);
            spatialDatabaseCellLookup.Update(ref state);

            var databases = TeamBasedSpatialDatabaseUtility.ConstructCachedSpatialDatabseROList(spatialDatabaseSingleton, spatialDatabaseLookup, spatialDatabaseCellLookup, spatialDatabaseElementLookup);

            foreach (var (ltw, detectionRange, teamTag, entity) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<DetectionRange>, RefRO<TeamTag>>()
                .WithNone<FleetMember, FleetLeader>()
                .WithAll<CombatEntity, FormFleetTag>()
                .WithEntityAccess())
            {
                ecb.RemoveComponent<FormFleetTag>(entity);
                if (processedEntities.Contains(entity))
                    continue;

                currentlyProcessedEntities.Clear();

                RangeBasedTargetingCollectorMultiple queryCollector = new RangeBasedTargetingCollectorMultiple(ref currentlyProcessedEntities, state.EntityManager
                    , ltw.ValueRO.Position, detectionRange.ValueRO.Value, TeamFilterMode.SameTeam, teamTag.ValueRO.Team);

                TeamBasedSpatialDatabaseUtility.GetTeamBasedDatabase(databases, teamTag.ValueRO.Team, TeamFilterMode.SameTeam, out var found, out var cachedDb);

                if (!found)
                    continue;

                SpatialDatabase.QuerySphereCellProximityOrder(cachedDb._SpatialDatabase, cachedDb._SpatialDatabaseCells, cachedDb._SpatialDatabaseElements,
                    ltw.ValueRO.Position, detectionRange.ValueRO.Value, ref queryCollector);


                //TODO: Arbitrary fleet Management, add fleet archetypes

                if (currentlyProcessedEntities.Length < 2)
                    continue;

                NativeList<float> shipRadii = new NativeList<float>(currentlyProcessedEntities.Length, Allocator.Temp);
                NativeList<Entity> validEntities = new NativeList<Entity>(currentlyProcessedEntities.Length, Allocator.Temp);

                for (int i = 0; i < currentlyProcessedEntities.Length; i++)
                {
                    if (!state.EntityManager.HasComponent<FormFleetTag>(currentlyProcessedEntities[i]))
                        continue;

                    if (processedEntities.Contains(currentlyProcessedEntities[i]))
                        continue;

                    validEntities.Add(currentlyProcessedEntities[i]);
                    shipRadii.Add(state.EntityManager.GetComponentData<BoundingRadius>(currentlyProcessedEntities[i]).Value);
                }

                if(validEntities.Length < 2)
                {
                    shipRadii.Dispose();
                    validEntities.Dispose();
                    continue;
                }

                var formation = FormationUtility.GeneratePackedFormation(shipRadii);

                var fleetEntity = ecb.CreateEntity();
                var fleetMemberBuffer = ecb.AddBuffer<FleetMemberReference>(fleetEntity);
                ecb.AddComponent<LocalTransform>(fleetEntity);
                ecb.AddComponent<LocalToWorld>(fleetEntity);

                float maximumSpeed = float.MaxValue;

                for (int i = 0; i < validEntities.Length; i++)
                {
                    var currentEntity = validEntities[i];

                    if (processedEntities.Contains(currentEntity))
                        continue;

                    if (state.EntityManager.HasComponent<ThrustSettings>(currentEntity))
                    {
                        var thrustSettings = state.EntityManager.GetComponentData<ThrustSettings>(currentEntity);
                        if (maximumSpeed > thrustSettings.MaxSpeed)
                            maximumSpeed = thrustSettings.MaxSpeed;
                    }

                    processedEntities.Add(currentEntity);
                    if (i == 0)
                    {
                        ecb.AddComponent(currentEntity, new FleetLeader() { FleetReference = fleetEntity });
                    }
                    else
                    {
                        ecb.AddComponent(currentEntity, new FleetMember() { 
                            FleetReference = fleetEntity,
                            LocalOffset = formation[i],
                            Command = FleetCommand.Follow
                        });
                        ecb.AddComponent<FleetMovementTag>(currentEntity);
                        fleetMemberBuffer.Add(new FleetMemberReference() { Ref = currentEntity });
                    }
                }

                ecb.AddComponent(fleetEntity, new FleetEntity() { 
                    Leader = validEntities[0],
                    MaxSpeed = maximumSpeed,
                });

                shipRadii.Dispose();
                validEntities.Dispose();
            }

            foreach (var db in databases)
                db.Dispose();

            databases.Dispose();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            processedEntities.Dispose();
            currentlyProcessedEntities.Dispose();
        }
    }
}
