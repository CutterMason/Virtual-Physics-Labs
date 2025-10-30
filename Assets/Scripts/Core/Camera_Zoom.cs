using UnityEngine;

public class Camera_Zoom : MonoBehaviour
{
    public float zoomSpeed = 5f;        // How fast the zoom reacts to the scroll
    public float smoothSpeed = 10f;     // How smooth the zoom feels
    public float minZoom = 3f;          // Minimum orthographic size or FOV
    public float maxZoom = 15f;         // Maximum orthographic size or FOV

    private Camera cam;
    private float targetZoom;

    void Start()
    {
        cam = GetComponent<Camera>();

        // Set the starting zoom here
        targetZoom = 3f; // smaller number = closer zoom

        if (cam.orthographic)
            cam.orthographicSize = targetZoom;
        else
            cam.fieldOfView = targetZoom; // for perspective cameras
    }

    void Update()
    {
        // Get scroll wheel input (positive = scroll up, negative = scroll down)
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            // Adjust zoom target
            if (cam.orthographic)
                targetZoom -= scroll * zoomSpeed;
            else
                targetZoom -= scroll * zoomSpeed * 5f; // Adjust for perspective FOV

            // Clamp zoom between min and max
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // Smoothly interpolate toward target zoom
        if (cam.orthographic)
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * smoothSpeed);
        else
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetZoom, Time.deltaTime * smoothSpeed);
    }
}
