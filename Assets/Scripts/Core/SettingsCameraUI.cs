using UnityEngine;
using UnityEngine.UI;

public class SettingsCameraUI : MonoBehaviour
{
    public Slider cameraSpeedSlider;

    private void Start()
    {
        cameraSpeedSlider.minValue = 2f;
        cameraSpeedSlider.maxValue = 20f;

        float savedSpeed = PlayerPrefs.GetFloat("CameraSpeed", 6f);

        cameraSpeedSlider.value = savedSpeed;

        ApplyCameraSpeed(savedSpeed);

        cameraSpeedSlider.onValueChanged.AddListener(OnCameraSpeedChanged);
    }

    private void OnCameraSpeedChanged(float value)
    {
        PlayerPrefs.SetFloat("CameraSpeed", value);
        ApplyCameraSpeed(value);
    }

    private void ApplyCameraSpeed(float value)
    {

    }
}