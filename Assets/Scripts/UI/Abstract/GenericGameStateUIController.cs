using SpaceGame.Game.State.Component;
using UnityEngine;

public abstract class GenericGameStateUIController<T> : GameStateUIController
        where T : GameStateUI
{
    [SerializeField]
    protected T ui;

    public override GameState GetRequiredGameState() => ui.GetRequiredGameState();

    public override void Hide(OnUIHide onHide = null)
    {
        ui.gameObject.SetActive(false);
        OnDisabledInternal();
        onHide?.Invoke();
    }

    public override void Show(OnUIShown onShow = null)
    {
        ui.gameObject.SetActive(true);
        OnEnableInternal();
        onShow?.Invoke();
    }

    protected virtual void OnEnableInternal()
    {

    }

    protected virtual void OnDisabledInternal()
    {

    }
}
