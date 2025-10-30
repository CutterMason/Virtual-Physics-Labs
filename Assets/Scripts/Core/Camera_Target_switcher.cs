using UnityEngine;

public class Camera_Target_Switcher : MonoBehaviour
{
    public Camera_Switcher cameraSwitcher;   // Reference to your existing camera script
    public Transform[] targets;              // List of objects the camera can follow
    public KeyCode switchKey = KeyCode.C;    // Key to switch targets

    private int currentIndex = 0;

    void Start()
    {
        if (cameraSwitcher == null)
        {
            cameraSwitcher = GetComponent<Camera_Switcher>();
        }

        // Start following the first target if available
        if (targets.Length > 0 && cameraSwitcher != null)
        {
            cameraSwitcher.player = targets[currentIndex];
            Debug.Log("Following: " + targets[currentIndex].name);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            SwitchTarget();
        }
    }

    void SwitchTarget()
    {
        if (targets.Length == 0 || cameraSwitcher == null)
        {
            Debug.LogWarning("No targets assigned or Camera_Switcher missing.");
            return;
        }

        // Cycle through targets
        currentIndex++;
        if (currentIndex >= targets.Length)
            currentIndex = 0;

        // Update the player target in your Camera_Switcher script
        cameraSwitcher.player = targets[currentIndex];

        Debug.Log("Switched to: " + targets[currentIndex].name);
    }
}
