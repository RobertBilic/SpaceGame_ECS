using SpaceGame.Game.State.Component;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelSelectionUIState : GameStateUI
{
    [SerializeField]
    private Button playButton;

    public void SetOnPlayButton(UnityAction action)
    {
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(action);
    }

    public override GameState GetRequiredGameState() => GameState.LevelSelection;
}
