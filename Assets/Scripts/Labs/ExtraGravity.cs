using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ExtraGravity : MonoBehaviour
{
    [Header("Gravity Multiplier")]
    [Range(1f, 5f)]
    public float gravityMultiplier = 2f;

    private Rigidbody rb;
    private bool skipOneFrame = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        // When unpausing, this script re-enables → skip the first frame
        skipOneFrame = true;
    }

    void FixedUpdate()
    {
        // Prevent gravity spike after unpause or velocity restore
        if (skipOneFrame)
        {
            skipOneFrame = false;
            return;
        }

        rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
    }
}
