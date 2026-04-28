using UnityEngine;
using System;

[ExecuteAlways]
public class SavableObject : MonoBehaviour
{
    [SerializeField] public string uniqueId;

    public GameObject prefab;
    public string prefabName;

    public bool isPresetObject = false;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Give editor-placed objects a stable ID
        if (string.IsNullOrEmpty(uniqueId))
        {
            uniqueId = Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        if (prefab != null && string.IsNullOrEmpty(prefabName))
        {
            prefabName = prefab.name;
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif

    private void Awake()
    {
        if (!isPresetObject && string.IsNullOrEmpty(uniqueId))
        {
            GenerateNewId();
        }

        if (prefab != null && string.IsNullOrEmpty(prefabName))
        {
            prefabName = prefab.name;
        }
    }

    private string GetCleanPrefabName()
    {
        string name = !string.IsNullOrEmpty(prefabName)
            ? prefabName
            : (prefab != null ? prefab.name : gameObject.name);

        name = name.Replace("(Clone)", "").Trim();

        return name;
    }

    public void GenerateNewId()
    {
        uniqueId = Guid.NewGuid().ToString();
    }

    public ObjectSaveData CaptureState()
    {
        return new ObjectSaveData
        {
            id = uniqueId,
            prefabName = GetCleanPrefabName(),

            px = transform.position.x,
            py = transform.position.y,
            pz = transform.position.z,

            rx = transform.eulerAngles.x,
            ry = transform.eulerAngles.y,
            rz = transform.eulerAngles.z,

            active = gameObject.activeSelf,

            isPresetObject = isPresetObject
        };
    }
}