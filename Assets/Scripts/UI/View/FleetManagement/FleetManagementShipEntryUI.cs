using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FleetManagementShipEntryUI : MonoBehaviour
{
    [SerializeField]
    private Button button;
    [SerializeField]
    private TextMeshProUGUI title;
    [SerializeField]
    private Image image;

    public void SetOnClickAction(UnityAction action)
    {
        button.onClick.AddListener(action);
    }

    public void SetTitle(string text)
    {
        title.text = text;
    }

    public void SetSprite(Sprite sprite)
    {
        image.sprite = sprite;
    }
}
