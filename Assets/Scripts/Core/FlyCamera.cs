using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FlyCameraCC : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float fastMultiplier = 3f;
    public float verticalSpeed = 4f;          // Space/Ctrl
    public float acceleration = 20f;          // smoothing (optional)

    [Header("Look")]
    public float lookSensitivity = 2f;
    public float maxPitch = 85f;

    [Header("Collision")]
    public float antiStickPush = 0.2f;        // tiny push so we don’t “stick” to walls

    private CharacterController cc;

    private float yaw;
    private float pitch;

    private Vector3 currentVelocity;          // smoothed velocity

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;
    }

    void Update()
    {
        HandleLook();
        MoveWithCollisions();
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
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? fastMultiplier : 1f);

        float x = Input.GetAxisRaw("Horizontal"); // A/D
        float z = Input.GetAxisRaw("Vertical");   // W/S

        Vector3 input = (transform.right * x + transform.forward * z);

        // Vertical fly (optional)
        if (Input.GetKey(KeyCode.Space)) input += Vector3.up;
        if (Input.GetKey(KeyCode.LeftControl)) input += Vector3.down;

        if (input.sqrMagnitude > 1f) input.Normalize();

        Vector3 targetVel = input * speed;

        // Smooth acceleration so it feels nicer (you can remove this if you want instant)
        currentVelocity = Vector3.MoveTowards(
            currentVelocity,
            targetVel,
            acceleration * Time.deltaTime * speed
        );

        // CharacterController.Move handles collision resolution
        CollisionFlags flags = cc.Move(currentVelocity * Time.deltaTime);

        // If we hit something, add a tiny outward move next frame to reduce “sticking”
        if ((flags & CollisionFlags.Sides) != 0)
        {
            cc.Move(-currentVelocity.normalized * antiStickPush * Time.deltaTime);
        }
    }
}