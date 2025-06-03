using SpaceGame.Movement.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Movement.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(CombatFleetMovementExecutionGroup))]
    public partial struct ApplyFleetVelocitySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComponent))
                return;

            float dt = timeComponent.DeltaTimeScaled;

            foreach (var (transform, velocity, desiredDir, desiredSpeed, rotationSpeed, thrust, entity) in SystemAPI
                     .Query<RefRW<LocalTransform>, RefRW<Velocity>, RefRO<DesiredMovementDirection>, RefRO<DesiredSpeed>, RefRO<RotationSpeed>, RefRO<ThrustSettings>>()
                     .WithAll<FleetMovementTag>()
                     .WithEntityAccess())
            {
                float3 forward = math.mul(transform.ValueRO.Rotation, new float3(1, 0, 0));
                float currentSpeed = math.dot(forward, velocity.ValueRO.Value);
                float maxSpeed = thrust.ValueRO.MaxSpeed;
                float speedMultiplier = 1.0f;

                var speed = math.clamp(desiredSpeed.ValueRO.Value * speedMultiplier, 0.0f, maxSpeed);
                float speedDelta = speed - currentSpeed;
                float maxDelta = (speedDelta > 0 ? thrust.ValueRO.Acceleration : thrust.ValueRO.Decceleration) * dt;
                float newSpeed = currentSpeed + math.clamp(speedDelta, -math.abs(maxDelta), math.abs(maxDelta));

                float forwardSpeedRatio = math.saturate(math.abs(newSpeed) / thrust.ValueRO.MaxSpeed);
                float rotationPenalty = 1f - (forwardSpeedRatio * thrust.ValueRO.SpeedRotationPenalty);
                float effectiveRotationSpeed = rotationSpeed.ValueRO.Value * rotationPenalty;

                float3 desired = math.normalizesafe(desiredDir.ValueRO.Value);
                float3 currentForward = math.mul(transform.ValueRO.Rotation, new float3(1, 0, 0));

                float angle = math.acos(math.clamp(math.dot(currentForward, desired), -1f, 1f));
                float maxRotate = effectiveRotationSpeed * dt;
                float3 axis = math.normalize(math.cross(currentForward, desired));
                quaternion targetRot = math.abs(angle) < 0.001f || !math.all(math.isfinite(axis))
                    ? transform.ValueRO.Rotation
                    : math.mul(quaternion.AxisAngle(axis, math.min(angle, maxRotate)), transform.ValueRO.Rotation);

                transform.ValueRW.Rotation = math.normalize(targetRot);

                float3 newForward = math.mul(transform.ValueRW.Rotation, new float3(1, 0, 0));
                velocity.ValueRW.Value = newForward * newSpeed;
                transform.ValueRW.Position += velocity.ValueRW.Value * dt;
            }
        }
    }
}
