using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public TMP_Text speedText;

    [Header("Top View")]
    public Transform turntableCenter;
    public float topViewHeight = 3f;
    public KeyCode topViewToggleKey = KeyCode.V;
    public float topViewMoveSpeed = 8f;
    public bool lookStraightDown = true;

    [Header("Optional")]
    public LockOnCamera lockOnCameraScript;

    private CharacterController cc;
    private float yaw;
    private float pitch;
    private Vector3 currentVelocity;
    private float speedMultiplier = 1f;

    private bool isTopView = false;

    private Vector3 savedPosition;
    private Quaternion savedRotation;
    private bool savedLockOnEnabled;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;
    }

    void Start()
    {
        speedMultiplier = PlayerPrefs.GetFloat("CameraSpeed", 1f);

        if (speedSlider != null)
        {
            speedSlider.minValue = 0.5f;
            speedSlider.maxValue = 1.5f;
            speedSlider.value = speedMultiplier;
            speedSlider.onValueChanged.RemoveAllListeners();
            speedSlider.onValueChanged.AddListener(UpdateSpeedMultiplier);
        }

        UpdateSpeedText();
    }

    void Update()
    {
        HandleTopViewToggle();

        if (isTopView)
        {
            HandleTopViewMovement();
            return;
        }

        // If lock-on camera is actively following something,
        // do not let free-fly camera movement fight it.
        if (lockOnCameraScript != null && lockOnCameraScript.IsLockedOn)
            return;

        HandleLook();
        MoveWithCollisions();
    }

    void HandleTopViewToggle()
    {
        if (!Input.GetKeyDown(topViewToggleKey))
            return;

        if (!isTopView)
            EnterTopView();
        else
            ExitTopView();
    }

    void EnterTopView()
    {
        if (turntableCenter == null)
        {
            Debug.LogWarning("FlyCameraCC: No turntableCenter assigned.");
            return;
        }

        isTopView = true;

        savedPosition = transform.position;
        savedRotation = transform.rotation;

        currentVelocity = Vector3.zero;

        if (lockOnCameraScript != null)
        {
            savedLockOnEnabled = lockOnCameraScript.enabled;
            lockOnCameraScript.enabled = false;
        }

        transform.position = turntableCenter.position + Vector3.up * topViewHeight;

        if (lookStraightDown)
        {
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
        else
        {
            transform.LookAt(turntableCenter);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ExitTopView()
    {
        isTopView = false;

        transform.position = savedPosition;
        transform.rotation = savedRotation;

        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;

        if (lockOnCameraScript != null)
        {
            lockOnCameraScript.enabled = savedLockOnEnabled;
        }
    }

    void HandleTopViewMovement()
    {
        if (turntableCenter == null) return;

        transform.position = turntableCenter.position + Vector3.up * topViewHeight;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    void UpdateSpeedMultiplier(float value)
    {
        speedMultiplier = value;
        PlayerPrefs.SetFloat("CameraSpeed", value);
        PlayerPrefs.Save();
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

    void HandleLook()
    {
        if (!Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

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