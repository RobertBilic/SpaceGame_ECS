using SpaceGame.Game.State.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FleetManagementStateUI : GameStateUI
{
    [SerializeField]
    private LayoutGroup content;
    [SerializeField]
    private FleetManagementShipEntryUI shipEntryPrefab;

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

    private IEnumerator ResizeContent(LayoutGroup group)
    {
        yield return new WaitForEndOfFrame();

        var rectTransform = group.transform as RectTransform;
        rectTransform.sizeDelta = new Vector2(group.preferredWidth, group.preferredHeight);
    }
}
