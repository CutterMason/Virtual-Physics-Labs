using System.Collections.Generic;
using UnityEngine;

public class PrefabRegistry : MonoBehaviour
{
    public static PrefabRegistry Instance;

    private void Awake()
    {
        Instance = this;
        Debug.Log("[PrefabRegistry] Initialized with " + spawnablePrefabs.Count + " prefab(s).");
    }

    public List<GameObject> spawnablePrefabs;

    public GameObject GetPrefab(string prefabName)
    {
        var prefab = spawnablePrefabs.Find(p => p.name == prefabName);
        if (prefab == null)
            Debug.LogError("[PrefabRegistry] No prefab found with name: " + prefabName);
        return prefab;
    }
}