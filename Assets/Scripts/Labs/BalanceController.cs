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

    [Header("Weight Position Limits")]
    public float weight1MinPosition = -0.5f;
    public float weight1MaxPosition = 0.5f;

    public float weight2MinPosition = -0.5f;
    public float weight2MaxPosition = 0.5f;

    [Header("Weight Mass")]
    public float weight1Mass = 1f;
    public float weight2Mass = 1f;

    [Header("Mass Settings")]
    public float minMass = 0.1f;
    public float maxMass = 5f;

    [Header("Sliders")]
    public Slider beamSlider;
    public Slider weight1Slider;
    public Slider weight2Slider;

    public Slider weight1MassSlider;
    public Slider weight2MassSlider;

    [Header("Slider Visual States")]
    public Color sliderActiveColor = Color.white;
    public Color sliderDisabledColor = Color.gray;

    [Header("UI Text (Position)")]
    public TMP_Text beamText;
    public TMP_Text weight1Text;
    public TMP_Text weight2Text;

    [Header("UI Text (Mass)")]
    public TMP_Text weight1MassText;
    public TMP_Text weight2MassText;

    public TMP_Text torqueText;
    public TMP_Text balanceStateText;

    [Header("Visual Feedback")]
    public Renderer beamRenderer;
    public Color balancedColor = Color.white;
    public Color heavyColor = Color.red;

    [Header("Physics Options")]
    public bool useGravity = false;
    public float gravity = 9.81f;

    [Header("Beam Influence")]
    public float beamBiasFactor = 2f;

    [Header("Tilt Settings")]
    public float maxTiltAngle = 40f;
    public float tiltSpeed = 120f;
    public float minTiltSpeed = 10f;
    public bool invertTiltDirection = false;

    private Vector3 beamStartWorldPos;
    private float currentTilt = 0f;

    // -----------------------------
    // STATE CHECK
    // -----------------------------
    bool IsPlaying => !GameControls.IsPaused && !GameControls.IsEditMode;

    void Start()
    {
        beamStartWorldPos = beamTransform.position;

        // Beam slider
        if (beamSlider != null)
        {
            beamSlider.minValue = beamMin;
            beamSlider.maxValue = beamMax;
            beamSlider.value = beamPosition;
            beamSlider.onValueChanged.AddListener(v => beamPosition = v);
        }

        // Position sliders
        if (weight1Slider != null)
        {
            weight1Slider.minValue = 0f;
            weight1Slider.maxValue = 1f;
            weight1Slider.value = Mathf.InverseLerp(weight1MinPosition, weight1MaxPosition, weight1Position);
            weight1Slider.onValueChanged.AddListener(v =>
                weight1Position = Mathf.Lerp(weight1MinPosition, weight1MaxPosition, v));
        }

        if (weight2Slider != null)
        {
            weight2Slider.minValue = 0f;
            weight2Slider.maxValue = 1f;
            weight2Slider.value = Mathf.InverseLerp(weight2MinPosition, weight2MaxPosition, weight2Position);
            weight2Slider.onValueChanged.AddListener(v =>
                weight2Position = Mathf.Lerp(weight2MinPosition, weight2MaxPosition, v));
        }

        // Mass sliders
        if (weight1MassSlider != null)
        {
            weight1MassSlider.minValue = minMass;
            weight1MassSlider.maxValue = maxMass;
            weight1MassSlider.value = weight1Mass;
            weight1MassSlider.onValueChanged.AddListener(v => weight1Mass = v);
        }

        if (weight2MassSlider != null)
        {
            weight2MassSlider.minValue = minMass;
            weight2MassSlider.maxValue = maxMass;
            weight2MassSlider.value = weight2Mass;
            weight2MassSlider.onValueChanged.AddListener(v => weight2Mass = v);
        }

        ApplyAll();
    }

    void Update()
    {
        ApplyAll();
        UpdateSliderLockState();
    }

    // -----------------------------
    // SLIDER LOCK + GREY OUT
    // -----------------------------
    void UpdateSliderLockState()
    {
        bool locked = IsPlaying;

        SetSliderState(beamSlider, locked);
        SetSliderState(weight1Slider, locked);
        SetSliderState(weight2Slider, locked);
        SetSliderState(weight1MassSlider, locked);
        SetSliderState(weight2MassSlider, locked);
    }

    void SetSliderState(Slider slider, bool locked)
    {
        if (slider == null) return;

        slider.interactable = !locked;

        Color targetColor = locked ? sliderDisabledColor : sliderActiveColor;

        Image bg = slider.GetComponent<Image>();
        Image fill = slider.fillRect ? slider.fillRect.GetComponent<Image>() : null;
        Image handle = slider.handleRect ? slider.handleRect.GetComponent<Image>() : null;

        if (bg != null) bg.color = targetColor;
        if (fill != null) fill.color = targetColor;
        if (handle != null) handle.color = targetColor;
    }

    // -----------------------------
    // MAIN LOOP
    // -----------------------------
    void ApplyAll()
    {
        ApplyBeam();

        ApplyWeight(weight1, weight1Position, weight1Text, "Weight 1");
        ApplyWeight(weight2, weight2Position, weight2Text, "Weight 2");

        UpdateMassUI();

        if (IsPlaying)
        {
            ApplyFakePhysics();
        }
    }

    void UpdateMassUI()
    {
        if (weight1MassText != null)
            weight1MassText.text = $"Weight 1 Mass: {weight1Mass:F2} kg";

        if (weight2MassText != null)
            weight2MassText.text = $"Weight 2 Mass: {weight2Mass:F2} kg";
    }

    void ApplyBeam()
    {
        float clamped = Mathf.Clamp(beamPosition, beamMin, beamMax);
        beamTransform.position = beamStartWorldPos + new Vector3(-clamped, 0f, 0f);

        if (beamText != null)
            beamText.text = $"Beam: {clamped:F2} m";
    }

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

    void ApplyFakePhysics()
    {
        float torque =
            (weight1Position * weight1Mass) +
            (weight2Position * weight2Mass);

        torque += beamPosition * beamBiasFactor;

        if (useGravity)
            torque *= gravity;

        float normalized = Mathf.Clamp(torque, -0.5f, 0.5f) / 0.5f;

        float exaggerated = Mathf.Sign(normalized) * normalized * normalized;

        float direction = invertTiltDirection ? -1f : 1f;

        float targetTilt = -exaggerated * maxTiltAngle * direction;

        float dynamicSpeed = tiltSpeed * Mathf.Abs(normalized);
        dynamicSpeed = Mathf.Max(dynamicSpeed, minTiltSpeed);

        currentTilt = Mathf.MoveTowards(
            currentTilt,
            targetTilt,
            dynamicSpeed * Time.deltaTime
        );

        beamTransform.rotation = Quaternion.Euler(0f, 0f, currentTilt);

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