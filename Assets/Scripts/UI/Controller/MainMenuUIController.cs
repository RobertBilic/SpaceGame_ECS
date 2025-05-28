using SpaceGame.Game.State.Component;
using Unity.Entities;
using UnityEngine;

public class MainMenuUIController : GenericGameStateUIController<MainMenuUI>
{
    private void Awake()
    {
        ui.SetOnFleetManagementAction(() => ChangeState(GameState.FleetManagement));
        ui.SetOnPlayAction(() => ChangeState(GameState.LevelSelection));     
    }

    private void ChangeState(GameState state)
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var entity = em.CreateEntity();
        em.AddComponentData(entity, new ChangeGameStateRequest() { Value = state});
    }
}
