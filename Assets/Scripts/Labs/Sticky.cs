using UnityEngine;

public class StickyCar : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Only stick to other toy cars
        if (!collision.gameObject.CompareTag("Savable")) return;

        Rigidbody rb1 = GetComponent<Rigidbody>();
        Rigidbody rb2 = collision.rigidbody;

        if (rb2 == null) return; // Only stick to objects with Rigidbody

        // Calculate combined velocity for inelastic collision
        Vector3 combinedVelocity = 
            (rb1.linearVelocity * rb1.mass + rb2.linearVelocity * rb2.mass) / (rb1.mass + rb2.mass);

        // Stop the second car from simulating physics and make it a child
        rb2.isKinematic = true;
        collision.transform.SetParent(transform);

        // Apply the new combined velocity to the first car
        rb1.linearVelocity = combinedVelocity;
    }
}
