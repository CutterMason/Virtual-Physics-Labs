using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class CyclerItem
{
    public GameObject item;
    public string displayText;
}

public class PrefabCycler : MonoBehaviour
{
    [Header("Items")]
    public List<CyclerItem> items = new List<CyclerItem>();

    [Header("Positions")]
    public Transform displayPoint;
    public Transform hiddenPoint;

    [Header("UI")]
    public TMP_Text uiText; 

    private int currentIndex = -1;

    void Start()
    {
        foreach (CyclerItem entry in items)
        {
            if (entry.item != null)
                entry.item.transform.position = hiddenPoint.position;
        }
    }

    public void CycleNext()
    {
        if (items.Count == 0) return;

        if (currentIndex >= 0 && currentIndex < items.Count)
        {
            var current = items[currentIndex];
            if (current.item != null)
                current.item.transform.position = hiddenPoint.position;
        }

        currentIndex = (currentIndex + 1) % items.Count;

        var next = items[currentIndex];

        if (next.item != null)
            next.item.transform.position = displayPoint.position;

        if (uiText != null)
            uiText.text = next.displayText;
    }
}