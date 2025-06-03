using SpaceGame.Combat.Patrol.Components;
using SpaceGame.Movement.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Movement.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(CombatFleetMovementCalculationGroup), OrderLast = true)]
    partial struct FleetMovementSystem : ISystem
    {

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FleetMember>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (ltw, desiredSpeed, desiredDir, fleetMember) in SystemAPI.Query<RefRO<LocalToWorld>, RefRW<DesiredSpeed>, RefRW<DesiredMovementDirection>,
                    RefRO<FleetMember>>()
                 .WithNone<DetectedEntity>())
            {
                if (fleetMember.ValueRO.FleetReference == Entity.Null || !state.EntityManager.Exists(fleetMember.ValueRO.FleetReference))
                    continue;

                if (!state.EntityManager.HasComponent<FleetEntity>(fleetMember.ValueRO.FleetReference))
                    continue;

                var fleetEntity = state.EntityManager.GetComponentData<FleetEntity>(fleetMember.ValueRO.FleetReference);

                if (!state.EntityManager.HasComponent<LocalToWorld>(fleetEntity.Leader))
                    continue;

                var baseSpeed = state.EntityManager.GetComponentData<DesiredSpeed>(fleetEntity.Leader).Value * fleetEntity.CohesionSpeedMultiplier;
                var leaderLtw = state.EntityManager.GetComponentData<LocalToWorld>(fleetEntity.Leader);
                var leaderDesiredDir = state.EntityManager.GetComponentData<DesiredMovementDirection>(fleetEntity.Leader);

                var leaderMatrix = leaderLtw.Value;
                float3x3 rotationOnly = new float3x3(
                    math.normalize(leaderMatrix.c0.xyz),
                    math.normalize(leaderMatrix.c1.xyz),
                    math.normalize(leaderMatrix.c2.xyz)
                );

                float3 rotatedOffset = math.mul(rotationOnly, fleetMember.ValueRO.LocalOffset);
                float3 targetPos = leaderLtw.Position + rotatedOffset;
                float3 dir = targetPos - ltw.ValueRO.Position;
                float3 leaderForward = leaderMatrix.c0.xyz; 
                float distance = math.distance(ltw.ValueRO.Position, targetPos);
                float distanceMultiplier = math.remap(0f, 50f, 1.0f, 2.0f, distance); 
                distanceMultiplier = math.clamp(distanceMultiplier, 1.0f, 2.0f);

                float dot = math.dot(math.normalize(dir), math.normalize(leaderForward)); 
                float alignmentMultiplier = math.remap(-1f, 1f, 0.8f, 1.0f, dot); 
                alignmentMultiplier = math.clamp(alignmentMultiplier, 0.8f, 1.0f);

                float finalMultiplier = distanceMultiplier * alignmentMultiplier;
                float3 finalDir = (distance < 0.5f && dot > 0.85f ) ? leaderDesiredDir.Value : math.normalize(dir);

                desiredSpeed.ValueRW.Value = baseSpeed * finalMultiplier;
                desiredDir.ValueRW.Value = finalDir;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}