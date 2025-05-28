using SpaceGame.Game.State.Component;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FleetManagementUIController : GenericGameStateUIController<FleetManagementStateUI>
{
    [SerializeField]
    private FleetManager manager;

    protected override void OnEnableInternal()
    {
        manager.OnTurretSlotSelected += OnTurretSlotClicked;
        manager.Enable();

        ui.SetContent(manager.GetOwnedShips(), manager.ShowShip);
        ui.SetOnBackAction(BackToMainMenu);
    }

    protected override void OnDisabledInternal()
    {
        manager.OnTurretSlotSelected -= OnTurretSlotClicked;
        
        manager.Disable();
        manager.Clear();
    }

    private void OnTurretSlotClicked(Vector3 screenPos, int slotIndex, int currentlySelected, List<string> turretIds)
    {
        if (UIUtility.IsPointerOverUI(EventSystem.current, ui.GetComponentInParent<GraphicRaycaster>(), screenPos))
            return;

        ui.ShowTurretSelection(screenPos, slotIndex, currentlySelected, turretIds, manager.AddTurret);
    }

    private void BackToMainMenu()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var entity = em.CreateEntity();
        em.AddComponentData(entity, new ChangeGameStateRequest() { Value = GameState.MainMenu });
    }

}
