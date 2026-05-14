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

    [Header("Physics Options")]
    public bool isStaticObject = false;

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

        ApplyStaticState();
    }

    public void ApplyChanges(float newMass, Vector3 newSize)
    {
        newMass = Mathf.Clamp(newMass, MIN_MASS, MAX_MASS);

        mass = newMass;
        size = newSize;

        transform.localScale = size;
        rb.mass = mass;

        ApplyStaticState();

        Debug.Log($"Applying Size: {newSize}");
    }

    public void ResetToOriginal()
    {
        size = originalScale;
        mass = originalMass;

        transform.localScale = originalScale;
        rb.mass = originalMass;

        isStaticObject = false;
        ApplyStaticState();
    }

    public void SetStaticState(bool isStatic)
    {
        isStaticObject = isStatic;
        ApplyStaticState();

        Debug.Log($"{name} static saved as {isStaticObject}");
    }

    public void ApplyStaticState()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.isKinematic = isStaticObject;
        rb.useGravity = !isStaticObject;

        if (isStaticObject)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }
        else
        {
            rb.WakeUp();
        }

        Debug.Log($"{name} applied static={isStaticObject}, isKinematic={rb.isKinematic}, useGravity={rb.useGravity}");
    }

    public Vector3 GetOriginalScale()
    {
        return originalScale;
    }
}