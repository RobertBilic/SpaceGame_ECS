using SpaceGame.Game.State.Component;
using UnityEngine;

public delegate void OnUIShown();
public delegate void OnUIHide();

public class GameStateUIController : MonoBehaviour
{
    [SerializeField]
    GameStateUI ui;

    public GameState GetRequiredGameState() => ui.GetRequiredGameState();
    public virtual void Show(OnUIShown onShow = null)
    {
        ui.gameObject.SetActive(true);
        onShow?.Invoke();
    }
    public virtual void Hide(OnUIHide onHide = null)
    {
        ui.gameObject.SetActive(false);
        onHide?.Invoke();
    }
}
