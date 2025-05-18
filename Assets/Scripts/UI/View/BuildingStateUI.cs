using SpaceGame.Game.State.Component;
using UnityEngine;

public class BuildingStateUI : GameStateUI
{
    public override GameState GetRequiredGameState() => GameState.Building;
}
