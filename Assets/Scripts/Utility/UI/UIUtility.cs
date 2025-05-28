using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class UIUtility 
{
    public static bool IsPointerOverUI(EventSystem eventSystem, GraphicRaycaster raycaster, Vector2 screenPos)
    {
        var pointerData = new PointerEventData(eventSystem)
        {
            position = screenPos
        };

        var results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);
        return results.Count > 0;
    }
}
