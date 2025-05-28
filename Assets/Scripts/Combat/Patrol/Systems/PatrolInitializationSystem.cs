using SpaceGame.Combat.Patrol.Components;
using SpaceGame.Game.Initialization.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat.Patrol.Systems {

    [UpdateInGroup(typeof(CombatInitializationGroup))]
    partial struct PatrolInitializationSystem : ISystem
    {
        private Random rand;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            rand = new Random(332299);
            state.RequireForUpdate<NeedsPatrolInitializationTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<Config>(out var configComp))
                return;

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            var waypointCount = 4;
            var minDistance = 100.0f;
            var points = new NativeList<float3>(waypointCount, Allocator.Temp);

            foreach (var (ltw, waypointIndex, entity) in SystemAPI.Query<RefRO<LocalToWorld>, RefRW<PatrolWaypointIndex>>()
                .WithAll<NeedsPatrolInitializationTag>()
                .WithEntityAccess())
            {
                if(state.EntityManager.HasBuffer<PatrolWaypoint>(entity))
                {
                    var waypoints = state.EntityManager.GetBuffer<PatrolWaypoint>(entity);
                    var minDistanceSq = float.MaxValue;
                    var ind = 0;

                    for (int i = 0; i < waypoints.Length; i++)
                    {
                        var distSq = math.distancesq(waypoints[i].Value, ltw.ValueRO.Position);
                        if (distSq < minDistanceSq)
                        {
                            ind = i;
                            minDistanceSq = distSq;
                        }
                    }

                    waypointIndex.ValueRW.Value = ind;
                }
                else
                {
                    //TODO: Better patrol system based on POIs
                    points.Clear();
                    points.Add(ltw.ValueRO.Position);

                    var buffer = ecb.AddBuffer<PatrolWaypoint>(entity);
                    var maxAttempts = 30;
                    int attempts = 0;

                    while (points.Length < waypointCount && attempts < maxAttempts)
                    {
                        attempts++;
                        var candidate = new float3(
                            rand.NextFloat(-configComp.GameSize / 2f, configComp.GameSize / 2f),
                            rand.NextFloat(-configComp.GameSize / 2f, configComp.GameSize / 2f),
                            0.0f);

                        bool valid = true;
                        for (int i = 0; i < points.Length; i++)
                        {
                            if (math.distancesq(candidate, points[i]) < minDistance * minDistance)
                            {
                                valid = false;
                                break;
                            }
                        }

                        if (valid)
                            points.Add(candidate);
                    }

                    for (int i = 0; i < points.Length; i++)
                        buffer.Add(new PatrolWaypoint { Value = points[i] });

                    waypointIndex.ValueRW.Value = 0;
                }

                ecb.RemoveComponent<NeedsPatrolInitializationTag>(entity);
            }

            points.Dispose();
            if (ecb.ShouldPlayback)
                ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}