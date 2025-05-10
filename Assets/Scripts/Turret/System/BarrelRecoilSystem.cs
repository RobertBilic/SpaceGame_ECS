using SpaceGame.Animations.Components;
using SpaceGame.Combat.Systems;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Animations.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(CombatAnimationGroup))]
    partial struct BarrelRecoilSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var (recoil, transform, entity) in SystemAPI.Query<RefRW<BarrelRecoil>, RefRW<LocalTransform>>().WithEntityAccess())
            {
                recoil.ValueRW.CurrentTime += deltaTime;
                float t = recoil.ValueRO.CurrentTime / recoil.ValueRO.Duration;

                if (t >= 1f)
                {
                    ecb.RemoveComponent<BarrelRecoil>(entity);
                    continue;
                }

                float displacement = 0f;

                if (t < 0.5f)
                {
                    displacement = math.lerp(0f, -recoil.ValueRO.MaxDistance, t * 2f);
                }
                else
                {
                    displacement = math.lerp(-recoil.ValueRO.MaxDistance, 0f, (t - 0.5f) * 2f);
                }

                transform.ValueRW.Position = recoil.ValueRO.DefaultPosition + new float3(displacement, 0, 0);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        private float EaseInOut(float t)
        {
            return t * t * (3f - 2f * t);
        }
    }
}