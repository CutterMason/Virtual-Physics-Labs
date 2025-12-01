using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsObject : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 originalScale;
    private float originalMass;
    private float originalSpeed = 5f;

    [Header("Editable Properties")]
    [Range(0.05f, 1f)]
    public float mass = 1f;

    public float speed = 5f;
    public Vector3 size = Vector3.one;

    private const float MIN_MASS = 0.05f;
    private const float MAX_MASS = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;
        originalMass = rb.mass;

        // Ensure Inspector matches actual
        size = originalScale;
        mass = Mathf.Clamp(mass, MIN_MASS, MAX_MASS);
    }

    public void ApplyChanges(float newMass, Vector3 newSize, float newSpeed)
    {
        // Clamp mass to valid range ALWAYS
        newMass = Mathf.Clamp(newMass, MIN_MASS, MAX_MASS);

        // Save the clamped mass + speed
        mass = newMass;
        speed = newSpeed;

        // Only update size if user changed it
        if (newSize != size)
        {
            size = newSize;
            transform.localScale = size;
        }

        if (rb != null)
            rb.mass = mass;
    }

    public void ResetToOriginal()
    {
        size = originalScale;
        mass = originalMass;
        speed = originalSpeed;

        transform.localScale = originalScale;
        rb.mass = originalMass;
    }
}
