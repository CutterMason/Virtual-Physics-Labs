using UnityEngine;

public class CarControllerSimple : MonoBehaviour
{
    public enum MoveAxis { X, Z }

    [Header("Movement Axis")]
    public MoveAxis moveAxis = MoveAxis.Z;   // which axis is forward
    public bool invertDirection = false;

    [Header("Acceleration Settings")]
    public float acceleration = 10f;         // how fast we speed up (accel mode)
    public float maxSpeed = 5f;              // target speed (const-speed mode too)

    [Header("Mode")]
    public bool useConstantSpeed = false;    // toggled by UI

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Read input every frame
        float input = 0f;
        if (Input.GetKey(KeyCode.A)) input -= 1f;
        if (Input.GetKey(KeyCode.D)) input += 1f;

        if (invertDirection)
            input = -input;

        // Choose movement model
        if (useConstantSpeed)
            ApplyConstantSpeed(input);
        else
            ApplyAcceleration(input);
    }

    void ApplyAcceleration(float input)
    {
        Vector3 v = rb.linearVelocity;

        // current speed along our chosen axis
        float current = (moveAxis == MoveAxis.X) ? v.x : v.z;

        // accelerate based on input
        current += input * acceleration * Time.fixedDeltaTime;

        // clamp to maxSpeed
        current = Mathf.Clamp(current, -maxSpeed, maxSpeed);

        // write back to velocity
        if (moveAxis == MoveAxis.X)
            v.x = current;
        else
            v.z = current;

        rb.linearVelocity = v;
    }

    void ApplyConstantSpeed(float input)
    {
        Vector3 v = rb.linearVelocity;

        // If no input, stop along that axis; if input, move at fixed speed
        float targetSpeed = input * maxSpeed;

        if (moveAxis == MoveAxis.X)
            v.x = targetSpeed;
        else
            v.z = targetSpeed;

        rb.linearVelocity = v;
    }

    // Called by the Toggle's OnValueChanged(bool)
    public void SetUseConstantSpeed(bool value)
    {
        useConstantSpeed = value;
    }
}