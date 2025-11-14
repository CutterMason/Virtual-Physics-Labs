using UnityEngine;
using UnityEngine.Rendering;

public class Camera_Switcher : MonoBehaviour
{
    public Transform player; // Assign player
    public float smoothSpeed = 5f; // Camera follow speed
    public float sideViewZ = -10f; // Camera z for side view
    public float topDownHeight = 10f; // Camera height for top-down
    public float orthoSize = 5f;
    public float sideViewYRotation = 0f;
    public float sideViewXOffset = -10f;  // Negative pushes camera left, positive pushes right
    public float topDownXOffset = 0f;     // Optional if you want top-down shift too

    private bool isTopDown = false;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        SetSideViewInstant();
    }

    void UpdateTransparencySorting()
    {
        GraphicsSettings.transparencySortMode = TransparencySortMode.CustomAxis;

        if (isTopDown)
            GraphicsSettings.transparencySortAxis = new Vector3(0f, 1f, 0f);
        else
            GraphicsSettings.transparencySortAxis = new Vector3(1f, 0f, 0f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isTopDown = !isTopDown;
            UpdateTransparencySorting();
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        if (isTopDown)
            MoveToTopDown();
        else
            MoveToSideView();
    }

    void MoveToSideView()
    {
        Vector3 targetPos = new Vector3(
        player.position.x + sideViewXOffset,
        player.position.y,
        sideViewZ
        );

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);

        Quaternion targetRot = Quaternion.Euler(0f, sideViewYRotation, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, orthoSize, Time.deltaTime * smoothSpeed);
    }

    void MoveToTopDown()
    {
        Vector3 targetPos = new Vector3(
        player.position.x + topDownXOffset,
        player.position.y + topDownHeight,
        player.position.z
        );

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);

        Quaternion targetRot = Quaternion.Euler(90f, sideViewYRotation, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, orthoSize, Time.deltaTime * smoothSpeed);
    }

    void SetSideViewInstant()
    {
        transform.position = new Vector3(
            player.position.x + sideViewXOffset,
            player.position.y,
            sideViewZ
        );

        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        cam.orthographicSize = orthoSize;
    }
}