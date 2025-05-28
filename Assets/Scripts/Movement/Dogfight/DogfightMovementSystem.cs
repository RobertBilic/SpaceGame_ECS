using SpaceGame.Movement.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Movement.Systems
{
    readonly partial struct DogfightMovementAspect : IAspect
    {
        public readonly RefRO<LocalToWorld> Ltw;
        public readonly RefRO<DetectedEntity> Target;
        public readonly RefRO<EngageDistance> EngageRange;
        public readonly RefRO<DisengageDistance> DisengageRange;
        public readonly RefRO<ThrustSettings> ThurstSettings;

        public readonly RefRW<DisengageSide> DisengageSide;
        public readonly RefRW<DesiredMovementDirection> DesiredMovementDirection;
        public readonly RefRW<DesiredSpeed> DesiredSpeed;
        public readonly RefRW<DisengageCurveDirection> DisengageCurve;
        public readonly RefRW<DogfightStateComponent> State;
    }


    [BurstCompile]
    [UpdateInGroup(typeof(CombatMovementCalculationGroup))]
    public partial struct DogfightMovementSystem : ISystem
    {
        private Random rnd;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            rnd = new Random(333222);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (dogfightAspect,entity) in SystemAPI
                     .Query<DogfightMovementAspect>()
                     .WithEntityAccess()
                     .WithAll<DogfightTag>())
            {
                var target = dogfightAspect.Target;
                var ltw = dogfightAspect.Ltw;
                var engageRangeComp = dogfightAspect.EngageRange;
                var disengageRangeComp = dogfightAspect.DisengageRange;
                var disengageCurveDir = dogfightAspect.DisengageCurve;
                var shipThrustSettings = dogfightAspect.ThurstSettings.ValueRO;
                var dogfightState = dogfightAspect.State;

                var disengageSide = dogfightAspect.DisengageSide;
                var desiredSpeedComp = dogfightAspect.DesiredSpeed;
                var desiredDir = dogfightAspect.DesiredMovementDirection;

                if (target.ValueRO.Value == Entity.Null || !SystemAPI.Exists(target.ValueRO.Value))
                {
                    desiredDir.ValueRW.Value = float3.zero;
                    continue;
                }

                if (!SystemAPI.HasComponent<LocalToWorld>(target.ValueRO.Value))
                {
                    desiredDir.ValueRW.Value = float3.zero;
                    continue;
                }

                float3 selfPos = ltw.ValueRO.Position;
                var targetLtw = SystemAPI.GetComponent<LocalToWorld>(target.ValueRO.Value);
                float3 targetPos = targetLtw.Position;

                float3 targetVel = float3.zero;
                if (SystemAPI.HasComponent<Velocity>(target.ValueRO.Value))
                    targetVel = SystemAPI.GetComponent<Velocity>(target.ValueRO.Value).Value;

                float offset = 0.0f;

                if (SystemAPI.HasComponent<BoundingRadius>(target.ValueRO.Value))
                    offset += SystemAPI.GetComponent<BoundingRadius>(target.ValueRO.Value).Value * math.length(targetLtw.Value.c0.xyz);
                if (SystemAPI.HasComponent<BoundingRadius>(entity))
                    offset += SystemAPI.GetComponent<BoundingRadius>(entity).Value * math.length(ltw.ValueRO.Value.c0.xyz);


                float3 toTarget = targetPos - selfPos;
                float distance = math.length(toTarget) - offset;

                float3 leadPos = targetPos + targetVel * math.min(1f, distance / 30f);
                float3 desired = float3.zero;
                float speedFactor = 1.0f;
                float engageRange = engageRangeComp.ValueRO.Value * rnd.NextFloat(0.9f,1.1f);
                float disengageRange = disengageRangeComp.ValueRO.Value * rnd.NextFloat(0.9f, 1.1f);

                if (dogfightState.ValueRO.Value == DogfightState.Engage && distance < engageRange)
                {
                    dogfightState.ValueRW.Value = DogfightState.Disengage;
                    disengageSide.ValueRW.Value = rnd.NextBool() ? -1.0f : 1.0f;
                }
                else if (dogfightState.ValueRO.Value == DogfightState.Disengage && distance > disengageRange)
                {
                    dogfightState.ValueRW.Value = DogfightState.Engage;
                }


                if (dogfightState.ValueRO.Value == DogfightState.Disengage)
                {
                    float3 perp = math.cross(toTarget, new float3(0, 0, disengageSide.ValueRO.Value));
                    float3 away = -math.normalize(toTarget) + math.normalize(perp);
                    desired = math.normalizesafe(away);
                    disengageCurveDir.ValueRW.Direction = desired;
                }
                else if (dogfightState.ValueRO.Value == DogfightState.Engage)
                {
                    float closenessFactor = math.saturate(math.unlerp(engageRange, disengageRange, distance));

                    desired = math.normalizesafe(leadPos - selfPos);
                    speedFactor = math.lerp(0.3f, 1.0f, closenessFactor); 
                }

                float desiredSpeed = shipThrustSettings.MaxSpeed * speedFactor;
                desiredSpeedComp.ValueRW.Value = desiredSpeed;
                desiredDir.ValueRW.Value = desired;
            }
        }
    }
}
