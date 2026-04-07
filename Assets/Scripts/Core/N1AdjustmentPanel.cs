using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class N1AdjustmentPanel : MonoBehaviour
{
    [Header("Sliders")]
    public Slider p1Slider;
    public Slider p2Slider;
    public Slider p3Slider;

    public Slider weight1Slider;
    public Slider weight2Slider;
    public Slider weight3Slider;

    [Header("Value Text")]
    public TMP_Text angle1Value;
    public TMP_Text angle2Value;
    public TMP_Text angle3Value;

    public TMP_Text weight1Value;
    public TMP_Text weight2Value;
    public TMP_Text weight3Value;

    [Header("Lab Reference")]
    public NewtonsFirstLawLab labController;

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
        RegisterSliderEvents();
        UpdateAllValueLabels();
        UpdateSliderInteractableState();
        PushUIValuesToLab();
    }

    private void Update()
    {
        // In case pause state changes through GameControls buttons,
        // keep the sliders synced each frame.
        UpdateSliderInteractableState();
    }

    private void RegisterSliderEvents()
    {
        if (p1Slider != null) p1Slider.onValueChanged.AddListener(delegate { OnAnySliderChanged(); });
        if (p2Slider != null) p2Slider.onValueChanged.AddListener(delegate { OnAnySliderChanged(); });
        if (p3Slider != null) p3Slider.onValueChanged.AddListener(delegate { OnAnySliderChanged(); });

        if (weight1Slider != null) weight1Slider.onValueChanged.AddListener(delegate { OnAnySliderChanged(); });
        if (weight2Slider != null) weight2Slider.onValueChanged.AddListener(delegate { OnAnySliderChanged(); });
        if (weight3Slider != null) weight3Slider.onValueChanged.AddListener(delegate { OnAnySliderChanged(); });
    }

    private void OnAnySliderChanged()
    {
        // Extra safety: ignore slider edits unless paused + edit mode
        if (!CanEditValues())
            return;

        UpdateAllValueLabels();
        PushUIValuesToLab();
    }

    private void HandleEditModeChanged(bool isEditing)
    {
        UpdateSliderInteractableState();
    }

    private bool CanEditValues()
    {
        return GameControls.IsPaused && GameControls.IsEditMode;
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

    private void UpdateAllValueLabels()
    {
        if (angle1Value != null && p1Slider != null)
            angle1Value.text = p1Slider.value.ToString("F0") + "°";

        if (angle2Value != null && p2Slider != null)
            angle2Value.text = p2Slider.value.ToString("F0") + "°";

        if (angle3Value != null && p3Slider != null)
            angle3Value.text = p3Slider.value.ToString("F0") + "°";

        if (weight1Value != null && weight1Slider != null)
            weight1Value.text = weight1Slider.value.ToString("F1");

        if (weight2Value != null && weight2Slider != null)
            weight2Value.text = weight2Slider.value.ToString("F1");

        if (weight3Value != null && weight3Slider != null)
            weight3Value.text = weight3Slider.value.ToString("F1");
    }

    public void PushUIValuesToLab()
    {
        if (labController == null) return;

        float a1 = p1Slider != null ? p1Slider.value : 0f;
        float a2 = p2Slider != null ? p2Slider.value : 0f;
        float a3 = p3Slider != null ? p3Slider.value : 0f;

        float w1 = weight1Slider != null ? weight1Slider.value : 0f;
        float w2 = weight2Slider != null ? weight2Slider.value : 0f;
        float w3 = weight3Slider != null ? weight3Slider.value : 0f;

        labController.SetPulleyValues(a1, a2, a3, w1, w2, w3);
    }
}