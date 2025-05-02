using SpaceGame.Combat.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace SpaceGame.Combat.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(CombatSystemGroup))]
    [UpdateAfter(typeof(TurretFiringSystem))]
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
            float deltaTime = SystemAPI.Time.DeltaTime;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (transform, heading, speed, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Heading>, RefRO<MoveSpeed>>().WithAll<BulletTag>().WithEntityAccess())
            {
                ecb.SetComponent(entity, new PreviousPosition() { Value = transform.ValueRO.Position });
                transform.ValueRW.Position += heading.ValueRO.Value * speed.ValueRO.Value * deltaTime;
            }

            foreach (var (lifetime, entity) in SystemAPI.Query<RefRW<Lifetime>>().WithAll<BulletTag>().WithEntityAccess())
            {
                lifetime.ValueRW.Value -= deltaTime;
                if (lifetime.ValueRW.Value <= 0f)
                {
                    ecb.DestroyEntity(entity);
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