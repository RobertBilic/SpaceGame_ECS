using SpaceGame.Game.State.Component;
using UnityEngine;

public delegate void OnUIShown();
public delegate void OnUIHide();

public abstract class GameStateUIController : MonoBehaviour
{
    public abstract GameState GetRequiredGameState();
    public abstract void Show(OnUIShown onShow = null);

    public abstract void Hide(OnUIHide onHide = null);
}
