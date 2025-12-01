using UnityEngine;

public class RopeTension : MonoBehaviour
{
    [Header("References")]
    public Rigidbody cart;      // the cart on the track
    public Rigidbody weight;    // the hanging mass

    [Header("Rope Settings")]
    public float ropeLength = 2f;   // distance before rope becomes taut
    public float g = 9.81f;         // gravity constant

    private float tension;

    void FixedUpdate()
    {
        if (weight == null || cart == null)
            return;

        // Always read m2 directly from the rigidbody at runtime
        float m2 = weight.mass;

        // Recompute tension every physics step
        tension = m2 * g;

        // Current rope distance
        float distance = Vector3.Distance(cart.position, weight.position);

        // Rope slack → do nothing
        if (distance < ropeLength)
            return;

        //
        // APPLY CONSTANT TENSION FORCE
        //

        // Cart moves along +Z
        Vector3 cartDirection = new Vector3(0, 0, 1);
        cart.AddForce(cartDirection * tension, ForceMode.Force);

        // Weight pulled upward
        weight.AddForce(Vector3.up * tension, ForceMode.Force);
    }
}
