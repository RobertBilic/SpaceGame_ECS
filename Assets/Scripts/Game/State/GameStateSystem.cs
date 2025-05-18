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
        InputSystem_Actions inputActions;

        public event OnGameStateChanged OnGameStateChange;

        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<GameStateComponent>();
            lastProcessedState = GameState.None;
            inputActions = new InputSystem_Actions();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            inputActions.Enable();
            inputActions.Gameplay.StateChange.performed += StateChange_performed;
        }
        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            inputActions.Disable();
            inputActions.Gameplay.StateChange.performed -= StateChange_performed;
        }

        private void StateChange_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var value = (byte)obj.ReadValue<float>();

            if (SystemAPI.TryGetSingletonRW<GameStateComponent>(out var gameState))
            {
                if (Enum.IsDefined(typeof(GameState), value))
                    gameState.ValueRW.Value = (GameState)value;
                else
                    UnityEngine.Debug.LogWarning($"GameState not defined at value {value}");

            }
        }
        protected override void OnUpdate()
        { 
            if (!SystemAPI.TryGetSingleton<GameStateComponent>(out var gameState))
                return;

            if (lastProcessedState == gameState.Value)
                return;

            lastProcessedState = gameState.Value;
            SetGroupEnabled(typeof(CombatSystemGroup),gameState.Value.HasFlag(GameState.Combat));

            OnGameStateChange?.Invoke(gameState.Value);
        }

        private void SetGroupEnabled(Type type,bool enabled)
        {
            var group = World.GetExistingSystemManaged(type);
            group.Enabled = enabled;
        }
    }
}