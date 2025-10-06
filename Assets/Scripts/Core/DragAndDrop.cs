using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 offset;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void OnMouseDown()
    {
        // When the player clicks the object, start dragging
        isDragging = true;

        // Convert mouse position to world point and calculate offset
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.WorldToScreenPoint(transform.position).z)
            );
        offset = transform.position - mouseWorldPos;
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            // Convert mouse to world position and apply offset
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.WorldToScreenPoint(transform.position).z)
            );

            Vector3 newPos = mouseWorldPos + offset;

            // Keep Z locked so the car always stays flat in 2D view
            newPos.z = transform.position.z;

            transform.position = newPos;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

}
