using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpringPhysicsRigidbody : MonoBehaviour
{
    public Transform targetPoint;
    public float springStrength = 50f;
    public float damping = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (targetPoint == null) return;

        // Calculate spring and damping forces
        Vector3 displacement = transform.position - targetPoint.position;
        Vector3 springForce = -springStrength * displacement;
        Vector3 dampingForce = -damping * rb.linearVelocity;

        // Apply the total force to the rigidbody
        rb.AddForce(springForce + dampingForce);
    }
}