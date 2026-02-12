using UnityEngine;
using UnityEngine.UI;   // <-- add this

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
    public bool useConstantSpeed = false;    // actually used by the movement code
    public Toggle constantSpeedToggle;       // <-- drag your Toggle here in Inspector

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // Make sure the script’s bool matches whatever the toggle LOOKS like
        SetUseConstantSpeed(constantSpeedToggle.isOn);

        // Also hook up the listener in code (you can remove it from the inspector if you want)
        constantSpeedToggle.onValueChanged.AddListener(SetUseConstantSpeed);
    }

    void FixedUpdate()
    {
        if (GameControls.IsPaused) return;
        float input = 0f;
        if (Input.GetKey(KeyCode.A)) input -= 1f;
        if (Input.GetKey(KeyCode.D)) input += 1f;

        if (invertDirection)
            input = -input;

        if (useConstantSpeed)
            ApplyConstantSpeed(input);
        else
            ApplyAcceleration(input);
    }

    void ApplyAcceleration(float input)
    {
        Vector3 v = rb.linearVelocity;

        float current = (moveAxis == MoveAxis.X) ? v.x : v.z;

        if (Mathf.Abs(input) < 0.01f)
        {
            // No input: gradually slow down toward 0
            current = Mathf.MoveTowards(current, 0f, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Input: accelerate
            current += input * acceleration * Time.fixedDeltaTime;
            current = Mathf.Clamp(current, -maxSpeed, maxSpeed);
        }

        if (moveAxis == MoveAxis.X)
            v.x = current;
        else
            v.z = current;

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

    public void SetUseConstantSpeed(bool value)
    {
        useConstantSpeed = value;

        if (!useConstantSpeed)
        {
            // When switching OFF constant speed, zero out motion on that axis
            Vector3 v = rb.linearVelocity;
            if (moveAxis == MoveAxis.X) v.x = 0f;
            else v.z = 0f;
            rb.linearVelocity = v;
        }
    }
}