using SpaceGame.Combat.Components;
using SpaceGame.Movement.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace SpaceGame.Combat.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(CombatLateUpdateGroup))]
    partial struct ProjectileLifetimeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Lifetime>();
            state.RequireForUpdate<ProjectileId>();
            state.RequireForUpdate<GlobalTimeComponent>();
            state.RequireForUpdate<ProjectilePoolRequest>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComp))
                return;

            if (!SystemAPI.TryGetSingletonBuffer<ProjectilePoolRequest>(out var poolBuffer))
                return;

            float deltaTime = timeComp.DeltaTimeScaled;

            foreach (var (lifetime, bulletId, entity) in SystemAPI.Query<RefRW<Lifetime>, RefRO<ProjectileId>>()
                .WithAll<ProjectileTag>()
                .WithEntityAccess())
            {
                lifetime.ValueRW.Value -= deltaTime;
                if (lifetime.ValueRW.Value <= 0f)
                {
                    poolBuffer.Add(new ProjectilePoolRequest()
                    {
                        Entity = entity,
                        Id = bulletId.ValueRO.Value
                    });
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}