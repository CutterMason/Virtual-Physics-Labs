using UnityEngine;
using System.Collections.Generic;

public class RopeGenerator : MonoBehaviour
{
    [Header("Rope Settings")]
    public GameObject segmentPrefab;
    public int segmentCount = 15;
    public float segmentLength = 0.5f;

    [Header("Joint Settings")]
    public float spring = 100f;
    public float damper = 5f;

    private List<Rigidbody> segments = new List<Rigidbody>();

    void Start()
    {
        GenerateRope();
    }

    void GenerateRope()
    {
        Rigidbody previousBody = null;

        for (int i = 0; i < segmentCount; i++)
        {
            // Create segment
            GameObject segment = Instantiate(segmentPrefab, transform.position - new Vector3(0, i * segmentLength, 0), Quaternion.identity);
            segment.transform.parent = transform; // optional, keep hierarchy clean
            Rigidbody rb = segment.GetComponent<Rigidbody>();
            segments.Add(rb);

            // Create joint if not first segment
            if (previousBody != null)
            {
                ConfigurableJoint joint = segment.AddComponent<ConfigurableJoint>();
                joint.connectedBody = previousBody;

                joint.xMotion = ConfigurableJointMotion.Limited;
                joint.yMotion = ConfigurableJointMotion.Limited;
                joint.zMotion = ConfigurableJointMotion.Limited;

                SoftJointLimit limit = joint.linearLimit;
                limit.limit = segmentLength;
                joint.linearLimit = limit;

                // Spring & damper
                JointDrive drive = new JointDrive
                {
                    positionSpring = spring,
                    positionDamper = damper,
                    maximumForce = Mathf.Infinity
                };
                joint.xDrive = joint.yDrive = joint.zDrive = drive;

                // Stabilize joints
                joint.projectionMode = JointProjectionMode.PositionAndRotation;
                joint.projectionDistance = 0.05f;
                joint.projectionAngle = 5f;
            }

            previousBody = rb;
        }

        // Ignore self-collisions
        for (int i = 0; i < segments.Count; i++)
        {
            for (int j = i + 1; j < segments.Count; j++)
            {
                Physics.IgnoreCollision(segments[i].GetComponent<Collider>(), segments[j].GetComponent<Collider>());
            }
        }
    }
}