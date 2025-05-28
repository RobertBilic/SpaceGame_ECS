using Unity.Burst;
using Unity.Entities;

namespace SpaceGame.Detection.Systems
{
    [UpdateInGroup(typeof(CombatInitializationGroup))]
    [UpdateBefore(typeof(Detection.Systems.DetectionSystem))]
    partial struct DetectionCooldownSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CombatDetectionCooldown>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComp))
                return;

            var dt = timeComp.DeltaTime;

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach(var (cooldown, entity) in SystemAPI.Query<RefRW<CombatDetectionCooldown>>()
                .WithEntityAccess())
            {
                cooldown.ValueRW.Value -= dt;

                if(cooldown.ValueRO.Value <= 0.0f)
                    ecb.RemoveComponent<CombatDetectionCooldown>(entity);
            }

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