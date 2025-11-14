using UnityEngine;

public class CarControllerSimple : MonoBehaviour
{
    public enum MoveAxis { X, Z }

    [Header("Movement Axis")]
    public MoveAxis moveAxis = MoveAxis.Z;  // which axis is left/right
    public bool invertDirection = false;

    [Header("Acceleration Mode")]
    public float acceleration = 10f;         // how fast we speed up
    public float maxSpeed = 5f;              // top speed
    public bool useConstantSpeed = false;    // if true: ignore acceleration

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float input = 0f;

        // A = left, D = right
        if (Input.GetKey(KeyCode.A)) input = -1f;
        if (Input.GetKey(KeyCode.D)) input = 1f;

        if (invertDirection)
            input *= -1;

        if (useConstantSpeed)
        {
            ApplyConstantSpeed(input);
        }
        else
        {
            ApplyAcceleration(input);
        }
    }

    void ApplyAcceleration(float input)
    {
        Vector3 v = rb.linearVelocity;

        // Compute current speed along our axis
        float current = (moveAxis == MoveAxis.X) ? v.x : v.z;

        // Increase speed based on input + acceleration
        float target = current + input * acceleration * Time.fixedDeltaTime;

        // Clamp speed
        target = Mathf.Clamp(target, -maxSpeed, maxSpeed);

        // Apply back to velocity
        if (moveAxis == MoveAxis.X)
            v.x = target;
        else
            v.z = target;

        rb.linearVelocity = v;
    }

    void ApplyConstantSpeed(float input)
    {
        Vector3 v = rb.linearVelocity;

        float targetSpeed = input * maxSpeed;

        if (moveAxis == MoveAxis.X)
            v.x = targetSpeed;
        else
            v.z = targetSpeed;

        rb.linearVelocity = v;
    }
}