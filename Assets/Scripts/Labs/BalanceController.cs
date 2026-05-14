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

    [Header("Response Tuning")]
    public float sensitivity = 3f; 
    public float balanceDeadzone = 0.001f; // Try 0.01 for small masses
    public float damping = 5f;            

    private Vector3 beamStartWorldPos;
    private float currentTilt = 0f;
    private float currentVelocity = 0f; 

    bool IsPlaying => !GameControls.IsPaused && !GameControls.IsEditMode;

    void Start()
    {
        beamStartWorldPos = beamTransform.position;

        if (beamSlider != null)
        {
            beamSlider.minValue = beamMin;
            beamSlider.maxValue = beamMax;
            beamPosition = Mathf.Clamp(beamPosition, beamMin, beamMax);
            beamSlider.SetValueWithoutNotify(beamPosition);
            beamSlider.onValueChanged.RemoveAllListeners();
            beamSlider.onValueChanged.AddListener(v => beamPosition = Mathf.Clamp(v, beamMin, beamMax));
        }

        if (weight1Slider != null)
        {
            weight1Slider.onValueChanged.RemoveAllListeners();
            weight1Slider.value = Mathf.InverseLerp(weight1MinPosition, weight1MaxPosition, weight1Position);
            weight1Slider.onValueChanged.AddListener(v => weight1Position = Mathf.Lerp(weight1MinPosition, weight1MaxPosition, v));
        }

        if (weight2Slider != null)
        {
            weight2Slider.onValueChanged.RemoveAllListeners();
            weight2Slider.value = Mathf.InverseLerp(weight2MinPosition, weight2MaxPosition, weight2Position);
            weight2Slider.onValueChanged.AddListener(v => weight2Position = Mathf.Lerp(weight2MinPosition, weight2MaxPosition, v));
        }

        if (weight1MassSlider != null)
        {
            weight1MassSlider.onValueChanged.RemoveAllListeners();
            weight1MassSlider.value = weight1Mass;
            weight1MassSlider.onValueChanged.AddListener(v => weight1Mass = v);
        }

        if (weight2MassSlider != null)
        {
            weight2MassSlider.onValueChanged.RemoveAllListeners();
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
        else
        {
            currentVelocity = 0f;
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
        beamPosition = Mathf.Clamp(beamPosition, beamMin, beamMax);
        float clamped = beamPosition;
        beamTransform.position = beamStartWorldPos + new Vector3(-clamped, 0f, 0f);
        float beamDisplayValue = clamped + 0.5f;
        if (beamText != null)
            beamText.text = $"Beam: {beamDisplayValue:F2} m";
    }

    void ApplyWeight(Transform weight, float value, TMP_Text text, string label)
    {
        float flipped = -value;
        weight.localPosition = new Vector3(flipped, weight.localPosition.y, weight.localPosition.z);
        float displayValue = value + 0.5f;
        if (text != null)
            text.text = $"{label}: {displayValue:F2} m";
    }

    void ApplyFakePhysics()
    {
        // 1. Calculate Raw Torque
        float torque = (weight1Position * weight1Mass) + (weight2Position * weight2Mass);
        torque += beamPosition * beamBiasFactor;

        if (useGravity) torque *= gravity;

        // 2. Dynamic Normalization
        float currentHighestMass = Mathf.Max(weight1Mass, weight2Mass, 0.01f);
        // We normalize against the actual scale of the weights used
        float maxPossibleTorque = (currentHighestMass * 0.5f) + (currentHighestMass * 0.5f) + (Mathf.Abs(beamMax) * beamBiasFactor);
        
        if (useGravity) maxPossibleTorque *= gravity;
        if (maxPossibleTorque <= 0.0001f) maxPossibleTorque = 0.0001f;

        float normalized = torque / maxPossibleTorque;

        // 3. Precision Deadzone
        // For very small weights, we check the raw torque against a tiny epsilon
        float targetTilt;
        bool isActuallyImbalanced = Mathf.Abs(torque) > 0.0001f; 

        if (!isActuallyImbalanced || Mathf.Abs(normalized) < balanceDeadzone)
        {
            targetTilt = 0f; 
        }
        else
        {
            float direction = invertTiltDirection ? -1f : 1f;
            targetTilt = -Mathf.Sign(normalized) * maxTiltAngle * direction;
        }

        // 4. Acceleration with a "Micro-Boost"
        float angleDiff = targetTilt - currentTilt;
        if (Mathf.Abs(angleDiff) > 0.01f)
        {
            // We give a minimum accelPower so 0.05kg weights can actually fight the damping
            float accelPower = Mathf.Clamp01(Mathf.Abs(normalized) * 10f); 
            accelPower = Mathf.Max(accelPower, 0.2f); // The "Floor"

            currentVelocity += Mathf.Sign(angleDiff) * sensitivity * accelPower * 100f * Time.deltaTime;
        }

        // 5. Damping & Movement
        currentVelocity = Mathf.MoveTowards(currentVelocity, 0, damping * Time.deltaTime * 10f);
        currentTilt += currentVelocity * Time.deltaTime;
        currentTilt = Mathf.Clamp(currentTilt, -maxTiltAngle, maxTiltAngle);

        if (Mathf.Abs(currentTilt) >= maxTiltAngle) currentVelocity = 0;

        beamTransform.localRotation = Quaternion.Euler(0f, 0f, currentTilt);

        // UI & Visuals...
        if (torqueText != null) torqueText.text = $"Torque: {torque:F4}"; // Extra precision for UI
    }
}