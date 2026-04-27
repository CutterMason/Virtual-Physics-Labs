using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsObject : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 originalScale;
    private float originalMass;

    [Header("Editable Properties")]
    [Range(0.05f, 1f)]
    public float mass;

    [HideInInspector]
    public Vector3 size;

    private const float MIN_MASS = 0.05f;
    private const float MAX_MASS = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        originalScale = transform.localScale;
        originalMass = rb.mass;

        size = originalScale;
        mass = Mathf.Clamp(originalMass, MIN_MASS, MAX_MASS);

        rb.mass = mass;
    }

    public void ApplyChanges(float newMass, Vector3 newSize)
    {
        newMass = Mathf.Clamp(newMass, MIN_MASS, MAX_MASS);

        mass = newMass;
        size = newSize;

        transform.localScale = size;
        rb.mass = mass;
        Debug.Log($"Applying Size: {newSize}");
    }

    public void ResetToOriginal()
    {
        size = originalScale;
        mass = originalMass;

        transform.localScale = originalScale;
        rb.mass = originalMass;
    }
    public Vector3 GetOriginalScale()
    {
        return originalScale;
    }
}