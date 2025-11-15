using UnityEngine;

public class TieMovement : MonoBehaviour
{
    public Transform objectA; // Object to move along Z
    public Transform objectB; // Object to track along Y

    private float lastY; // Previous Y position of B

    void Start()
    {
        if (objectB != null)
            lastY = objectB.position.y; // Initialize
    }

    void Update()
    {
        if (objectA != null && objectB != null)
        {
            // Compute how much B moved along Y
            float deltaY = objectB.position.y - lastY;

            // Apply the opposite movement to A along Z
            objectA.position += new Vector3(0f, 0f, -deltaY);

            // Update lastY for next frame
            lastY = objectB.position.y;
        }
    }
}

