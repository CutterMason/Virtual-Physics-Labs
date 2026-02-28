using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class FlyCameraCC : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float fastMultiplier = 3f;
    public float verticalSpeed = 4f;
    public float acceleration = 20f;

    [Header("Look")]
    public float lookSensitivity = 2f;
    public float maxPitch = 85f;

    [Header("Collision")]
    public float antiStickPush = 0.2f;

    [Header("UI")]
    public Slider speedSlider;
    public Text speedText; // Assign a Text UI to show current speed

    private CharacterController cc;
    private float yaw;
    private float pitch;
    private Vector3 currentVelocity;

    private float speedMultiplier = 1f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;

        // Setup slider
        if (speedSlider != null)
        {
            speedSlider.minValue = 0.5f;
            speedSlider.maxValue = 1.5f;
            speedSlider.value = 1f;
            speedSlider.onValueChanged.AddListener(UpdateSpeedMultiplier);
        }

        UpdateSpeedText();
    }

    void UpdateSpeedMultiplier(float value)
    {
        speedMultiplier = value;
        UpdateSpeedText();
    }

    void UpdateSpeedText()
    {
        if (speedText != null)
        {
            float currentSpeed = moveSpeed * speedMultiplier;
            speedText.text = $"Speed: {currentSpeed:0.0}";
        }
    }

    void Update()
    {
        HandleLook();
        MoveWithCollisions();
    }

    void HandleLook()
    {
        // Default state: cursor acts like a normal cursor
        if (!Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // RMB held: lock + hide for free-look
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        float mx = Input.GetAxis("Mouse X") * lookSensitivity;
        float my = Input.GetAxis("Mouse Y") * lookSensitivity;

        yaw += mx;
        pitch -= my;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void MoveWithCollisions()
    {

        if (UnityEngine.EventSystems.EventSystem.current != null &&
        UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null)
        {
            // Optional: return only if it's an InputField (typing), not sliders/buttons
        }
        // Use unscaled time so camera movement works even when Time.timeScale = 0
        float dt = Time.unscaledDeltaTime;

        float speed = moveSpeed * speedMultiplier * (Input.GetKey(KeyCode.LeftShift) ? fastMultiplier : 1f);

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 input = (transform.right * x + transform.forward * z);

        if (Input.GetKey(KeyCode.Space)) input += Vector3.up;
        if (Input.GetKey(KeyCode.LeftControl)) input += Vector3.down;

        if (input.sqrMagnitude > 1f) input.Normalize();

        Vector3 targetVel = input * speed;

        currentVelocity = Vector3.MoveTowards(
            currentVelocity,
            targetVel,
            acceleration * dt * speed
        );

        CollisionFlags flags = cc.Move(currentVelocity * dt);

        if ((flags & CollisionFlags.Sides) != 0 && currentVelocity.sqrMagnitude > 0.0001f)
        {
            cc.Move(-currentVelocity.normalized * antiStickPush * dt);
        }

        UpdateSpeedText();
    }
}