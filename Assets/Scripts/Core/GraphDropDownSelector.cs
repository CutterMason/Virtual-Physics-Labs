using UnityEngine;

public class GraphDropdownController : MonoBehaviour
{
    [Header("Dropdown Panel")]
    public GameObject graphDropdownPanel;

    [Header("Graph Panels")]
    public GameObject[] graphPanels;
    // Example:
    // 0 = Position
    // 1 = Velocity
    // 2 = Acceleration

    private int currentOpenIndex = -1;

    void Start()
    {
        if (graphDropdownPanel != null)
            graphDropdownPanel.SetActive(false);

        CloseAllGraphs();
    }

    public void ToggleDropdown()
    {
        if (graphDropdownPanel == null)
            return;

        graphDropdownPanel.SetActive(!graphDropdownPanel.activeSelf);
    }

    public void ShowGraphPanel(int index)
    {
        if (graphPanels == null || index < 0 || index >= graphPanels.Length)
            return;

        CloseAllGraphs();

        if (graphPanels[index] != null)
        {
            graphPanels[index].SetActive(true);
            currentOpenIndex = index;
        }

        if (graphDropdownPanel != null)
            graphDropdownPanel.SetActive(false);
    }

    public void CloseAllGraphs()
    {
        if (graphPanels == null)
            return;

        for (int i = 0; i < graphPanels.Length; i++)
        {
            if (graphPanels[i] != null)
                graphPanels[i].SetActive(false);
        }

        currentOpenIndex = -1;
    }

    public void CloseDropdown()
    {
        if (graphDropdownPanel != null)
            graphDropdownPanel.SetActive(false);
    }
}