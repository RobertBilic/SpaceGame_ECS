using SpaceGame.Combat.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(CombatMovementGroup))]
public partial struct MissileMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComp)) 
            return;

        float deltaTime = timeComp.DeltaTimeScaled;

        foreach (var (transform, settings, targetRef) in SystemAPI
            .Query<RefRW<LocalTransform>, RefRO<MissileSettings>, RefRO<Target>>())
        {
            if (!SystemAPI.HasComponent<LocalToWorld>(targetRef.ValueRO.Value))
            {
                float3 fw = math.mul(transform.ValueRO.Rotation, new float3(1, 0, 0));
                transform.ValueRW.Position += fw * settings.ValueRO.MoveSpeed * deltaTime;
                continue;
            }

            float3 targetPos = SystemAPI.GetComponent<LocalToWorld>(targetRef.ValueRO.Value).Position;
            float3 missilePos = transform.ValueRO.Position;

            float3 forward = math.mul(transform.ValueRO.Rotation, new float3(1, 0, 0));
            float3 toTarget = math.normalizesafe(targetPos - missilePos);

            float currentAngle = math.atan2(forward.y, forward.x);
            float targetAngle = math.atan2(toTarget.y, toTarget.x);

            float angleDiff = math.atan2(math.sin(targetAngle - currentAngle), math.cos(targetAngle - currentAngle));

            float maxRotation = settings.ValueRO.TurnSpeed * deltaTime;
            float rotationStep = math.clamp(angleDiff, -maxRotation, maxRotation);

            float newAngle = currentAngle + rotationStep;
            quaternion newRot = quaternion.RotateZ(newAngle);

            transform.ValueRW.Rotation = newRot;
            float3 newForward = math.mul(newRot, new float3(1, 0, 0));
            transform.ValueRW.Position += newForward * settings.ValueRO.MoveSpeed * deltaTime;
        }
    }
}
