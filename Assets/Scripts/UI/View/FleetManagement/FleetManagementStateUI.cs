using SpaceGame.Game.State.Component;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FleetManagementStateUI : GameStateUI
{
    [SerializeField]
    private Button backButton;
    [SerializeField]
    private LayoutGroup content;
    [SerializeField]
    private FleetManagementShipEntryUI shipEntryPrefab;
    [SerializeField]
    private SelectionContextMenu contextMenuPrefab;

    private SelectionContextMenu activeContextMenu;

    public override GameState GetRequiredGameState() => GameState.FleetManagement;

    public void SetContent(List<ShipLoadout> loadout, OnShipSelected onShipSelected)
    {
        //TODO: Refactor to not use the original ShipLoadout class, it's missing a sprite for the UI

        foreach (Transform transform in content.transform)
            GameObject.Destroy(transform.gameObject);

        foreach(var entry in loadout)
        {
            var shipEntry = GameObject.Instantiate(shipEntryPrefab, content.transform);
            //TODO: Add localization to the id
            shipEntry.SetTitle(entry.ShipId);
            shipEntry.SetOnClickAction(() => onShipSelected.Invoke(entry.LocalId));
        }

        StartCoroutine(ResizeContent(content));
    }

    public void ShowTurretSelection(Vector3 screenPosition, int slotIndex, int currentlySelected, List<string> turretIds, OnTurretSelected onTurretSelected)
    {
        if (activeContextMenu != null)
            Destroy(activeContextMenu.gameObject);

        activeContextMenu = GameObject.Instantiate(contextMenuPrefab, transform);
        activeContextMenu.transform.position = screenPosition;

        activeContextMenu.SetOptions(turretIds, (ind) =>
        {
            var str = turretIds[ind];
            onTurretSelected?.Invoke(slotIndex, str);
        });
    }

    private IEnumerator ResizeContent(LayoutGroup group)
    {
        yield return new WaitForEndOfFrame();

        var rectTransform = group.transform as RectTransform;
        rectTransform.sizeDelta = new Vector2(group.preferredWidth, group.preferredHeight);
    }

    public void SetOnBackAction(UnityAction action)
    {
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(action);
    }
}
