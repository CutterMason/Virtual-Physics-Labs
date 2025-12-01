using UnityEngine;

public class GraphPanelController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Assign all graph panels here. Index 0 = first graph, 1 = second graph, etc.")]
    public GameObject[] graphPanels;

    // internal state for each graph
    private bool[] isOpen;

    void Start()
    {
        if (graphPanels == null) return;

        // initialize state array
        isOpen = new bool[graphPanels.Length];

        // ensure all panels start hidden by default
        for (int i = 0; i < graphPanels.Length; i++)
        {
            if (graphPanels[i] != null)
                graphPanels[i].SetActive(false);
        }
    }

    // Called by UI Buttons
    // Each button passes the index of the graph panel it controls
    public void ToggleGraphPanel(int index)
    {
        if (graphPanels == null || index < 0 || index >= graphPanels.Length)
            return;

        // toggle this panel
        isOpen[index] = !isOpen[index];

        if (graphPanels[index] != null)
            graphPanels[index].SetActive(isOpen[index]);
    }
}
