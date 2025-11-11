using UnityEngine;
using UnityEngine.UI;

public class SpeedControl : MonoBehaviour
{
    public Slider speedSlider;

    private float[] speedSteps = new float[] { 0.25f, 0.5f, 1f, 2f };

    private void Start()
    {
        // set initial speed to 1x
        Time.timeScale = 1f;

        if (speedSlider != null)
        {
            speedSlider.wholeNumbers = true;
            speedSlider.minValue = 0;
            speedSlider.maxValue = speedSteps.Length - 1;
            speedSlider.value = 2; // index of 1x speed
            speedSlider.onValueChanged.AddListener(UpdateSpeed);
        }
    }

    public void UpdateSpeed(float sliderValue)
    {
        int index = Mathf.RoundToInt(sliderValue);
        Time.timeScale = speedSteps[index];
        Debug.Log("Speed set to: " + Time.timeScale + "x");
    }
}