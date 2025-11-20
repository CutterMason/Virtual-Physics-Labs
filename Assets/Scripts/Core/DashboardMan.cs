using UnityEngine;
using UnityEngine.UI;

public class DashboardManager : MonoBehaviour
{
    [System.Serializable]
    public class AssetButton
    {
        public Button button;
        public GameObject prefab;
    }

    [Header("Dashboard Settings")]
    public RectTransform dashboardPanel;  // The panel to slide in/out
    public AssetButton[] assetButtons;    // Asset buttons
    public Transform spawnPoint;          // Optional spawn location

    [Header("Animation Settings")]
    public float slideSpeed = 10f;        // Speed of slide
    private Vector2 hiddenPos;
    private Vector2 visiblePos;

    private bool isOpen = false; //changes to true once panel is slide out

    private void Start()
    {
        // Save positions
        visiblePos = dashboardPanel.anchoredPosition;
        hiddenPos = visiblePos + new Vector2(dashboardPanel.rect.width, 0); // off-screen to the right
        dashboardPanel.anchoredPosition = hiddenPos; // start hidden

        foreach (var ab in assetButtons)
        {
            if (ab.button != null && ab.prefab != null)
            {
                ab.button.onClick.AddListener(() => SpawnAsset(ab.prefab));
            }
        }

    }

    private void Update()
    {
        // Slide
        Vector2 targetPos = isOpen ? visiblePos : hiddenPos;
        dashboardPanel.anchoredPosition = Vector2.Lerp(dashboardPanel.anchoredPosition, targetPos, Time.deltaTime * slideSpeed);
    }

    // Called by the toggle button
    public void ToggleDashboard()
    {
        isOpen = !isOpen;
    }

    // Spawns the asset in front of the camera or at spawnPoint
    public void SpawnAsset(GameObject prefab)
    {
        if (prefab == null) return;

        Vector3 spawnPos;
        if (spawnPoint != null)
            spawnPos = spawnPoint.position;
        else if (Camera.main != null)
            spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 5f;
        else
            spawnPos = Vector3.zero;

       
        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

        
        SavableObject so = obj.GetComponent<SavableObject>();
        if (so != null)
        {
            so.prefab = prefab;          // Assign original prefab
            so.prefabName = prefab.name; // Ensure correct lookup name
        }
        else
        {
            Debug.LogError($"Spawned object {prefab.name} is missing SavableObject component!");
        }
    }
}
