using UnityEngine;

public class PulleyRopeSystem : MonoBehaviour
{
    [Header("References")]
    public Rigidbody cart;
    public Rigidbody weight;

    [Header("Rope Settings")]
    public float ropeLength = 2f;

    [Header("Visual Settings")]
    public float ropeCorrectionSpeed = 5f;  // How quickly the rope corrects itself
    public float lateralDamping = 0.98f;    // Reduces sideways flopping

    private float g;

    void Start()
    {
        g = Physics.gravity.magnitude;

        if (weight != null)
            weight.useGravity = true;

        // Optional: auto-set rope length
        ropeLength = Vector3.Distance(cart.position, weight.position);
    }

    void FixedUpdate()
    {
        if (cart == null || weight == null) return;

        // 1. Horizontal direction: cart is pulled toward weight
        Vector3 horizontalDir = weight.position - cart.position;
        horizontalDir.y = 0f;
        horizontalDir.Normalize();

        // 2. System acceleration based on hanging weight
        float m1 = cart.mass;
        float m2 = weight.mass;
        float a = (m2 * g) / (m1 + m2);

        // Apply horizontal tension to cart
        cart.AddForce(horizontalDir * a * m1, ForceMode.Force);

        // 3. Enforce rope length with smooth correction
        Vector3 ropeVector = weight.position - cart.position;
        float currentDistance = ropeVector.magnitude;

        if (currentDistance > ropeLength)
        {
            ropeVector.Normalize();

            // Gradually move weight toward rope length (smooth)
            Vector3 targetPos = cart.position + ropeVector * ropeLength;
            weight.position = Vector3.Lerp(weight.position, targetPos, Time.fixedDeltaTime * ropeCorrectionSpeed);

            // Remove extra velocity along rope direction to prevent overshoot
            Vector3 relativeVelocity = weight.linearVelocity - cart.linearVelocity;
            float alongRope = Vector3.Dot(relativeVelocity, ropeVector);
            if (alongRope > 0f)
                weight.linearVelocity -= ropeVector * alongRope;
        }

        // 4. Damp lateral (horizontal) velocity to prevent rope flopping
        Vector3 lateralVelocity = weight.linearVelocity;
        lateralVelocity.y = 0f; // only horizontal
        lateralVelocity *= lateralDamping;
        weight.linearVelocity = new Vector3(lateralVelocity.x, weight.linearVelocity.y, lateralVelocity.z);
    }
}