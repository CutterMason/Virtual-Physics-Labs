using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsObject : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 originalScale;
    private float originalMass;
    private float originalSpeed = 5f;

    [Header("Editable Properties")]
    public float mass = 1f;
    public float speed = 5f;
    public Vector3 size = Vector3.one;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;
        originalMass = rb.mass;
    }

    public void ApplyChanges(float newMass, Vector3 newSize, float newSpeed)
    {
        // persist the current values
        mass = newMass;
        size = newSize;
        speed = newSpeed;

        // Apply to runtime state
        transform.localScale = size;
        if (rb != null) rb.mass = mass;
    }

    public void ResetToOriginal()
    {
        ApplyChanges(originalMass, originalScale, originalSpeed);
    }
}