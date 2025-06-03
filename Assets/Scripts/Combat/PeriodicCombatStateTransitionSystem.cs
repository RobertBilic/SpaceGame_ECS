using SpaceGame.Combat.StateTransition.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace SpaceGame.Combat.StateTransition.Systems
{
    [UpdateInGroup(typeof(CombatStateTransitionGroup))]
    [UpdateBefore(typeof(CombatStateTransitionSystem))]
    partial struct PeriodicCombatStateTransitionSystem : ISystem
    {
        private float StateCheckMax;
        private float CurrentTime;
        private int Intervals;
        private int CurrentInterval;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            StateCheckMax = 5.0f;
            CurrentTime = StateCheckMax;
            Intervals = 5;
            CurrentInterval = 0;

            state.RequireForUpdate<NeedsCombatStateChange>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComp))
                return;

            CurrentTime -= timeComp.DeltaTime;

            if (CurrentTime > 0.0f)
                return;

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach(var (localTransform, entity) in SystemAPI.Query<RefRO<LocalTransform>>()
                .WithNone<NeedsCombatStateChange>()
                .WithAll<CombatEntity>()
                .WithEntityAccess())
            {
                if (entity.Index % Intervals != CurrentInterval)
                    continue;

                ecb.AddComponent<NeedsCombatStateChange>(entity);
            }

            if (CurrentInterval + 1 >= Intervals)
                CurrentTime = StateCheckMax;


            CurrentInterval = (CurrentInterval + 1) % Intervals;

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}