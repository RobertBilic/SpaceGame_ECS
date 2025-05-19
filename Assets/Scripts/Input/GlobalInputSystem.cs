using SpaceGame.Game.State.Component;
using System;
using Unity.Burst;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
partial class GlobalInputSystem : SystemBase
{
    private InputSystem_Actions inputActions;


    protected override void OnCreate()
    {
        inputActions = new InputSystem_Actions();
    }

    protected override void OnStopRunning()
    {
        inputActions.Disable();
        inputActions.Gameplay.StateChange.performed -= StateChange_performed;
    }

    protected override void OnStartRunning()
    {
        inputActions.Enable();
        inputActions.Gameplay.StateChange.performed += StateChange_performed;
    }

    protected override void OnUpdate()
    {

    }

    private void StateChange_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        var value = (byte)obj.ReadValue<float>();

        if (!Enum.IsDefined(typeof(GameState), value))
        {
            UnityEngine.Debug.LogWarning($"GameState not defined at value {value}");
            return;
        }

        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(entity, new ChangeGameStateRequest() { Value = (GameState)value });
    }
}
