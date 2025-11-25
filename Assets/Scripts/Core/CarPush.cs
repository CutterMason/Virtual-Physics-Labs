using UnityEngine;

public class CarPush : MonoBehaviour
{
    public float pushForce = 500f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void PushRight()
    {
        rb.AddForce(transform.forward * pushForce, ForceMode.Impulse);
    }
}