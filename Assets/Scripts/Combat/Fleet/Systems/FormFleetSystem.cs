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
        private CachedSpatialDatabaseRO _CachedSpatialDatabase;
        private NativeList<Entity> processedEntities;
        private NativeList<Entity> currentlyProcessedEntities;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            processedEntities = new NativeList<Entity>(Allocator.Persistent);
            currentlyProcessedEntities = new NativeList<Entity>(Allocator.Persistent);
            state.RequireForUpdate<FormFleetTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton<SpatialDatabaseSingleton>(out SpatialDatabaseSingleton spatialDatabaseSingleton))
            {
                _CachedSpatialDatabase = new CachedSpatialDatabaseRO
                {
                    SpatialDatabaseEntity = spatialDatabaseSingleton.TargetablesSpatialDatabase,
                    SpatialDatabaseLookup = SystemAPI.GetComponentLookup<SpatialDatabase>(true),
                    CellsBufferLookup = SystemAPI.GetBufferLookup<SpatialDatabaseCell>(true),
                    ElementsBufferLookup = SystemAPI.GetBufferLookup<SpatialDatabaseElement>(true),
                };

                _CachedSpatialDatabase.CacheData();
            }
            else
            {
                return;
            }

            processedEntities.Clear();
            var ecb = new EntityCommandBuffer(Allocator.Persistent);

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
                    , ltw.ValueRO.Position, detectionRange.ValueRO.Value, TargetFilterMode.SameTeam, teamTag.ValueRO.Team);

                SpatialDatabase.QuerySphereCellProximityOrder(_CachedSpatialDatabase._SpatialDatabase, _CachedSpatialDatabase._SpatialDatabaseCells, _CachedSpatialDatabase._SpatialDatabaseElements,
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
