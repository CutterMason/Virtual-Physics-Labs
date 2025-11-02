using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SpringVisual : MonoBehaviour
{
    public Transform targetPoint;    // The anchor (top of the spring)
    public int coilCount = 10;       // Number of coils
    public float coilRadius = 0.2f;  // Width of the spring
    public int pointsPerCoil = 20;   // Higher = smoother

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (targetPoint == null) return;
        DrawSpring(targetPoint.position, transform.position);
    }

    void DrawSpring(Vector3 start, Vector3 end)
    {
        Vector3 dir = end - start;
        float length = dir.magnitude;
        dir.Normalize();

        // Build a coordinate system perpendicular to the spring
        Vector3 up = Vector3.Cross(dir, Vector3.up);
        if (up.sqrMagnitude < 0.001f)
            up = Vector3.Cross(dir, Vector3.right);
        up.Normalize();

        Vector3 right = Vector3.Cross(dir, up);

        int totalPoints = coilCount * pointsPerCoil + 1;
        Vector3[] positions = new Vector3[totalPoints];

        for (int i = 0; i < totalPoints; i++)
        {
            float t = (float)i / (totalPoints - 1);
            float angle = t * coilCount * 2f * Mathf.PI;

            // Generate a circular coil around the axis
            Vector3 circularOffset = (Mathf.Sin(angle) * up + Mathf.Cos(angle) * right) * coilRadius;

            positions[i] = Vector3.Lerp(start, end, t) + circularOffset;
        }

        lineRenderer.positionCount = totalPoints;
        lineRenderer.SetPositions(positions);
    }
}