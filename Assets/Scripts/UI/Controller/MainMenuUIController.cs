using SpaceGame.Game.State.Component;
using Unity.Entities;
using UnityEngine;

public class MainMenuUIController : GenericGameStateUIController<MainMenuUI>
{
    private void Awake()
    {
        ui.SetOnPlayAction(OnPlay);    
    }

    private void OnPlay()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var entity = em.CreateEntity();
        em.AddComponentData(entity, new ChangeGameStateRequest() { Value = GameState.Combat });
    }
}
