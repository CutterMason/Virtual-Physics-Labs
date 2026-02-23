using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GunAngleControl : MonoBehaviour
{
    public Transform gunPivot;
    public Slider angleSlider;
    public float minAngle = -20f;
    public float maxAngle = 60f;
    public TextMeshProUGUI AngleText;

    void Start()
    {
        angleSlider.minValue = minAngle;
        angleSlider.maxValue = maxAngle;
        Update();
        angleSlider.onValueChanged.AddListener(delegate { Update(); });
    }

    void Update()
    {
        float angle = angleSlider.value;

        // rotate ONLY on X axis
        gunPivot.localRotation = Quaternion.Euler(angle, -90f, 0f);
        AngleText.text = Mathf.Abs(angle).ToString("F1") + "°";
    }
}
