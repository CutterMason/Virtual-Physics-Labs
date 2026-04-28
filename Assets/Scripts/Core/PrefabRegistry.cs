using System.Collections.Generic;
using UnityEngine;

public class PrefabRegistry : MonoBehaviour
{
    public static PrefabRegistry Instance;

    public List<GameObject> spawnablePrefabs = new List<GameObject>();

    private void Awake()
    {
        Instance = this;

        Debug.Log("[PrefabRegistry] Initialized with " + spawnablePrefabs.Count + " prefab(s).");

        foreach (GameObject prefab in spawnablePrefabs)
        {
            if (prefab != null)
            {
                Debug.Log("[PrefabRegistry] Registered prefab: " + prefab.name);
            }
        }
    }

    public GameObject GetPrefab(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName))
        {
            Debug.LogError("[PrefabRegistry] Tried to get prefab, but prefabName was empty.");
            return null;
        }

        prefabName = prefabName.Replace("(Clone)", "").Trim();

        GameObject prefab = spawnablePrefabs.Find(p => p != null && p.name == prefabName);

        if (prefab == null)
        {
            Debug.LogError("[PrefabRegistry] No prefab found with name: " + prefabName);
        }

        return prefab;
    }
}