using UnityEngine;
using UnityEngine.UI;

public class EngineeringGunAngleControl : MonoBehaviour
{
    public Transform gunPivot;
    public Slider angleSlider;
    public float minAngle = -20f;
    public float maxAngle = 60f;

    void Start()
    {
        angleSlider.minValue = minAngle;
        angleSlider.maxValue = maxAngle;
    }

    void Update()
    {
        float angle = angleSlider.value;

        // rotate ONLY on X axis
        gunPivot.localRotation = Quaternion.Euler(angle, -90f, 0f);
    }
}