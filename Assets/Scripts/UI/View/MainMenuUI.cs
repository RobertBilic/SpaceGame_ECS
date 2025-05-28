using SpaceGame.Game.State.Component;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuUI : GameStateUI
{
    [SerializeField]
    private Button playButton;
    [SerializeField]
    private Button fleetManagement;

    public void SetOnFleetManagementAction(UnityAction action)
    {
        fleetManagement.onClick.RemoveAllListeners();
        fleetManagement.onClick.AddListener(action);
    }

    public void SetOnPlayAction(UnityAction onPlay)
    {
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(onPlay);
    }

    public override GameState GetRequiredGameState() => GameState.MainMenu;
}
