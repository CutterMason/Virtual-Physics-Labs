using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class SimpleRope : MonoBehaviour
{
    [Header("Rope Settings")]
    public Rigidbody startBody;           // Object A
    public Rigidbody endBody;             // Object B
    public GameObject ropeSegmentPrefab;  // Small capsule prefab
    public int segmentCount = 20;
    public float ropeMass = 0.05f;

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
        CreateRope();
    }

    void CreateRope()
    {
        Vector3 start = startBody.position;
        Vector3 end = endBody.position;
        float segmentLength = Vector3.Distance(start, end) / segmentCount;

        // Spawn rope segments between start and end
        Rigidbody prevBody = startBody;
        for (int i = 0; i < segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            Vector3 pos = Vector3.Lerp(start, end, t);

            GameObject seg = Instantiate(ropeSegmentPrefab, pos, Quaternion.identity, transform);
            Rigidbody rb = seg.GetComponent<Rigidbody>();
            rb.mass = ropeMass;
            segments.Add(rb);

            // Connect joints
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

        // Connect the last rope segment to the end body
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