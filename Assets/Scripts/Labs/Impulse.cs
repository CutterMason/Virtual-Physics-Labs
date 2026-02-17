using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpringPhysicsRigidbodyImpulse : MonoBehaviour
{
    public Transform targetPoint;
    public float impulseStrength = 10f;   // how hard the impulse pulls
    public float damping = 0.5f;           // how much velocity is resisted

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (targetPoint == null) return;

        // Direction toward target
        Vector3 displacement = targetPoint.position - transform.position;

        // Desired velocity change toward target
        Vector3 desiredVelocity = displacement * impulseStrength;

        // Damping (counteracts current velocity)
        Vector3 dampingVelocity = -rb.linearVelocity * damping;

        // Net impulse (Impulse = mass * Δv)
        Vector3 impulse = (desiredVelocity + dampingVelocity) * rb.mass;

        rb.AddForce(impulse, ForceMode.Impulse);
    }
}
