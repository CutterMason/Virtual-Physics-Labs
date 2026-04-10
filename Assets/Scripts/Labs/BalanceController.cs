using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BeamWeightController : MonoBehaviour
{
    [Header("Beam Reference")]
    public BoxCollider beamCollider;

    [Header("Weights")]
    public Transform weight1;
    public Transform weight2;

    [Header("Sliders (0–1 full range)")]
    public Slider slider1;
    public Slider slider2;

    [Header("Text Labels")]
    public TMP_Text text1;
    public TMP_Text text2;

    [Header("Default Setup (logical space 0–1)")]
    [Range(0f, 0.5f)]
    public float normalizedDistanceFromEnd = 0.1f;

    [Header("Movement Bounds (logical space 0–1)")]
    public float weight1Min = 0f;
    public float weight1Max = 1f;

    public float weight2Min = 0f;
    public float weight2Max = 1f;

    private float minX;
    private float maxX;

    private bool simulationStarted = false;

    void Start()
    {
        // ---------------------------------------------------
        // Get real beam size from BoxCollider
        // ---------------------------------------------------
        float beamWorldLength =
            beamCollider.size.x * beamCollider.transform.lossyScale.x;

        minX = -beamWorldLength / 2f;
        maxX = beamWorldLength / 2f;

        // ---------------------------------------------------
        // Slider setup (full 0–1 range)
        // ---------------------------------------------------
        slider1.minValue = 0f;
        slider1.maxValue = 1f;

        slider2.minValue = 0f;
        slider2.maxValue = 1f;

        slider1.onValueChanged.AddListener(UpdateWeight1);
        slider2.onValueChanged.AddListener(UpdateWeight2);

        // ---------------------------------------------------
        // Default symmetric placement
        // ---------------------------------------------------
        float leftValue = normalizedDistanceFromEnd;
        float rightValue = 1f - normalizedDistanceFromEnd;

        slider1.value = leftValue;
        slider2.value = rightValue;

        UpdateWeight1(leftValue);
        UpdateWeight2(rightValue);
    }

    // -------------------------------------------------------
    // PUBLIC CALL FROM YOUR MAIN START BUTTON
    // -------------------------------------------------------
    public void StartSimulation()
    {
        simulationStarted = true;

        // Lock sliders so they no longer affect weights
        slider1.interactable = false;
        slider2.interactable = false;
    }

    // -------------------------------------------------------
    // SLIDER INPUT HANDLING (DISABLED AFTER START)
    // -------------------------------------------------------

    void UpdateWeight1(float sliderValue)
    {
        if (simulationStarted) return;

        float value = Mathf.Lerp(weight1Min, weight1Max, sliderValue);

        ApplyWeight(weight1, text1, value, "Weight 1");
    }

    void UpdateWeight2(float sliderValue)
    {
        if (simulationStarted) return;

        float value = Mathf.Lerp(weight2Min, weight2Max, sliderValue);

        ApplyWeight(weight2, text2, value, "Weight 2");
    }

    // -------------------------------------------------------
    // CORE POSITIONING + DISPLAY
    // -------------------------------------------------------

    void ApplyWeight(Transform weight, TMP_Text text, float value, string label)
    {
        // Flip for camera orientation (0 = right, 1 = left)
        float flipped = 1f - value;

        float xPos = Mathf.Lerp(minX, maxX, flipped);

        weight.localPosition = new Vector3(
            xPos,
            weight.localPosition.y,
            weight.localPosition.z
        );

        text.text = $"{label}: {value:F2} m";
    }
}