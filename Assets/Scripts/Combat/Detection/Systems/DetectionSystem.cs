﻿using SpaceGame.Combat;
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
        private CachedSpatialDatabaseRO _CachedSpatialDatabase;
        private int Intervals;
        private int CurrentInterval;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CombatEntity>();
            //TODO: Dynamic interval based on the number of combat entities, 100 entities per cycle
            Intervals = 5;
            CurrentInterval = 0;

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

            //TODO: Better target acquiring

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

                    SpatialDatabase.QuerySphereCellProximityOrder(_CachedSpatialDatabase._SpatialDatabase, _CachedSpatialDatabase._SpatialDatabaseCells, _CachedSpatialDatabase._SpatialDatabaseElements
                        , ltw.ValueRO.Position, detectionRange.ValueRO.Value, ref collector);

                    if (collector.collectedEnemy != Entity.Null)
                    {
                        ecb.AddComponent(entity, new DetectedEntity() { Value = collector.collectedEnemy });
                        ecb.AddComponent<NeedsCombatStateChange>(entity);
                    }
                    else
                    {
                        ecb.RemoveComponent<DetectedEntity>(entity);
                    }
                }
                else
                {
                    RangeBasedTargetingCollectorSingle collector = new RangeBasedTargetingCollectorSingle(state.EntityManager, ltw.ValueRO.Position, detectionRange.ValueRO.Value, teamTag.ValueRO.Team);

                    SpatialDatabase.QuerySphereCellProximityOrder(_CachedSpatialDatabase._SpatialDatabase, _CachedSpatialDatabase._SpatialDatabaseCells, _CachedSpatialDatabase._SpatialDatabaseElements
                        , ltw.ValueRO.Position, detectionRange.ValueRO.Value, ref collector);

                    if (collector.collectedEnemy != Entity.Null)
                    {
                        ecb.AddComponent(entity, new DetectedEntity() { Value = collector.collectedEnemy });
                        ecb.AddComponent<NeedsCombatStateChange>(entity);
                    }
                }
            }

            CurrentInterval = (CurrentInterval + 1) % Intervals;

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
