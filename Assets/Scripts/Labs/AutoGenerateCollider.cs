using UnityEngine;

public class AutoBoxColliderGenerator : MonoBehaviour
{
    public int slices = 3;

    [ContextMenu("Generate Colliders")]
    void Generate()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (!mf) return;

        Bounds b = mf.sharedMesh.bounds;

        Vector3 size = b.size;
        Vector3 center = b.center;

        float step = size.x / slices;

        for (int i = 0; i < slices; i++)
        {
            GameObject colObj = new GameObject("BoxCollider_" + i);
            colObj.transform.SetParent(transform, false);

            BoxCollider bc = colObj.AddComponent<BoxCollider>();

            float x = center.x - size.x / 2f + step * i + step / 2f;

            bc.center = new Vector3(x, center.y, center.z);
            bc.size = new Vector3(step, size.y, size.z);
        }
    }
}