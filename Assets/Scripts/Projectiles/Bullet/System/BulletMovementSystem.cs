using SpaceGame.Combat.Components;
using SpaceGame.Movement.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace SpaceGame.Combat.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(CombatMovementGroup))]
    partial struct BulletMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BulletTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComp))
                return;

            float deltaTime = timeComp.DeltaTimeScaled;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (transform, heading, speed, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Heading>, RefRO<ThrustSettings>>().WithAll<BulletTag>().WithEntityAccess())
            {
                ecb.SetComponent(entity, new PreviousPosition() { Value = transform.ValueRO.Position });
                transform.ValueRW.Position += heading.ValueRO.Value * speed.ValueRO.MaxSpeed * deltaTime;
            }

            foreach (var (lifetime, bulletId, entity) in SystemAPI.Query<RefRW<Lifetime>, RefRO<ProjectileId>>().WithAll<BulletTag>().WithEntityAccess())
            {
                lifetime.ValueRW.Value -= deltaTime;
                if (lifetime.ValueRW.Value <= 0f)
                {
                    if (SystemAPI.TryGetSingletonBuffer<ProjectilePoolRequest>(out var poolBuffer))
                    {
                        poolBuffer.Add(new ProjectilePoolRequest()
                        {
                            Entity = entity,
                            Id = bulletId.ValueRO.Value
                        });
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}