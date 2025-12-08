using UnityEngine;

public class RopeTension : MonoBehaviour
{
    [Header("References")]
    public Rigidbody cart;      
    public Rigidbody weight;    

    [Header("Rope Settings")]
    public float ropeLength = 2f;   
    public float g = 9.81f;         

    private float tension;

    void FixedUpdate()
    {
        if (weight == null || cart == null)
            return;

       
        float m2 = weight.mass;
        tension = m2 * g;

        float distance = Vector3.Distance(cart.position, weight.position);
        if (distance < ropeLength)
            return;

        Vector3 cartDirection = new Vector3(0, 0, 1);
        cart.AddForce(cartDirection * tension, ForceMode.Force);

        weight.AddForce(Vector3.up * tension, ForceMode.Force);
    }
}
