using SpaceGame.Combat;
using SpaceGame.Combat.Components;
using SpaceGame.Detection.Component;
using SpaceGame.Movement.Components;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace SpaceGame.Detection.Systems
{
    [UpdateInGroup(typeof(CombatInitializationGroup), OrderLast = true)]
    public partial struct DetectionSystem : ISystem
    {
        private CachedSpatialDatabaseRO _CachedSpatialDatabase;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimpleMovementEntityTag>();
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

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var(ltw, detectionRange, teamTag, entity) in SystemAPI.Query<RefRO<LocalToWorld>,RefRO<DetectionRange>, RefRO<TeamTag>>()
                .WithEntityAccess())
            {
                if (state.EntityManager.HasComponent<MovementTarget>(entity))
                {
                    var movementTarget = SystemAPI.GetComponentRW<MovementTarget>(entity);

                    if (movementTarget.ValueRO.TargetEntity != Entity.Null && state.EntityManager.Exists(movementTarget.ValueRO.TargetEntity))
                    {
                        var targetLtw = state.EntityManager.GetComponentData<LocalToWorld>(movementTarget.ValueRO.TargetEntity);
                        movementTarget.ValueRW.Value = targetLtw.Position;
                        continue;
                    }

                    RangeBasedTargetingCollectorSingle collector = new RangeBasedTargetingCollectorSingle(state.EntityManager, ltw.ValueRO.Position, detectionRange.ValueRO.Value, teamTag.ValueRO.Team);

                    SpatialDatabase.QuerySphereCellProximityOrder(_CachedSpatialDatabase._SpatialDatabase, _CachedSpatialDatabase._SpatialDatabaseCells, _CachedSpatialDatabase._SpatialDatabaseElements
                        , ltw.ValueRO.Position, detectionRange.ValueRO.Value, ref collector);

                    if(collector.collectedEnemy != Entity.Null)
                    {
                        movementTarget.ValueRW.TargetEntity = collector.collectedEnemy;
                        movementTarget.ValueRW.Value = SystemAPI.GetComponent<LocalToWorld>(collector.collectedEnemy).Position;
                    }
                }
                else
                {
                    RangeBasedTargetingCollectorSingle collector = new RangeBasedTargetingCollectorSingle(state.EntityManager, ltw.ValueRO.Position, detectionRange.ValueRO.Value, teamTag.ValueRO.Team);

                    SpatialDatabase.QuerySphereCellProximityOrder(_CachedSpatialDatabase._SpatialDatabase, _CachedSpatialDatabase._SpatialDatabaseCells, _CachedSpatialDatabase._SpatialDatabaseElements
                        , ltw.ValueRO.Position, detectionRange.ValueRO.Value, ref collector);

                    if (collector.collectedEnemy != Entity.Null)
                    {
                        ecb.AddComponent(entity, new MovementTarget()
                        {
                            TargetEntity = collector.collectedEnemy,
                            Value = SystemAPI.GetComponent<LocalToWorld>(collector.collectedEnemy).Position
                        });
                    }
                }
            }

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
