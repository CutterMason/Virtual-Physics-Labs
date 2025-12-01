using UnityEngine;

public class ToyCar : MonoBehaviour
{
    public Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
}
