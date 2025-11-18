using UnityEngine;
using System;

public class SavableObject : MonoBehaviour
{
    public string uniqueId;

    public GameObject prefab;

    private void Awake()
    {
        // Ensure a unique ID exists
        if (string.IsNullOrEmpty(uniqueId))
            uniqueId = Guid.NewGuid().ToString();
    }

   
    public ObjectSaveData CaptureState()
    {
        return new ObjectSaveData
        {
            id = uniqueId,

            
            prefabName = prefab != null ? prefab.name : gameObject.name,

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