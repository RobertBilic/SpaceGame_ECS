using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void OnSelection(int index);

public class SelectionContextMenu : MonoBehaviour
{
    [SerializeField]
    private SelectionContextMenuEntry entryPrefab;
    [SerializeField]
    private LayoutGroup layoutGroup;

    public void SetOptions(List<string> strings, OnSelection onSelection)
    {
        for(int i=0;i<strings.Count;i++)
        {
            int iCopy = i;
            var str = strings[i];
            var entry = GameObject.Instantiate(entryPrefab, layoutGroup.transform);
            entry.SetText(str);
            entry.SetAction(()=>
            {
                onSelection(iCopy);
                Destroy(gameObject);
            });
        }

        StartCoroutine(ResizeContent());
    }

    private IEnumerator ResizeContent()
    {
        yield return new WaitForEndOfFrame();

        var rect = layoutGroup.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(layoutGroup.preferredWidth, layoutGroup.preferredHeight);
    }
   

}
