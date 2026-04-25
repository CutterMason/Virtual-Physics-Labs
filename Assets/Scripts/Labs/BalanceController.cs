using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SeesawSystem : MonoBehaviour
{
    [Header("Beam")]
    public Transform beamTransform;

    [Range(-0.5f, 0.5f)]
    public float beamPosition = 0f;

    public float beamMin = -0.5f;
    public float beamMax = 0.5f;

    [Header("Weights")]
    public Transform weight1;
    public Transform weight2;

    [Range(-0.5f, 0.5f)]
    public float weight1Position = -0.25f;

    [Range(-0.5f, 0.5f)]
    public float weight2Position = 0.25f;

    [Header("Weight Mass")]
    public float weight1Mass = 1f;
    public float weight2Mass = 1f;

    [Header("Physics Options")]
    public bool useGravity = false;
    public float gravity = 9.81f;

    [Header("Beam Influence (movement imbalance)")]
    public float beamBiasFactor = 2f;

    [Header("Sliders")]
    public Slider beamSlider;
    public Slider weight1Slider;
    public Slider weight2Slider;

    [Header("UI Text")]
    public TMP_Text beamText;
    public TMP_Text weight1Text;
    public TMP_Text weight2Text;

    public TMP_Text torqueText;
    public TMP_Text balanceStateText;

    [Header("Visual Feedback")]
    public Renderer beamRenderer;
    public Color balancedColor = Color.white;
    public Color heavyColor = Color.red;

    [Header("Fake Physics")]
    public float maxTiltAngle = 40f;
    public float tiltSmoothSpeed = 6f;

    private float currentTilt = 0f;
    private Vector3 beamStartWorldPos;

    void Start()
    {
        beamStartWorldPos = beamTransform.position;

        if (beamSlider != null)
        {
            beamSlider.minValue = beamMin;
            beamSlider.maxValue = beamMax;
            beamSlider.value = beamPosition;
            beamSlider.onValueChanged.AddListener(v => beamPosition = v);
        }

        if (weight1Slider != null)
        {
            weight1Slider.minValue = 0f;
            weight1Slider.maxValue = 1f;
            weight1Slider.value = Mathf.InverseLerp(-0.5f, 0.5f, weight1Position);
            weight1Slider.onValueChanged.AddListener(v => weight1Position = Mathf.Lerp(-0.5f, 0.5f, v));
        }

        if (weight2Slider != null)
        {
            weight2Slider.minValue = 0f;
            weight2Slider.maxValue = 1f;
            weight2Slider.value = Mathf.InverseLerp(-0.5f, 0.5f, weight2Position);
            weight2Slider.onValueChanged.AddListener(v => weight2Position = Mathf.Lerp(-0.5f, 0.5f, v));
        }

        ApplyAll();
    }

    void Update()
    {
        ApplyAll();
    }

    // -----------------------------
    // MAIN UPDATE
    // -----------------------------
    void ApplyAll()
    {
        ApplyBeam();

        ApplyWeight(weight1, weight1Position, weight1Text, "Weight 1");
        ApplyWeight(weight2, weight2Position, weight2Text, "Weight 2");

        ApplyFakePhysics();
    }

    // -----------------------------
    // BEAM POSITION (world preserved)
    // -----------------------------
    void ApplyBeam()
    {
        float clamped = Mathf.Clamp(beamPosition, beamMin, beamMax);

        Vector3 offset = new Vector3(-clamped, 0f, 0f);
        beamTransform.position = beamStartWorldPos + offset;

        if (beamText != null)
            beamText.text = $"Beam: {clamped:F2} m";
    }

    // -----------------------------
    // WEIGHT POSITION (local space)
    // -----------------------------
    void ApplyWeight(Transform weight, float value, TMP_Text text, string label)
    {
        float flipped = -value;

        weight.localPosition = new Vector3(
            flipped,
            weight.localPosition.y,
            weight.localPosition.z
        );

        if (text != null)
            text.text = $"{label}: {value:F2} m";
    }

    // -----------------------------
    // FAKE PHYSICS (TORQUE SYSTEM)
    // -----------------------------
    void ApplyFakePhysics()
    {
        // Distance × Mass (core physics model)
        float torque =
            (weight1Position * weight1Mass) +
            (weight2Position * weight2Mass);

        // Beam movement also affects balance
        torque += beamPosition * beamBiasFactor;

        // Optional gravity scaling
        if (useGravity)
            torque *= gravity;

        // normalize
        float normalized = Mathf.Clamp(torque, -0.5f, 0.5f) / 0.5f;

        // exaggerated response for visibility
        float exaggerated = Mathf.Sign(normalized) * normalized * normalized;

        float targetTilt = -exaggerated * maxTiltAngle;

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSmoothSpeed);

        // subtle wobble when unbalanced
        currentTilt += Mathf.Sin(Time.time * 6f) * 0.3f * Mathf.Abs(normalized);

        beamTransform.rotation = Quaternion.Euler(0f, 0f, currentTilt);

        // -----------------------------
        // VISUAL FEEDBACK
        // -----------------------------
        float imbalance = Mathf.Abs(normalized);

        if (beamRenderer != null)
        {
            beamRenderer.material.color =
                Color.Lerp(balancedColor, heavyColor, imbalance);
        }

        if (torqueText != null)
            torqueText.text = $"Torque: {torque:F2}";

        if (balanceStateText != null)
        {
            if (Mathf.Abs(torque) < 0.01f)
                balanceStateText.text = "Balanced";
            else if (torque > 0)
                balanceStateText.text = "Right Side Heavier";
            else
                balanceStateText.text = "Left Side Heavier";
        }
    }
}