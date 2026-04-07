using UnityEngine;
using UnityEngine.EventSystems;

public class PropertyManagerV2 : MonoBehaviour
{
    [Header("References")]
    public Canvas canvas;
    public GameObject propertyPanelPrefab;
    public GameObject selectedObject; //this will allow UIProp to pull current sliders state from previous changes

    private GameObject currentPanel;
    private PropertyPanelV2 currentPanelScript;
    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.15f;
    private GameObject lastclickedObject;


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
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            float timeSinceLastClick = Time.time - lastClickTime;
            //will use ray as this is going to detect if the same object is clicked 2x
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject clickedObj = hit.collider.gameObject;

                // TRUE double-click past if statements too many met criteria for "double-click"
                if (clickedObj == lastclickedObject &&
                    timeSinceLastClick <= doubleClickThreshold)
                {
                    var physicsObj = clickedObj.GetComponentInParent<PhysicsObject>();
                    if (physicsObj != null)
                    {
                        selectedObject = physicsObj.gameObject;
                        OpenPropertyPanel(selectedObject);
                    }
                    lastclickedObject = null;
                }
                else
                {
                    // Only update lastclickedObject if NOT a double-click
                    lastclickedObject = clickedObj;
                }
            }

            lastClickTime = Time.time;
    }
}


    private void OpenPropertyPanel(GameObject obj)
    {
        if (currentPanel == null)
        {
            currentPanel = Instantiate(propertyPanelPrefab, canvas.transform);
            currentPanelScript = currentPanel.GetComponent<PropertyPanelV2>();
        }

        currentPanel.SetActive(true);

        currentPanelScript.Open(obj, this);
    }
    public void CloseCurrentPanel()
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);  // Keep the prefab alive just inactive
        }
    }
}