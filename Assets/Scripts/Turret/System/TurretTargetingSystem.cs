using SpaceGame.Combat.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(CombatSystemGroup))]
    partial struct TurretTargetingSystem : ISystem
    {
        private CachedSpatialDatabaseRO _CachedSpatialDatabase;

        private ComponentLookup<SpatialDatabase> spatialDatabaseLookup;
        private BufferLookup<SpatialDatabaseCell> spatialDatabaseCellLookup;
        private BufferLookup<SpatialDatabaseElement> spatialDatabaseElementLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TurretTag>();
            state.RequireForUpdate<SpatialDatabaseSingleton>();

            spatialDatabaseLookup = SystemAPI.GetComponentLookup<SpatialDatabase>(true);
            spatialDatabaseCellLookup = SystemAPI.GetBufferLookup<SpatialDatabaseCell>(true);
            spatialDatabaseElementLookup = SystemAPI.GetBufferLookup<SpatialDatabaseElement>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton<SpatialDatabaseSingleton>(out SpatialDatabaseSingleton spatialDatabaseSingleton))
            {
                spatialDatabaseCellLookup.Update(ref state);
                spatialDatabaseElementLookup.Update(ref state);
                spatialDatabaseLookup.Update(ref state);

                _CachedSpatialDatabase = new CachedSpatialDatabaseRO
                {
                    SpatialDatabaseEntity = spatialDatabaseSingleton.TargetablesSpatialDatabase,
                    SpatialDatabaseLookup = spatialDatabaseLookup,
                    CellsBufferLookup = spatialDatabaseCellLookup,
                    ElementsBufferLookup = spatialDatabaseElementLookup
                };

                _CachedSpatialDatabase.CacheData();
            }
            else
            {
                return;
            }

            float deltaTime = SystemAPI.Time.DeltaTime;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (transform, rotationSpeed, range, isAlive, rotationBase, teamTag, entity)
                     in SystemAPI.Query<RefRW<LocalToWorld>, RefRO<RotationSpeed>, RefRO<Range>, RefRO<IsAlive>, RefRO<RotationBaseReference>, RefRO<TeamTag>>()
                         .WithAll<TurretTag, LocalToWorld>()
                         .WithEntityAccess())
            {
                float3 turretPosition = transform.ValueRO.Position;

                bool alreadyHasTarget = SystemAPI.HasComponent<Target>(entity);
                Entity targetEntity = Entity.Null;

                if (alreadyHasTarget)
                {
                    targetEntity = SystemAPI.GetComponent<Target>(entity).Value;

                    if (targetEntity == Entity.Null || !SystemAPI.Exists(targetEntity)
                        || (!SystemAPI.HasComponent<IsAlive>(targetEntity))
                        || (SystemAPI.HasComponent<LocalToWorld>(targetEntity) && (math.distancesq(turretPosition, SystemAPI.GetComponent<LocalToWorld>(targetEntity).Position) > range.ValueRO.Value * range.ValueRO.Value)))
                    {
                        ecb.RemoveComponent<Target>(entity);
                        targetEntity = Entity.Null;
                    }
                }

                if (targetEntity == Entity.Null)
                {

                    var turretTargetingCollector = new RangeBasedTargetingCollector(state.EntityManager, turretPosition, range.ValueRO.Value, teamTag.ValueRO.Team);
                    SpatialDatabase.QuerySphereCellProximityOrder(_CachedSpatialDatabase._SpatialDatabase, _CachedSpatialDatabase._SpatialDatabaseCells, _CachedSpatialDatabase._SpatialDatabaseElements, turretPosition, range.ValueRO.Value,ref turretTargetingCollector);

                    Entity closestEnemy = turretTargetingCollector.collectedEnemy;

                    if (closestEnemy != Entity.Null)
                    {
                        targetEntity = closestEnemy;
                        ecb.AddComponent(entity, new Target() { Value = targetEntity });
                    }
                }

                if (targetEntity != Entity.Null)
                {
                    if (!SystemAPI.HasComponent<LocalToWorld>(targetEntity))
                        continue;

                    var enemyPosition = SystemAPI.GetComponent<LocalToWorld>(targetEntity).Position;
                    quaternion parentWorldRotation = quaternion.identity;

                    if (SystemAPI.HasComponent<Parent>(rotationBase.ValueRO.RotationBase))
                    {
                        var parent = SystemAPI.GetComponent<Parent>(rotationBase.ValueRO.RotationBase);
                        parentWorldRotation = SystemAPI.GetComponent<LocalToWorld>(parent.Value).Rotation;
                    }

                    float3 direction = math.normalize(enemyPosition - turretPosition);
                    float angle = math.atan2(direction.y, direction.x);
                    quaternion desiredWorldRotation = quaternion.RotateZ(angle);

                    quaternion desiredLocalRotation = desiredWorldRotation;

                    if (!parentWorldRotation.Equals(quaternion.identity))
                        desiredLocalRotation = math.mul(math.inverse(parentWorldRotation), desiredWorldRotation);


                    var rotationTransform = SystemAPI.GetComponentRW<LocalTransform>(rotationBase.ValueRO.RotationBase);

                    rotationTransform.ValueRW.Rotation = math.slerp(
                        rotationTransform.ValueRW.Rotation,
                        desiredLocalRotation,
                        rotationSpeed.ValueRO.Value * deltaTime
                    );

                    quaternion worldRotation = parentWorldRotation.Equals(quaternion.identity) ? rotationTransform.ValueRW.Rotation
                        : math.mul(parentWorldRotation, rotationTransform.ValueRW.Rotation);


                    ecb.SetComponent(entity, new Heading() { Value = math.mul(worldRotation, new float3(1, 0, 0)) });
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}