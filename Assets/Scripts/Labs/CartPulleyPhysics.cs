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

        float a = (m2 * gravity) / (m1 + m2);
        float tension = m1 * a;
        cartRb.AddForce(Vector3.forward * tension, ForceMode.Force);

        weightRb.AddForce(Vector3.up * tension, ForceMode.Force);
    }
}
