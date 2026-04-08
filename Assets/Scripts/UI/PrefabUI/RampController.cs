using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class RampControllerPhysics : MonoBehaviour
{
    public Slider angleSlider;
    public TMP_Text angleText;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;

        if (angleSlider != null)
            angleSlider.onValueChanged.AddListener(UpdateRampAngle);

        UpdateRampAngle(angleSlider.value);
    }

    void UpdateRampAngle(float angle)
    {
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, -angle);

        rb.MoveRotation(targetRotation);

        if (angleText != null)
            angleText.text = angle.ToString("F1") + "°";
    }
}