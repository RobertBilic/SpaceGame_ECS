using UnityEngine;

public class FleetManagementUIController : GenericGameStateUIController<FleetManagementStateUI>
{
    [SerializeField]
    private FleetManager manager;

    protected override void OnEnableInternal()
    {
        manager.Enable();
        ui.SetContent(manager.GetOwnedShips(), manager.ShowShip);
    }

    protected override void OnDisabledInternal()
    {
        manager.Disable();
        manager.Clear();
    }
}
