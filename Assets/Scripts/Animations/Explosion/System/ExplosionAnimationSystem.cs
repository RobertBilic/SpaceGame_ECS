using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using SpaceGame.Animations.Components;

namespace SpaceGame.Animations.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct ExplosionAnimationSystem : ISystem
    {

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var scale))
                return;

            var dt = scale.DeltaTime;
            
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (animationState, random, sprites, entity) in
                SystemAPI.Query<RefRW<ExplosionAnimationState>, RefRO<RandomGenerator>, DynamicBuffer<ExplosionSpriteElement>>()
                    .WithEntityAccess())
            {
                animationState.ValueRW.TimeSinceLastFrame += dt;

                if (animationState.ValueRW.TimeSinceLastFrame >= animationState.ValueRW.TimeUntilNextFrame)
                {
                    if (animationState.ValueRW.CurrentFrame < sprites.Length)
                    {
                        var oldSpriteEntity = sprites[animationState.ValueRW.CurrentFrame].SpriteEntity;
                        if (SystemAPI.Exists(oldSpriteEntity) && SystemAPI.HasComponent<Disabled>(oldSpriteEntity) == false)
                        {
                            ecb.AddComponent<Disabled>(oldSpriteEntity);
                        }
                    }

                    animationState.ValueRW.CurrentFrame++;
                    animationState.ValueRW.TimeSinceLastFrame = 0f;

                    if (animationState.ValueRW.CurrentFrame >= sprites.Length)
                    {
                        ecb.DestroyEntity(entity);
                        continue;
                    }

                    var newSpriteElement = sprites[animationState.ValueRW.CurrentFrame];
                    var newSpriteEntity = newSpriteElement.SpriteEntity;

                    if (SystemAPI.Exists(newSpriteEntity))
                    {
                        if (SystemAPI.HasComponent<Disabled>(newSpriteEntity))
                        {
                            ecb.RemoveComponent<Disabled>(newSpriteEntity);
                        }
                    }

                    animationState.ValueRW.TimeUntilNextFrame = random.ValueRO.Value.NextFloat(newSpriteElement.TimeOnElementMin, newSpriteElement.TimeOnElementMax);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}