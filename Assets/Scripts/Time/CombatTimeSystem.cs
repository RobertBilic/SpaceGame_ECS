using SpaceGame.Combat.Components;
using Unity.Entities;

namespace SpaceGame.Combat.Systems
{
    [UpdateInGroup(typeof(CombatInitializationGroup), OrderLast = true)]
    partial class CombatTimeSystem : SystemBase
    {
        public float timeMultiplier;
        public float lastNonZeroTimeMultiplier;

        protected override void OnCreate()
        {
            lastNonZeroTimeMultiplier = 1.0f;
            timeMultiplier = 1.0f;
        }

        protected override void OnStartRunning()
        {
            if (!SystemAPI.TryGetSingletonRW<GlobalTimeComponent>(out var timeComponent))
                return;

            timeComponent.ValueRW.ElapsedTime = 0.0f;
            timeComponent.ValueRW.ElapsedTimeScaled = 0.0f;
            timeComponent.ValueRW.FrameCount = 0;
            timeComponent.ValueRW.FrameCountScaled = 0;
        }

        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonRW<GlobalTimeComponent>(out var timeComponent))
                return;

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var (timeScaleChangeRequest, entity) in SystemAPI.Query<RefRO<TimeScaleChangeRequest>>()
                .WithEntityAccess())
            {
                var value = timeScaleChangeRequest.ValueRO.Value;

                if (value != 0.0f)
                    lastNonZeroTimeMultiplier = value;

                if (value == 0.0f && timeMultiplier == 0.0f)
                    value = lastNonZeroTimeMultiplier;

                timeMultiplier = value;
                ecb.DestroyEntity(entity);
            }

            timeComponent.ValueRW.FrameCount++;
            timeComponent.ValueRW.FrameCountScaled += (long)timeMultiplier;

            var dt = SystemAPI.Time.DeltaTime;

            timeComponent.ValueRW.ElapsedTime += dt;
            timeComponent.ValueRW.DeltaTime = dt;

            timeComponent.ValueRW.DeltaTimeScaled = dt * timeMultiplier;
            timeComponent.ValueRW.ElapsedTimeScaled += timeComponent.ValueRO.DeltaTimeScaled;

            UnityEngine.Time.timeScale = timeMultiplier;

            if (ecb.ShouldPlayback)
                ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}