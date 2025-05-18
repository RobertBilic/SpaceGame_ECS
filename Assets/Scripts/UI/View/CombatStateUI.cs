using SpaceGame.Game.State.Component;
using UnityEngine;

public class CombatStateUI : GameStateUI
{
    public override GameState GetRequiredGameState() => GameState.Combat;
}
