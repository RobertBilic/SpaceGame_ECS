using Unity.Burst;
using Unity.Entities;
using SpaceGame.Game.Initialization.Systems;
using SpaceGame.Game.State.Component;
using System;

public delegate void OnGameStateChanged(GameState state);

namespace SpaceGame.Game.State.Systems {
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(GameInitializationSystem))]
    partial class GameStateSystem : SystemBase
    {
        GameState lastProcessedState;

        public event OnGameStateChanged OnGameStateChange;

        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<ChangeGameStateRequest>();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
        }
        protected override void OnStopRunning()
        {
            base.OnStopRunning();
        }

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach(var (gameStateChangeRequest, entity) in SystemAPI.Query<RefRO<ChangeGameStateRequest>>()
                .WithEntityAccess())
            {
                var gameState = gameStateChangeRequest.ValueRO.Value;

                if (lastProcessedState == gameState)
                    return;

                lastProcessedState = gameState;
                SetGroupEnabled(typeof(CombatSystemGroup), gameState.HasFlag(GameState.Combat));
                OnGameStateChange?.Invoke(gameState);

                ecb.DestroyEntity(entity);
            
                if(SystemAPI.TryGetSingletonRW<GameStateComponent>(out var gameStateSingleton))
                    gameStateSingleton.ValueRW.Value = gameState;
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private void SetGroupEnabled(Type type,bool enabled)
        {
            var group = World.GetExistingSystemManaged(type);
            group.Enabled = enabled;
        }
    }
}