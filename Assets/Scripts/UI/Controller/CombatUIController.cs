using SpaceGame.Combat.Components;
using Unity.Entities;
using UnityEngine;

public delegate void OnTimeChangeButtonPressed(float value);

public class CombatUIController : GenericGameStateUIController<CombatStateUI>
{
    private void Awake()
    {
        ui.SetSpeedChangeAction(OnTimeChangeButtonPressed);    
    }

    private void Update()
    {
        SetFPS(1.0f / Time.deltaTime);
    }

    private void SetFPS(float fps)
    {
        ui.SetFPS(fps);
    }

    private void OnTimeChangeButtonPressed(float value)
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var entity = em.CreateEntity();
        em.AddComponentData(entity, new TimeScaleChangeRequest() { Value = value });
    }
}
