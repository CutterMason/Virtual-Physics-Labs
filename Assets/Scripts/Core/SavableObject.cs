using UnityEngine;
using System;

[ExecuteAlways]
public class SavableObject : MonoBehaviour
{
    [SerializeField] public string uniqueId;

    public GameObject prefab;
    public string prefabName;

    // Preset = placed in the scene in editor (not spawned at runtime)
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
        // Runtime: spawned objects might not have an ID yet, so generate one.
        // Preset objects should already have one from the editor.
        if (!isPresetObject && string.IsNullOrEmpty(uniqueId))
            uniqueId = Guid.NewGuid().ToString();

        if (prefab != null && string.IsNullOrEmpty(prefabName))
            prefabName = prefab.name;
    }

    public ObjectSaveData CaptureState()
    {
        return new ObjectSaveData
        {
            id = uniqueId,
            prefabName = !string.IsNullOrEmpty(prefabName)
                ? prefabName
                : (prefab != null ? prefab.name : gameObject.name),

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