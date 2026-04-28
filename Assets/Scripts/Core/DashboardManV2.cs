using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashboardManagerV2 : MonoBehaviour
{
    [System.Serializable]
    public class AssetButton
    {
        public Button button;
        public GameObject prefab;

        [Header("Spawn Limit")]
        public int maxCount = 5;

        [HideInInspector]
        public List<GameObject> spawnedObjects = new List<GameObject>();
    }

    [Header("Dashboard Settings")]
    public RectTransform dashboardPanel;
    public AssetButton[] assetButtons;
    public Transform spawnPoint;

    [Header("Edit Mode Toggle")]
    public Toggle editModeToggle;

    [Header("Animation Settings")]
    public float slideSpeed = 10f;
    private Vector2 hiddenPos;
    private Vector2 visiblePos;

    private bool isOpen = false;

    private void Start()
    {
        visiblePos = dashboardPanel.anchoredPosition;
        hiddenPos = visiblePos + new Vector2(dashboardPanel.rect.width, 0);
        dashboardPanel.anchoredPosition = hiddenPos;

        foreach (var ab in assetButtons)
        {
            if (ab.button != null && ab.prefab != null)
            {
                AssetButton capturedAB = ab;
                ab.button.onClick.AddListener(() => SpawnAsset(capturedAB));
            }
        }
    }

    private void Update()
    {
        //tracks deleted objects
        foreach (var ab in assetButtons)
        {
            ab.spawnedObjects.RemoveAll(obj => obj == null);
        }

        Vector2 targetPos = isOpen ? visiblePos : hiddenPos;
        dashboardPanel.anchoredPosition = Vector2.Lerp(
            dashboardPanel.anchoredPosition,
            targetPos,
            Time.unscaledDeltaTime * slideSpeed
        );
    }

    public void ToggleDashboard()
    {
        isOpen = !isOpen;
    }

    public void SpawnAsset(AssetButton asset)
    {
        //only spawn when edit mode it toggled
        if (editModeToggle != null && !editModeToggle.isOn)
        {
            Debug.LogWarning("Cannot spawn: Edit Mode is OFF");
            return;
        }

        if (asset == null || asset.prefab == null) return;

        asset.spawnedObjects.RemoveAll(obj => obj == null);

        if (asset.spawnedObjects.Count >= asset.maxCount)
        {
            Debug.LogWarning($"{asset.prefab.name} limit reached!");
            return;
        }

        Vector3 spawnPos;
        if (spawnPoint != null)
            spawnPos = spawnPoint.position;
        else if (Camera.main != null)
            spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 1f;
        else
            spawnPos = Vector3.zero;

        GameObject obj = Instantiate(asset.prefab, spawnPos, Quaternion.identity);

        asset.spawnedObjects.Add(obj);

        SavableObject so = obj.GetComponent<SavableObject>();

        if (so == null)
        {
            so = obj.GetComponentInChildren<SavableObject>();
        }

        if (so != null)
        {
            so.GenerateNewId();

            so.prefab = asset.prefab;
            so.prefabName = asset.prefab.name;
            so.isPresetObject = false;

            Debug.Log($"Spawned savable object: {so.prefabName}, ID: {so.uniqueId}");
        }
        else
        {
            Debug.LogError($"Spawned object {asset.prefab.name} is missing SavableObject component!");
        }
    }
}