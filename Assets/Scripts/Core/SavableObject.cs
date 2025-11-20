using UnityEngine;
using System;

public class SavableObject : MonoBehaviour
{
    public string uniqueId;

    // The original prefab that this object came from
    public GameObject prefab;

    // The name used by the save system to match the registry
    public string prefabName;

    // NEW: Marks objects that were placed in the scene manually and should NOT be saved/loaded
    public bool isPresetObject = false;

    private void Awake()
    {
        // Ensure a unique ID exists
        if (string.IsNullOrEmpty(uniqueId))
            uniqueId = Guid.NewGuid().ToString();

        // Auto-fill prefabName if prefab is assigned
        if (prefab != null && string.IsNullOrEmpty(prefabName))
            prefabName = prefab.name;
    }

    public ObjectSaveData CaptureState()
    {
        return new ObjectSaveData
        {
            id = uniqueId,

            // Use saved name or fallback to prefab
            prefabName = !string.IsNullOrEmpty(prefabName)
                     ? prefabName
                     : (prefab != null ? prefab.name : gameObject.name),

            px = transform.position.x,
            py = transform.position.y,
            pz = transform.position.z,

            rx = transform.eulerAngles.x,
            ry = transform.eulerAngles.y,
            rz = transform.eulerAngles.z,

            active = gameObject.activeSelf
        };
    }
}