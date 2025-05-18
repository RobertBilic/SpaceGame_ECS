using SpaceGame.Game.State.Component;
using UnityEngine;

public class MainMenuUI : GameStateUI
{
    public override GameState GetRequiredGameState() => GameState.MainMenu;
}
