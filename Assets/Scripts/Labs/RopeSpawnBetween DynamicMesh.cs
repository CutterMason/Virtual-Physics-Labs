using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RopeSpawnBetweenDynamicMesh : MonoBehaviour
{
    [Header("Rope Setup")]
    [SerializeField] GameObject partPrefab;
    [SerializeField] Transform startObject;
    [SerializeField] Transform endObject;
    [SerializeField] Transform parentObject;

    [Header("Rope Physics")]
    [SerializeField, Range(0.01f, 1f)] float partDistance = 0.2f;
    [SerializeField] bool snapFirst = false;
    [SerializeField] bool snapLast = false;

    [Header("Dynamic Update")]
    [SerializeField] bool autoUpdate = true;
    [SerializeField] float updateThreshold = 0.1f;
    [SerializeField] float checkInterval = 0.25f;

    [Header("Visual Settings")]
    [SerializeField] float ropeRadius = 0.05f;
    [SerializeField] int radialSegments = 8;
    [SerializeField] Material ropeMaterial;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private float lastDistance = 0f;
    private List<GameObject> ropeParts = new List<GameObject>();

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (ropeMaterial)
            meshRenderer.material = ropeMaterial;
    }

    void Start()
    {
        if (startObject && endObject)
        {
            SpawnRope();
            if (autoUpdate)
                StartCoroutine(CheckDistanceRoutine());
        }
    }

    IEnumerator CheckDistanceRoutine()
    {
        while (autoUpdate)
        {
            float currentDist = Vector3.Distance(startObject.position, endObject.position);
            if (Mathf.Abs(currentDist - lastDistance) > updateThreshold)
                SpawnRope();
            yield return new WaitForSeconds(checkInterval);
        }
    }

    void Update()
    {
        if (ropeParts.Count > 0)
            UpdateRopeMesh();
    }

    public void SpawnRope()
    {
        if (!startObject || !endObject || !partPrefab) return;

        // Destroy old rope
        foreach (var part in ropeParts)
            if (part) Destroy(part);
        ropeParts.Clear();

        Vector3 startPos = startObject.position;
        Vector3 endPos = endObject.position;
        Vector3 direction = (endPos - startPos);
        float totalDistance = direction.magnitude;
        direction.Normalize();

        int count = Mathf.Max(2, Mathf.RoundToInt(totalDistance / partDistance));
        lastDistance = totalDistance;

        Rigidbody previousBody = startObject.GetComponent<Rigidbody>();

        for (int i = 0; i < count; i++)
        {
            float t = (i + 0.5f) / count;
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);

            GameObject segment = Instantiate(partPrefab, pos, Quaternion.identity, parentObject);
            segment.name = $"RopePart_{i}";
            ropeParts.Add(segment);

            Rigidbody rb = segment.GetComponent<Rigidbody>();
            CharacterJoint joint = segment.GetComponent<CharacterJoint>();

            if (joint != null && previousBody != null)
            {
                joint.connectedBody = previousBody;
                joint.anchor = new Vector3(0, -0.5f * partDistance, 0); // bottom of this segment
                joint.connectedAnchor = new Vector3(0, 0.5f * partDistance, 0); // top of previous
                joint.autoConfigureConnectedAnchor = false;
            }

            if (i == 0 && snapFirst)
                rb.constraints = RigidbodyConstraints.FreezeAll;

            previousBody = rb; // update for next segment
        }

        // Attach last segment to endObject without breaking chain
        Rigidbody lastSegmentRb = ropeParts[ropeParts.Count - 1].GetComponent<Rigidbody>();
        if (endObject.GetComponent<Rigidbody>() == null)
        {
            Rigidbody endRb = endObject.gameObject.AddComponent<Rigidbody>();
            endRb.isKinematic = true;
        }

        CharacterJoint lastSegmentJoint = lastSegmentRb.GetComponent<CharacterJoint>();
        if (lastSegmentJoint != null)
        {
            lastSegmentJoint.connectedBody = endObject.GetComponent<Rigidbody>();
            lastSegmentJoint.anchor = new Vector3(0, 0.5f * partDistance, 0); // top of last segment
            lastSegmentJoint.connectedAnchor = Vector3.zero; // center of endObject
            lastSegmentJoint.autoConfigureConnectedAnchor = false;
        }

        if (snapLast)
            lastSegmentRb.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void UpdateRopeMesh()
    {
        // Collect rope positions
        List<Vector3> points = new List<Vector3>();
        points.Add(startObject.position);
        foreach (var seg in ropeParts)
            if (seg != null)
                points.Add(seg.transform.position);
        points.Add(endObject.position);

        meshFilter.mesh = GenerateTubeMesh(points, ropeRadius, radialSegments);
    }

    private Mesh GenerateTubeMesh(List<Vector3> points, float radius, int segmentsAround)
    {
        Mesh mesh = new Mesh();

        if (points.Count < 2)
            return mesh;

        int vertexCount = points.Count * segmentsAround;
        int triangleCount = (points.Count - 1) * segmentsAround * 2 * 3; // 2 triangles per quad
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[triangleCount];

        float totalLength = 0f;
        for (int i = 1; i < points.Count; i++)
            totalLength += Vector3.Distance(points[i - 1], points[i]);

        float vCoord = 0f;
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 forward = (i == points.Count - 1) ? 
                (points[i] - points[i - 1]).normalized : 
                (points[i + 1] - points[i]).normalized;

            Vector3 up = Vector3.up;
            if (Vector3.Dot(forward, up) > 0.9f)
                up = Vector3.right;

            Vector3 right = Vector3.Cross(forward, up).normalized;
            up = Vector3.Cross(right, forward).normalized;

            for (int j = 0; j < segmentsAround; j++)
            {
                float angle = (j / (float)segmentsAround) * Mathf.PI * 2;
                Vector3 circlePos = right * Mathf.Cos(angle) * radius + up * Mathf.Sin(angle) * radius;
                int idx = i * segmentsAround + j;
                vertices[idx] = points[i] + circlePos;
                normals[idx] = circlePos.normalized;
                uvs[idx] = new Vector2(j / (float)segmentsAround, vCoord);
            }

            if (i < points.Count - 1)
                vCoord += Vector3.Distance(points[i], points[i + 1]) / totalLength;
        }

        int triIndex = 0;
        for (int i = 0; i < points.Count - 1; i++)
        {
            for (int j = 0; j < segmentsAround; j++)
            {
                int current = i * segmentsAround + j;
                int next = i * segmentsAround + (j + 1) % segmentsAround;
                int nextRing = (i + 1) * segmentsAround + j;
                int nextRingNext = (i + 1) * segmentsAround + (j + 1) % segmentsAround;

                triangles[triIndex++] = current;
                triangles[triIndex++] = nextRing;
                triangles[triIndex++] = next;

                triangles[triIndex++] = next;
                triangles[triIndex++] = nextRing;
                triangles[triIndex++] = nextRingNext;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.RecalculateBounds();

        return mesh;
    }
}
