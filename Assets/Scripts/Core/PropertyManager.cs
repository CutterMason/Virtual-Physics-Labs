using UnityEngine;
using UnityEngine.EventSystems;

public class PropertyEditorManager : MonoBehaviour
{
    [Header("References")]
    public Canvas canvas;
    public GameObject propertyPanelPrefab;
    public GameObject selectedObject; //this will allow UIProp to pull current sliders state from previous changes

    private GameObject currentPanel;
    private PropertyPanel currentPanelScript;


    private void Start()
    {
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found in the scene!");
            }
        }

        if (propertyPanelPrefab == null)
        {
            Debug.LogError("No PropertyPanel Prefab assigned!");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1)) //this is used for the right click option
        {
            // UI will not recongize clicks outide of it
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // need to look until parent is found
                var physicsObj = hit.collider.GetComponentInParent<PhysicsObject>();
                if (physicsObj != null)
                {
                    selectedObject = physicsObj.gameObject;
                    OpenPropertyPanel(selectedObject);
                }
                else
                {
                    Debug.Log("Clicked object has no PhysicsObject component.");
                }
            }
        }
    }


    private void OpenPropertyPanel(GameObject obj)
    {
        // If we don’t already have a panel, create one
        if (currentPanel == null)
        {
            currentPanel = Instantiate(propertyPanelPrefab, canvas.transform);
            currentPanelScript = currentPanel.GetComponent<PropertyPanel>();
        }

        // Make sure the panel is visible, canvas is active but the panel child within is not
        currentPanel.SetActive(true);

        // Pass selected object and manager to the panel
        currentPanelScript.Open(obj, this);
    }
    public void CloseCurrentPanel()
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);  // Keep the prefab alive
        }
    }
}