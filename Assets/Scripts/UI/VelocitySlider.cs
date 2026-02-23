using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VelocitySlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private float scale = 0.01f; // 250 -> 2.50

    public float VelocityMS => slider.value * scale;

    private void Awake()
    {
        Refresh();
    }

    private void OnEnable()
    {
        if (slider) slider.onValueChanged.AddListener(OnSliderChanged);
        Refresh();
    }

    private void Start()
    {
        Refresh();
    }

    private void OnDisable()
    {
        if (slider) slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float _)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (!slider || !valueText) return;
        valueText.text = $"{VelocityMS:F2} m/s";
    }
}