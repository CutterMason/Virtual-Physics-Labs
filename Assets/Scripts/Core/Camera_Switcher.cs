using UnityEngine;

public class Camera_Switcher : MonoBehaviour
{
    public Transform player; // Assign player
    public float smoothSpeed = 5f; // Camera follow speed
    public float sideViewZ = -10f; // Camera z for side view
    public float topDownHeight = 10f; // Camera height for top-down
    public float orthoSize = 5f;

    private bool isTopDown = false;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        SetSideViewInstant();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isTopDown = !isTopDown;
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
        Vector3 targetPos = new Vector3(player.position.x, player.position.y, sideViewZ);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);

        Quaternion targetRot = Quaternion.Euler(0f, 0f, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, orthoSize, Time.deltaTime * smoothSpeed);
    }

    void MoveToTopDown()
    {
        Vector3 targetPos = new Vector3(player.position.x, player.position.y + topDownHeight, player.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);

        Quaternion targetRot = Quaternion.Euler(90f, 0f, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, orthoSize, Time.deltaTime * smoothSpeed);
    }

    void SetSideViewInstant()
    {
        transform.position = new Vector3(player.position.x, player.position.y, sideViewZ);
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        cam.orthographicSize = orthoSize;
    }
}
