using SpaceGame.Combat.Patrol.Components;
using SpaceGame.Movement.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Movement.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(CombatMovementCalculationGroup))]
    partial struct PatrolMovementSystem : ISystem
    {
        private float minDistance;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            minDistance = 10.0f;
            state.RequireForUpdate<PatrolTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (ltw, desiredSpeed, desiredDir, waypoints, waypointIndex, boundingRadius, thrustSettings) in SystemAPI.Query<RefRO<LocalToWorld>, RefRW<DesiredSpeed>, RefRW<DesiredMovementDirection>,
                DynamicBuffer<PatrolWaypoint>, RefRW<PatrolWaypointIndex>, RefRO<BoundingRadius>, RefRO<ThrustSettings>>()
                .WithAll<PatrolTag>())
            {
                var currentPos = ltw.ValueRO.Position;
                var currentTarget = waypoints[waypointIndex.ValueRO.Value].Value;
                var distSq = math.distancesq(currentPos, currentTarget);
                var scaledBoundingRadius = math.length(ltw.ValueRO.Value.c0.xyz) * boundingRadius.ValueRO.Value;

                if (distSq < (minDistance + scaledBoundingRadius) * (minDistance + scaledBoundingRadius))
                {
                    var nextIndex = (waypointIndex.ValueRO.Value + 1) % waypoints.Length;
                    waypointIndex.ValueRW.Value = nextIndex;
                    currentTarget = waypoints[nextIndex].Value;
                }

                var direction = currentTarget - currentPos;
                desiredDir.ValueRW.Value = math.normalizesafe(direction);
                desiredSpeed.ValueRW.Value = thrustSettings.ValueRO.MaxSpeed * 0.5f;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}