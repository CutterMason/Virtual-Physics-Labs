using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ForceTableAdjustmentPanel : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider p1Slider;
    [SerializeField] private Slider p2Slider;
    [SerializeField] private Slider p3Slider;
    [SerializeField] private Slider weight1Slider;
    [SerializeField] private Slider weight2Slider;
    [SerializeField] private Slider weight3Slider;

    [Header("Value Labels")]
    [SerializeField] private TMP_Text angle1Value;
    [SerializeField] private TMP_Text angle2Value;
    [SerializeField] private TMP_Text angle3Value;
    [SerializeField] private TMP_Text weight1Value;
    [SerializeField] private TMP_Text weight2Value;
    [SerializeField] private TMP_Text weight3Value;

    [Header("Lab Reference")]
    [SerializeField] private ForceTableLabController labController;

    [Header("Ranges")]
    [SerializeField] private float pulleyMin = 0f;
    [SerializeField] private float pulleyMax = 360f;
    [SerializeField] private float weightMin = 0f;
    [SerializeField] private float weightMax = 5f;

    private void OnEnable()
    {
        GameControls.OnEditModeChanged += HandleEditModeChanged;
    }

    private void OnDisable()
    {
        GameControls.OnEditModeChanged -= HandleEditModeChanged;
    }

    private void Start()
    {
        SetupSliderRanges();
        RegisterSliderEvents();
        RefreshUIState();
    }

    private void Update()
    {
        UpdateSliderInteractableState();
    }

    public void RefreshUIState()
    {
        SyncSlidersFromLab();
        UpdateAllLabels();
        UpdateSliderInteractableState();
    }

    private void SetupSliderRanges()
    {
        SetupSlider(p1Slider, pulleyMin, pulleyMax, true);
        SetupSlider(p2Slider, pulleyMin, pulleyMax, true);
        SetupSlider(p3Slider, pulleyMin, pulleyMax, true);

        SetupSlider(weight1Slider, weightMin, weightMax, false);
        SetupSlider(weight2Slider, weightMin, weightMax, false);
        SetupSlider(weight3Slider, weightMin, weightMax, false);
    }

    private void SetupSlider(Slider slider, float min, float max, bool wholeNumbers)
    {
        if (slider == null) return;

        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = wholeNumbers;
    }

    private void RegisterSliderEvents()
    {
        if (p1Slider != null) p1Slider.onValueChanged.AddListener(_ => OnAnySliderChanged());
        if (p2Slider != null) p2Slider.onValueChanged.AddListener(_ => OnAnySliderChanged());
        if (p3Slider != null) p3Slider.onValueChanged.AddListener(_ => OnAnySliderChanged());

        if (weight1Slider != null) weight1Slider.onValueChanged.AddListener(_ => OnAnySliderChanged());
        if (weight2Slider != null) weight2Slider.onValueChanged.AddListener(_ => OnAnySliderChanged());
        if (weight3Slider != null) weight3Slider.onValueChanged.AddListener(_ => OnAnySliderChanged());
    }

    private void OnAnySliderChanged()
    {
        if (!CanEditValues() || labController == null)
            return;

        labController.SetSetupValues(
            p1Slider != null ? p1Slider.value : 0f,
            p2Slider != null ? p2Slider.value : 0f,
            p3Slider != null ? p3Slider.value : 0f,
            weight1Slider != null ? weight1Slider.value : 0f,
            weight2Slider != null ? weight2Slider.value : 0f,
            weight3Slider != null ? weight3Slider.value : 0f
        );

        SyncSlidersFromLab();
        UpdateAllLabels();
    }

    public void SyncSlidersFromLab()
    {
        if (labController == null)
            return;

        if (p1Slider != null) p1Slider.SetValueWithoutNotify(labController.GetAngle1());
        if (p2Slider != null) p2Slider.SetValueWithoutNotify(labController.GetAngle2());
        if (p3Slider != null) p3Slider.SetValueWithoutNotify(labController.GetAngle3());

        if (weight1Slider != null) weight1Slider.SetValueWithoutNotify(labController.GetWeight1());
        if (weight2Slider != null) weight2Slider.SetValueWithoutNotify(labController.GetWeight2());
        if (weight3Slider != null) weight3Slider.SetValueWithoutNotify(labController.GetWeight3());
    }

    private void UpdateAllLabels()
    {
        if (p1Slider != null && angle1Value != null) angle1Value.text = $"{p1Slider.value:F0}°";
        if (p2Slider != null && angle2Value != null) angle2Value.text = $"{p2Slider.value:F0}°";
        if (p3Slider != null && angle3Value != null) angle3Value.text = $"{p3Slider.value:F0}°";

        if (weight1Slider != null && weight1Value != null) weight1Value.text = $"{weight1Slider.value:F1}";
        if (weight2Slider != null && weight2Value != null) weight2Value.text = $"{weight2Slider.value:F1}";
        if (weight3Slider != null && weight3Value != null) weight3Value.text = $"{weight3Slider.value:F1}";
    }

    private void HandleEditModeChanged(bool _)
    {
        RefreshUIState();
    }

    private bool CanEditValues()
    {
        return GameControls.IsEditMode;
    }

    private void UpdateSliderInteractableState()
    {
        bool canEdit = CanEditValues();

        if (p1Slider != null) p1Slider.interactable = canEdit;
        if (p2Slider != null) p2Slider.interactable = canEdit;
        if (p3Slider != null) p3Slider.interactable = canEdit;

        if (weight1Slider != null) weight1Slider.interactable = canEdit;
        if (weight2Slider != null) weight2Slider.interactable = canEdit;
        if (weight3Slider != null) weight3Slider.interactable = canEdit;
    }
}