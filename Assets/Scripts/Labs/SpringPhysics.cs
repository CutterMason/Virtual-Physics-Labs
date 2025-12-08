using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpringPhysicsRigidbody : MonoBehaviour
{
    public Transform targetPoint;
    public float springStrength = 50f;
    public float damping = 5f;

    private Rigidbody rb;

    private Vector3 lastVelocity;
    private bool suppressOneFrame = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lastVelocity = rb.linearVelocity;
    }

    void FixedUpdate()
    {
        if (targetPoint == null) return;
        if (lastVelocity.sqrMagnitude < 0.0001f && rb.linearVelocity.sqrMagnitude > 0.0001f)
        {
            suppressOneFrame = true;
        }

        lastVelocity = rb.linearVelocity;
        if (suppressOneFrame)
        {
            suppressOneFrame = false;
            return;  
        }
        Vector3 displacement = transform.position - targetPoint.position;
        Vector3 springForce = -springStrength * displacement;
        Vector3 dampingForce = -damping * rb.linearVelocity;

        rb.AddForce(springForce + dampingForce, ForceMode.Force);
    }
}
