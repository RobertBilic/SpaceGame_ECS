using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(CombatSystemGroup))]
partial struct TurretTargetingSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TurretTag>();

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (transform, rotationSpeed, range, isAlive, rotationBase, entity)
                 in SystemAPI.Query<RefRW<LocalTransform>, RefRO<RotationSpeed>, RefRO<Range>, RefRO<IsAlive>, RefRO<TurretRotationBaseReference>>()
                     .WithAll<TurretTag, LocalToWorld>()
                     .WithEntityAccess())
        {
            if (!isAlive.ValueRO.Value)
                continue;

            float3 turretPosition = SystemAPI.GetComponent<LocalToWorld>(entity).Position;

            bool alreadyHasTarget = SystemAPI.HasComponent<Target>(entity);
            Entity targetEntity = Entity.Null;

            if (alreadyHasTarget)
            {
                targetEntity = SystemAPI.GetComponent<Target>(entity).Value;

                if (!SystemAPI.Exists(targetEntity) || !SystemAPI.GetComponent<IsAlive>(targetEntity).Value ||
                    math.distancesq(turretPosition, SystemAPI.GetComponent<LocalToWorld>(targetEntity).Position) > range.ValueRO.Value * range.ValueRO.Value)
                {
                    ecb.RemoveComponent<Target>(entity);
                    targetEntity = Entity.Null;
                }
            }

            if (targetEntity == Entity.Null)
            {
                Entity closestEnemy = Entity.Null;
                float closestDistanceSq = float.MaxValue;

                foreach (var (enemyTransform, enemyIsAlive, enemyEntity) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<IsAlive>>()
                         .WithAll<Team2Tag>()
                         .WithEntityAccess())
                {
                    if (!enemyIsAlive.ValueRO.Value)
                        continue;

                    float distanceSq = math.distancesq(turretPosition, enemyTransform.ValueRO.Position);

                    if (distanceSq < closestDistanceSq && distanceSq <= range.ValueRO.Value * range.ValueRO.Value)
                    {
                        closestDistanceSq = distanceSq;
                        closestEnemy = enemyEntity;
                    }
                }

                if (closestEnemy != Entity.Null)
                {
                    targetEntity = closestEnemy;
                    ecb.AddComponent(entity, new Target() { Value = targetEntity });
                }
            }

            if (targetEntity != Entity.Null)
            {
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
