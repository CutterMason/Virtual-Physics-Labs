using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class TargetScoring : MonoBehaviour
{
    public float maxScore = 10f;

    [Header("Debug / Live Score")]
    [SerializeField] private float lastScore;   // <-- shows in Inspector

    private float targetRadius;

    void Start()
    {
        CalculateRadius();
    }

    void CalculateRadius()
    {
        MeshCollider meshCol = GetComponent<MeshCollider>();

        if (meshCol.sharedMesh == null)
        {
            Debug.LogError("No mesh found!");
            return;
        }

        Bounds bounds = meshCol.sharedMesh.bounds;

        float radiusY = bounds.extents.y * transform.localScale.y;
        float radiusZ = bounds.extents.z * transform.localScale.z;

        targetRadius = Mathf.Max(radiusY, radiusZ);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody == null)
            return;

        ContactPoint contact = collision.contacts[0];
        Vector3 localHitPoint = transform.InverseTransformPoint(contact.point);

        // Target faces X → use Y & Z
        Vector2 hit2D = new Vector2(localHitPoint.y, localHitPoint.z);

        float distanceFromCenter = hit2D.magnitude;

        lastScore = CalculateScore(distanceFromCenter);

        Debug.Log("Score: " + lastScore);
    }

    float CalculateScore(float distance)
    {
        if (distance > targetRadius)
            return 0;

        float normalized = distance / targetRadius;
        float inverted = 1 - normalized;

        return Mathf.Round(inverted * maxScore);
    }
}