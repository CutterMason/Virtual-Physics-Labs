using UnityEngine;

public class GraphPanelController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Assign all graph panels here. Index 0 = first graph, 1 = second graph, etc.")]
    public GameObject[] graphPanels;

    private bool[] isOpen;

    void Start()
    {
        if (graphPanels == null) return;

        isOpen = new bool[graphPanels.Length];

        for (int i = 0; i < graphPanels.Length; i++)
        {
            if (graphPanels[i] != null)
                graphPanels[i].SetActive(false);
        }
    }

    public void ToggleGraphPanel(int index)
    {
        if (graphPanels == null || index < 0 || index >= graphPanels.Length)
            return;

        CloseAllExcept(index);

        isOpen[index] = !isOpen[index];

        if (graphPanels[index] != null)
            graphPanels[index].SetActive(isOpen[index]);
    }
    
    private void CloseAllExcept(int keepIndex)
    {
        for (int i = 0; i < graphPanels.Length; i++)
        {
            if (i == keepIndex) continue;

            isOpen[i] = false;

            if (graphPanels[i] != null)
                graphPanels[i].SetActive(false);
        }
    }
}
