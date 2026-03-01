using System.Collections.Generic;
using UnityEngine;

public class PrefabRegistry : MonoBehaviour
{
    public static PrefabRegistry Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // optional: keep registry across scene loads

        Debug.Log("[PrefabRegistry] Initialized with " + spawnablePrefabs.Count + " prefab(s).");
    }

    public List<GameObject> spawnablePrefabs;

    public GameObject GetPrefab(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName))
            return null;

        prefabName = prefabName.Replace("(Clone)", "").Trim();

        var prefab = spawnablePrefabs.Find(p => p != null && p.name == prefabName);
        if (prefab == null)
            Debug.LogError("[PrefabRegistry] No prefab found with name: " + prefabName);
        return prefab;
    }
}