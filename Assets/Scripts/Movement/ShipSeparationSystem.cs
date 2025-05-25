using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using SpaceGame.Movement.Components;
using SpaceGame.Movement.Simple.Systems;
using SpaceGame.SpatialGrid.Components;
using SpaceGame.Combat;

namespace SpaceGame.Movement
{
    [UpdateInGroup(typeof(CombatMovementGroup))]
    [UpdateBefore(typeof(SimpleShipMovementSystem))]
    [BurstCompile]
    public partial struct ShipSeparationSteeringSystem : ISystem
    {
        CachedSpatialDatabaseRO _CachedSpatialDatabase;
        NativeList<Entity> collectedEntities;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            collectedEntities = new NativeList<Entity>(1024, Allocator.Persistent);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComponent))
                return;

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

            foreach(var (ltw, movementTarget, separationSettings,entity) in SystemAPI.Query<RefRO<LocalToWorld>, RefRW<MovementTarget>, RefRO<SeparationSettings>>()
                .WithEntityAccess())
            {

                collectedEntities.Clear();
                RangeBasedTargetingCollectorMultiple collector = new RangeBasedTargetingCollectorMultiple(ref collectedEntities, state.EntityManager, ltw.ValueRO.Position, separationSettings.ValueRO.RepulsionRadius, -1);

                SpatialDatabase.QuerySphereCellProximityOrder(_CachedSpatialDatabase._SpatialDatabase, _CachedSpatialDatabase._SpatialDatabaseCells, _CachedSpatialDatabase._SpatialDatabaseElements
                    , ltw.ValueRO.Position, 30.0f, ref collector);

                float3 selfPos = ltw.ValueRO.Position;
                float distance = math.distance(selfPos, movementTarget.ValueRO.Value);
                float3 desiredDirection = math.normalizesafe(movementTarget.ValueRO.Value - selfPos);
                float3 repulsion = float3.zero;
                float r = separationSettings.ValueRO.RepulsionRadius;

                foreach(var current in collectedEntities)
                {
                    if (entity == current)
                        continue;

                    float3 otherPos = state.EntityManager.GetComponentData<LocalToWorld>(current).Position;
                    float3 toOther = otherPos - selfPos;
                    float distSqr = math.lengthsq(toOther);

                    if (distSqr < r * r && distSqr > 0.0001f)
                    {
                        float3 away = -math.normalize(toOther);
                        float strength = (1.0f - math.sqrt(distSqr) / r);
                        repulsion += away * strength;
                    }
                }

                float3 finalDir = math.normalizesafe(desiredDirection + repulsion * r);

                float3 finalTargetPos = selfPos + finalDir * distance;
                movementTarget.ValueRW.Value = finalTargetPos;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            collectedEntities.Dispose();
        }
    }
}