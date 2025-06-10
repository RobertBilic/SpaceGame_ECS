using SpaceGame.Combat.Components;
using SpaceGame.Movement.Components;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(CombatTargetingGroup))]
    partial struct TurretTargetingSystem : ISystem
    {
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
            if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComp))
                return;

            if (!SystemAPI.TryGetSingleton<SpatialDatabaseSingleton>(out SpatialDatabaseSingleton spatialDatabaseSingleton))
                return;

            spatialDatabaseCellLookup.Update(ref state);
            spatialDatabaseElementLookup.Update(ref state);
            spatialDatabaseLookup.Update(ref state);

            var list = TeamBasedSpatialDatabaseUtility.ConstructCachedSpatialDatabseROList(spatialDatabaseSingleton, spatialDatabaseLookup, spatialDatabaseCellLookup, spatialDatabaseElementLookup);

            float deltaTime = timeComp.DeltaTimeScaled;
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
                    TeamBasedSpatialDatabaseUtility.GetTeamBasedDatabase(list, teamTag.ValueRO.Team, TeamFilterMode.DifferentTeam, out bool found, out var cachedDb);

                    if (!found)
                        continue;

                    var turretTargetingCollector = new RangeBasedTargetingCollectorSingle(state.EntityManager, turretPosition, range.ValueRO.Value, teamTag.ValueRO.Team);
                    SpatialDatabase.QuerySphereCellProximityOrder(cachedDb._SpatialDatabase, cachedDb._SpatialDatabaseCells, cachedDb._SpatialDatabaseElements, turretPosition, range.ValueRO.Value,ref turretTargetingCollector);

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

                    float3 heading = math.mul(math.mul(parentWorldRotation, rotationTransform.ValueRW.Rotation), new float3(1, 0, 0));

                    ecb.SetComponent(entity, new Heading() { Value = heading } );
                }
            }

            foreach (var db in list)
                db.Dispose();

            list.Dispose();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}