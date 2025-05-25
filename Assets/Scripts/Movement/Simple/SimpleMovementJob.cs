using SpaceGame.Movement.Components;
using SpaceGame.Movement.Flowfield.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Movement.Simple.Jobs
{
    [BurstCompile]
    [WithAll(typeof(SimpleMovementEntityTag), typeof(MovementTarget))]
    public partial struct SimpleMovementJob : IJobEntity
    {
        public float DeltaTime;

        [NativeDisableParallelForRestriction]
        public ComponentLookup<DisengageCurveDirection> DisengageLookup;

        [NativeDisableParallelForRestriction]
        public ComponentLookup<ShipBankingData> BankingLookup;

        public EntityCommandBuffer.ParallelWriter ECB;
        public Unity.Mathematics.Random Random;

        public void Execute(
            [EntityIndexInQuery] int index,
            Entity entity,
            ref LocalTransform transform,
            ref CurrentRotation currentRotation,
            ref ShipMovementBehaviourState behavior,
            in MoveSpeed moveSpeed,
            in RotationSpeed turnSpeed,
            in ApproachDistance stopDistance,
            in MovementTarget movementTarget)
        {
            float3 pos = transform.Position;
            float3 targetPosition = movementTarget.Value;
            var dir = math.normalize((targetPosition - pos));

            float3 movementDir = math.normalize(new float3(dir.x, dir.y, 0));
            if (math.lengthsq(movementDir) < 0.0001f) return;

            float dist = math.distance(new float2(pos.x, pos.y), new float2(targetPosition.x, targetPosition.y));

            switch (behavior.Value)
            {
                case ShipMovementBehaviour.MoveToTarget:
                    if (dist < stopDistance.Value)
                    {
                        behavior.Value = ShipMovementBehaviour.Disengage;
                        if (!DisengageLookup.HasComponent(entity))
                        {
                            ECB.AddComponent(index, entity, new DisengageCurveDirection
                            {
                                Value = Random.NextFloat()
                            });
                        }
                    }
                    break;

                case ShipMovementBehaviour.Disengage:
                    if (dist > stopDistance.Value * 2f)
                    {
                        behavior.Value = ShipMovementBehaviour.Reengage;
                        if (DisengageLookup.HasComponent(entity))
                            ECB.RemoveComponent<DisengageCurveDirection>(index, entity);
                    }
                    break;

                case ShipMovementBehaviour.Reengage:
                    behavior.Value = ShipMovementBehaviour.MoveToTarget;
                    break;
            }

            float angle = currentRotation.Value;
            float3 forward = new float3(math.cos(angle), math.sin(angle), 0);
            float3 desiredDir = forward;

            if (behavior.Value == ShipMovementBehaviour.MoveToTarget || behavior.Value == ShipMovementBehaviour.Reengage)
            {
                desiredDir = movementDir;
            }
            else if (behavior.Value == ShipMovementBehaviour.Disengage)
            {
                float curveAmount = 0.5f;
                float curveDir = DisengageLookup.HasComponent(entity) ? DisengageLookup[entity].Value : 1f;

                float3 backward = -movementDir;
                float3 perp = new float3(-movementDir.y, movementDir.x, 0) * curveDir;
                desiredDir = math.normalize(math.lerp(backward, perp, curveAmount));
            }

            float dot = math.dot(math.normalize(forward), math.normalize(desiredDir));
            float angleBetween = math.degrees(math.acos(math.clamp(dot, -1f, 1f)));
            float crossZ = forward.x * desiredDir.y - forward.y * desiredDir.x;
            float sign = math.sign(crossZ);

            float maxTurn = turnSpeed.Value * DeltaTime;
            float applied = math.min(angleBetween, maxTurn);
            float rad = math.radians(applied * sign);
            float cos = math.cos(rad);
            float sin = math.sin(rad);

            float3 rotated = math.normalize(new float3(
                cos * forward.x - sin * forward.y,
                sin * forward.x + cos * forward.y,
                0
            ));

            angle = math.atan2(rotated.y, rotated.x);
            currentRotation.Value = angle;

            float turnFactor = angleBetween / 180f;
            float adjustedSpeed = math.lerp(moveSpeed.Value, moveSpeed.Value * 0.1f, turnFactor);

            transform.Position += rotated * adjustedSpeed * DeltaTime;

            float roll = 0f;
            if (BankingLookup.HasComponent(entity))
            {
                var banking = BankingLookup[entity];
                float bankStrength = 45f;
                float targetAngle = math.clamp(crossZ * bankStrength, -bankStrength, bankStrength);
                banking.CurrentBankAngle = math.lerp(banking.CurrentBankAngle, targetAngle, DeltaTime * banking.SmoothSpeed);
                roll = banking.CurrentBankAngle;
                BankingLookup[entity] = banking;
            }

            transform.Rotation = quaternion.Euler(math.radians(roll), 0f, angle);
        }
    }
}