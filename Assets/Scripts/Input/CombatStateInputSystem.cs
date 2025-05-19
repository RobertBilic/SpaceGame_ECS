using SpaceGame.Combat.Components;
using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(CombatInitializationGroup), OrderFirst = true)]
partial class CombatStateInputSystem : SystemBase
{
    private InputSystem_Actions inputActions;

    protected override void OnCreate()
    {
        inputActions = new InputSystem_Actions();
    }

    protected override void OnStartRunning()
    {
        inputActions.Enable();
        inputActions.Gameplay.TimeManipulation.performed += TimeManipulation_performed;
    }

    protected override void OnStopRunning()
    {
        inputActions.Disable();
        inputActions.Gameplay.TimeManipulation.performed -= TimeManipulation_performed;
    }

    protected override void OnUpdate()
    {

    }

    private void TimeManipulation_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        var value = obj.ReadValue<float>();
        var entity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(entity, new TimeScaleChangeRequest() { Value = value });
    }
}
