using SpaceGame.Game.State.Component;
using UnityEngine;

public abstract class GameStateUI : MonoBehaviour
{
    public abstract GameState GetRequiredGameState();
}
