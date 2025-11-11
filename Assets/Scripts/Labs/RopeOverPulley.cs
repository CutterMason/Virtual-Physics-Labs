using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class RopeOverPulley : MonoBehaviour
{
    [Header("Rope Settings")]
    public Rigidbody startBody;           // Object A
    public Rigidbody endBody;             // Object B
    public GameObject ropeSegmentPrefab;  // Small capsule prefab
    public int segmentCount = 25;
    public float segmentLength = 0.1f;
    public float ropeMass = 0.05f;

    [Header("Pulley Settings")]
    public Transform pulleyTransform;     // Center of the pulley
    public float pulleyRadius = 0.5f;

    private List<Rigidbody> segments = new List<Rigidbody>();
    private LineRenderer lr;

    void Start()
    {
        if (!startBody || !endBody || !ropeSegmentPrefab)
        {
            Debug.LogError("Missing rope setup references!");
            return;
        }

        lr = GetComponent<LineRenderer>();
        lr.positionCount = segmentCount;

        CreateRope();
    }

    void CreateRope()
    {
        Vector3 start = startBody.position;
        Vector3 end = endBody.position;

        // Initial path: A → pulley → B
        Vector3 dirAtoPulley = (pulleyTransform.position - start).normalized;
        Vector3 dirPulleyToB = (end - pulleyTransform.position).normalized;

        Vector3[] pathPoints = new Vector3[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);
            if (t < 0.5f)
                pathPoints[i] = Vector3.Lerp(start, pulleyTransform.position + dirAtoPulley * pulleyRadius, t * 2);
            else
                pathPoints[i] = Vector3.Lerp(pulleyTransform.position + dirPulleyToB * pulleyRadius, end, (t - 0.5f) * 2);
        }

        Rigidbody prevBody = startBody;

        for (int i = 0; i < segmentCount; i++)
        {
            GameObject seg = Instantiate(ropeSegmentPrefab, pathPoints[i], Quaternion.identity, transform);
            Rigidbody rb = seg.GetComponent<Rigidbody>();
            rb.mass = ropeMass;
            segments.Add(rb);

            ConfigurableJoint joint = seg.GetComponent<ConfigurableJoint>();
            joint.connectedBody = prevBody;

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;

            prevBody = rb;
        }

        // Attach last segment to the hanging object
        ConfigurableJoint endJoint = endBody.gameObject.AddComponent<ConfigurableJoint>();
        endJoint.connectedBody = segments[segments.Count - 1];
        endJoint.xMotion = ConfigurableJointMotion.Locked;
        endJoint.yMotion = ConfigurableJointMotion.Locked;
        endJoint.zMotion = ConfigurableJointMotion.Locked;
    }

    void LateUpdate()
    {
        if (segments.Count == 0) return;

        lr.positionCount = segments.Count;
        for (int i = 0; i < segments.Count; i++)
            lr.SetPosition(i, segments[i].position);
    }
}