using SpaceGame.Game.State.Component;
using SpaceGame.Game.State.Systems;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private const string TAG = "UIController <-> ";

    [SerializeField]
    private List<GameStateUIController> uiControllers;

    private GameStateUIController activeController;

    private void Awake()
    {
        foreach (var ui in uiControllers)
            ui.Hide();
    }

    private void Start()
    {
        var system = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<GameStateSystem>();
        system.OnGameStateChange += System_OnGameStateChange;
    }

    private void System_OnGameStateChange(SpaceGame.Game.State.Component.GameState state)
    {
        ShowUI(state);
    }

    public void ShowUI(GameState state)
    {
        var controller = uiControllers.Find(x => x.GetRequiredGameState() == state);

        if (controller == null)
        {
            Debug.LogWarning($"{TAG} UI with state {state.ToString()} doesn't exist");
            activeController?.Hide();
            return;
        }

        OnUIHide loadAction = () => { controller.Show(); };

        if(activeController != null)
            activeController.Hide(loadAction);
        else
            loadAction.Invoke();

        activeController = controller;
    }
}
