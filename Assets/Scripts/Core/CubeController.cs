using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CubeController : MonoBehaviour
{
    public Slider forceSlider;
    public TMP_Text forceText;

    public float forceMultiplier = 2f;

    private Rigidbody rb;
  
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float force = forceSlider.value * forceMultiplier;

        rb.AddForce(Vector3.left * force, ForceMode.Force);

        if (forceText != null)
            forceText.text = forceSlider.value.ToString("F1") + " N";
    }
}
