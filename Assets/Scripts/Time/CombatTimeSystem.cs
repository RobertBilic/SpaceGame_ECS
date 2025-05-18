using Unity.Entities;

[UpdateInGroup(typeof(CombatSystemGroup), OrderFirst = true)]
partial class CombatTimeSystem : SystemBase
{
    private InputSystem_Actions inputActions;
    public float timeMultiplier;
    public float lastNonZeroTimeMultiplier;

    protected override void OnCreate()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Gameplay.TimeManipulation.performed += TimeManipulation_performed;
        lastNonZeroTimeMultiplier = 1.0f;
        timeMultiplier = 1.0f;
    }

    protected override void OnUpdate()
    {
        if (!SystemAPI.TryGetSingletonRW<GlobalTimeComponent>(out var timeComponent))
            return;

        timeComponent.ValueRW.FrameCount++;
        timeComponent.ValueRW.FrameCountScaled += (long)timeMultiplier;

        var dt = SystemAPI.Time.DeltaTime;

        timeComponent.ValueRW.ElapsedTime += dt;
        timeComponent.ValueRW.DeltaTime = dt;

        timeComponent.ValueRW.DeltaTimeScaled = dt * timeMultiplier;
        timeComponent.ValueRW.ElapsedTimeScaled += timeComponent.ValueRO.DeltaTimeScaled;
    }

    protected override void OnStartRunning()
    {
        inputActions?.Enable();
    }

    protected override void OnStopRunning()
    {
        inputActions?.Disable();
    }

    private void TimeManipulation_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        var value = obj.ReadValue<float>();

        if (SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeScale))
        {
            if (value != 0.0f)
                lastNonZeroTimeMultiplier = value;

            if (value == 0.0f && timeMultiplier == 0.0f)
                value = lastNonZeroTimeMultiplier;

            timeMultiplier = value;
        }
    }

}
