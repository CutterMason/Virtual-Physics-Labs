using UnityEngine;
using UnityEngine.UI;

public class SettingsCameraUI : MonoBehaviour
{
    public Slider cameraSpeedSlider;

    private void Start()
    {
        // Ensure slider range
        cameraSpeedSlider.minValue = 2f;
        cameraSpeedSlider.maxValue = 20f;

        // --- Load saved value (or default) ---
        float savedSpeed = PlayerPrefs.GetFloat("CameraSpeed", 6f);

        // Set UI
        cameraSpeedSlider.value = savedSpeed;

        // Apply BEFORE wiring listener (same pattern as audio script)
        ApplyCameraSpeed(savedSpeed);

        // Wire event
        cameraSpeedSlider.onValueChanged.AddListener(OnCameraSpeedChanged);
    }

    private void OnCameraSpeedChanged(float value)
    {
        PlayerPrefs.SetFloat("CameraSpeed", value);
        ApplyCameraSpeed(value);
    }

    private void ApplyCameraSpeed(float value)
    {
        // This does NOT directly reference the camera.
        // It just stores the value globally via PlayerPrefs.
        // The camera will read it when needed.
    }
}