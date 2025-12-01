using UnityEngine;

public class CartPulleyPhysics : MonoBehaviour
{
    public Rigidbody cartRb;
    public Rigidbody weightRb;

    public float gravity = 9.81f;

    void FixedUpdate()
    {
        float m1 = cartRb.mass;
        float m2 = weightRb.mass;

        // The real acceleration of the system (mass m2 falling, pulling cart)
        float a = (m2 * gravity) / (m1 + m2);

        // The tension force that acts on BOTH masses
        float tension = m1 * a;

        // Apply continuous force on cart along +Z
        cartRb.AddForce(Vector3.forward * tension, ForceMode.Force);

        // Apply continuous upward force on the weight to simulate tension
        weightRb.AddForce(Vector3.up * tension, ForceMode.Force);
        // Gravity automatically adds m2 * g downward
    }
}
