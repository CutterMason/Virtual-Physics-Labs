using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeSpawnBetween : MonoBehaviour 
{
    [SerializeField] GameObject partPrefab, parentObject;
    [SerializeField] Transform startObject, endObject;   // ⬅️ Added references
    [SerializeField] [Range(1, 1000)] int length = 1;
    [SerializeField] float partDistance = 0.21f;
    [SerializeField] bool reset, spawn, snapFirst, snapLast;

    void Update()
    {
        if (reset)
        {
            foreach (Transform child in parentObject.transform)
                Destroy(child.gameObject);
            reset = false;
        }

        if (spawn)
        {
            Spawn();
            spawn = false;
        }
    }

    public void Spawn()
    {
        if (startObject == null || endObject == null)
        {
            Debug.LogWarning("Missing start or end object!");
            return;
        }

        // Compute direction and distance
        Vector3 direction = (endObject.position - startObject.position);
        float totalDistance = direction.magnitude;
        direction.Normalize();

        int count = Mathf.Max(1, Mathf.RoundToInt(totalDistance / partDistance));
        Rigidbody previousBody = null;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = startObject.position + direction * partDistance * i;
            GameObject part = Instantiate(partPrefab, pos, Quaternion.LookRotation(direction), parentObject.transform);
            part.name = $"RopePart_{i}";

            Rigidbody rb = part.GetComponent<Rigidbody>();
            CharacterJoint joint = part.GetComponent<CharacterJoint>();

            if (i == 0)
            {
                // First segment connects to start object
                if (joint != null)
                {
                    joint.connectedBody = startObject.GetComponent<Rigidbody>();
                }

                if (snapFirst)
                    rb.constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                if (joint != null && previousBody != null)
                {
                    joint.connectedBody = previousBody;
                }
            }

            previousBody = rb;
        }

        // Connect the last segment to the end object
        if (previousBody != null && snapLast)
            previousBody.constraints = RigidbodyConstraints.FreezeAll;
        else if (previousBody != null)
        {
            CharacterJoint endJoint = previousBody.GetComponent<CharacterJoint>();
            if (endJoint != null)
                endJoint.connectedBody = endObject.GetComponent<Rigidbody>();
        }
    }
}