using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectionContextMenuEntry : MonoBehaviour
{
    [SerializeField]
    private Button button;
    [SerializeField]
    private TextMeshProUGUI titleText;

    public void SetText(string txt)
    {
        //TODO: Add localization instead of raw text
        titleText.text = txt;
    }

    public void SetAction(UnityAction action)
    {
        button?.onClick.AddListener(action);
    }
}
