using SpaceGame.Game.State.Component;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuUI : GameStateUI
{
    [SerializeField]
    private Button playButton;

    public void SetOnPlayAction(UnityAction onPlay)
    {
        playButton.onClick.AddListener(onPlay);
    }

    public override GameState GetRequiredGameState() => GameState.MainMenu;
}
