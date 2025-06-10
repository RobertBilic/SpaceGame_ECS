using SpaceGame.Combat;
using SpaceGame.Combat.Components;
using SpaceGame.Detection.Component;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Detection.Systems
{
    [UpdateInGroup(typeof(CombatInitializationGroup), OrderLast = true)]
    public partial struct DetectionSystem : ISystem
    {
        private int Intervals;
        private int CurrentInterval;
        private float coolDownPerInternval;
        private float cooldownAtTheEnd;
        private float currentCooldown;

        private ComponentLookup<FleetMember> fleetMemberLookup;
        private ComponentLookup<FleetMovementTag> fleetMovementTagLookup;

        private ComponentLookup<SpatialDatabase> dbLookup;
        private BufferLookup<SpatialDatabaseElement> elementLookup;
        private BufferLookup<SpatialDatabaseCell> cellLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CombatEntity>();
            fleetMemberLookup = state.GetComponentLookup<FleetMember>(true);
            fleetMovementTagLookup = state.GetComponentLookup<FleetMovementTag>(true);

            dbLookup = state.GetComponentLookup<SpatialDatabase>(true);
            cellLookup = state.GetBufferLookup<SpatialDatabaseCell>(true);
            elementLookup = state.GetBufferLookup<SpatialDatabaseElement>(true);

            //TODO: Dynamic interval based on the number of combat entities, 100 entities per cycle
            Intervals = 10;
            currentCooldown = 0.0f;
            coolDownPerInternval = 0.1f;
            cooldownAtTheEnd = 0.5f;
            CurrentInterval = 0;

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComp))
                return;

            if (!SystemAPI.TryGetSingleton<SpatialDatabaseSingleton>(out SpatialDatabaseSingleton spatialDatabaseSingleton))
                return;

            dbLookup.Update(ref state);
            cellLookup.Update(ref state);
            elementLookup.Update(ref state);

            var list = TeamBasedSpatialDatabaseUtility.ConstructCachedSpatialDatabseROList(spatialDatabaseSingleton, dbLookup, cellLookup, elementLookup);

            currentCooldown -= timeComp.DeltaTime;

            if (currentCooldown > 0.0f)
                return;

            //TODO: Better target acquiring
            fleetMemberLookup.Update(ref state);
            fleetMovementTagLookup.Update(ref state);

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var(ltw, detectionRange, teamTag, entity) in SystemAPI.Query<RefRO<LocalToWorld>,RefRO<DetectionRange>, RefRO<TeamTag>>()
                .WithAll<CombatEntity>()
                .WithNone<CombatDetectionCooldown>()
                .WithEntityAccess())
            {
                if (entity.Index % Intervals != CurrentInterval)
                    continue;

                //TODO: Space out the combat detection with a persistant component such as NextDetectionCycle to avoid structural changes, for now this is okay
                ecb.AddComponent(entity, new CombatDetectionCooldown() { Value = 1.0f });

                if (state.EntityManager.HasComponent<DetectedEntity>(entity))
                {
                    var detectedEntity = SystemAPI.GetComponentRW<DetectedEntity>(entity);

                    if (detectedEntity.ValueRO.Value != Entity.Null && state.EntityManager.Exists(detectedEntity.ValueRO.Value))
                    {
                        if(state.EntityManager.HasComponent<LocalToWorld>(detectedEntity.ValueRO.Value))
                        {
                            var targetLtw = state.EntityManager.GetComponentData<LocalToWorld>(detectedEntity.ValueRO.Value);

                            if(math.distancesq(targetLtw.Position, ltw.ValueRO.Position) >= detectionRange.ValueRO.Value * detectionRange.ValueRO.Value)
                            {
                                detectedEntity.ValueRW.Value = Entity.Null;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                    RangeBasedTargetingCollectorSingle collector = new RangeBasedTargetingCollectorSingle(state.EntityManager, ltw.ValueRO.Position, detectionRange.ValueRO.Value, teamTag.ValueRO.Team);

                    TeamBasedSpatialDatabaseUtility.GetTeamBasedDatabase(list, teamTag.ValueRO.Team, TeamFilterMode.DifferentTeam, out var found, out var cachedDb);

                    if (!found)
                        continue;

                    SpatialDatabase.QuerySphereCellProximityOrder(cachedDb._SpatialDatabase, cachedDb._SpatialDatabaseCells, cachedDb._SpatialDatabaseElements
                        , ltw.ValueRO.Position, detectionRange.ValueRO.Value, ref collector);

                    if (collector.collectedEnemy != Entity.Null)
                    {
                        ecb.AddComponent(entity, new DetectedEntity() { Value = collector.collectedEnemy });
                        ecb.AddComponent<NeedsCombatStateChange>(entity);

                        if (fleetMemberLookup.HasComponent(entity))
                        {
                            if (fleetMovementTagLookup.HasComponent(entity))
                                ecb.RemoveComponent<FleetMovementTag>(entity);
                        }
                    }
                    else
                    {
                        ecb.RemoveComponent<DetectedEntity>(entity);
                        if(fleetMemberLookup.HasComponent(entity))
                        {
                            if (!fleetMovementTagLookup.HasComponent(entity))
                                ecb.AddComponent<FleetMovementTag>(entity);
                        }
                    }
                }
                else
                {
                    TeamBasedSpatialDatabaseUtility.GetTeamBasedDatabase(list, teamTag.ValueRO.Team, TeamFilterMode.DifferentTeam, out var found, out var cachedDb);
                    RangeBasedTargetingCollectorSingle collector = new RangeBasedTargetingCollectorSingle(state.EntityManager, ltw.ValueRO.Position, detectionRange.ValueRO.Value, teamTag.ValueRO.Team);
                    if (!found)
                        continue;

                    SpatialDatabase.QuerySphereCellProximityOrder(cachedDb._SpatialDatabase, cachedDb._SpatialDatabaseCells, cachedDb._SpatialDatabaseElements
                        , ltw.ValueRO.Position, detectionRange.ValueRO.Value, ref collector);

                    if (collector.collectedEnemy != Entity.Null)
                    {
                        ecb.AddComponent(entity, new DetectedEntity() { Value = collector.collectedEnemy });
                        ecb.AddComponent<NeedsCombatStateChange>(entity);

                        if (fleetMemberLookup.HasComponent(entity))
                        {
                            if (fleetMovementTagLookup.HasComponent(entity))
                                ecb.RemoveComponent<FleetMovementTag>(entity);
                        }
                    }
                }
            }

            CurrentInterval = (CurrentInterval + 1) % Intervals;
            currentCooldown = CurrentInterval == 0 ? cooldownAtTheEnd : coolDownPerInternval;
            
            foreach (var db in list)
                db.Dispose();

            list.Dispose();
            if (ecb.ShouldPlayback)
                ecb.Playback(state.EntityManager);
            ecb.Dispose();

        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}
