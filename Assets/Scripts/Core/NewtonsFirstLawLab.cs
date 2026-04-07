using UnityEngine;

public class NewtonsFirstLawLab : MonoBehaviour
{
    [Header("Pulley Rotators")]
    public Transform pulley1;
    public Transform pulley2;
    public Transform pulley3;

    [Header("Weight Data / Objects")]
    public GameObject weight1Object;
    public GameObject weight2Object;
    public GameObject weight3Object;

    [Header("Stored Current Values")]
    public float angle1;
    public float angle2;
    public float angle3;

    public float weight1;
    public float weight2;
    public float weight3;

    public void SetPulleyValues(float a1, float a2, float a3, float w1, float w2, float w3)
    {
        angle1 = a1;
        angle2 = a2;
        angle3 = a3;

        weight1 = w1;
        weight2 = w2;
        weight3 = w3;

        ApplyValues();
    }

    private void ApplyValues()
    {
        // Change axis if needed depending on your model orientation
        if (pulley1 != null)
            pulley1.localRotation = Quaternion.Euler(0f, angle1, 0f);

        if (pulley2 != null)
            pulley2.localRotation = Quaternion.Euler(0f, angle2, 0f);

        if (pulley3 != null)
            pulley3.localRotation = Quaternion.Euler(0f, angle3, 0f);

    }
}
