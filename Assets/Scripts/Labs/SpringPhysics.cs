using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class SpringPhysicsRigidbody : MonoBehaviour
{
    [Range(250f,600f)]
    public Transform targetPoint;
    public float springStrength = 250f;  //changing to 250 for bottom limit as we know 470 matches
    public float damping = 5f;
    public Slider springSlider;
    public TMP_Text SpringValueText;

    private Rigidbody rb;

    private Vector3 lastVelocity;
    private bool suppressOneFrame = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lastVelocity = rb.linearVelocity;

        if (springSlider != null)
        {
            springSlider.minValue = 250f;
            springSlider.maxValue = 600f;
            springSlider.value = springStrength;

            springSlider.onValueChanged.AddListener(SetSpringStrength);
        }

        if (SpringValueText != null)
        {
            SpringValueText.text = "Launcher Strength: " + springStrength.ToString("F0") + " N";
        }
    }

    public void SetSpringStrength(float value)
    {
        springStrength = value;
        if (SpringValueText != null)
        {
            SpringValueText.text = "Launcher Strength: " + springStrength.ToString("F0") + " N";
        }
    }

    void FixedUpdate()
    {
        if (GameControls.IsPaused)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
            return;
        }

        if (targetPoint == null) return;
        if (lastVelocity.sqrMagnitude < 0.0001f && rb.linearVelocity.sqrMagnitude > 0.0001f)
        {
            suppressOneFrame = true;
        }

        lastVelocity = rb.linearVelocity;
        if (suppressOneFrame)
        {
            suppressOneFrame = false;
            return;  
        }
        Vector3 displacement = transform.position - targetPoint.position;
        Vector3 springForce = -springStrength * displacement;
        Vector3 dampingForce = -damping * rb.linearVelocity;

        rb.AddForce(springForce + dampingForce, ForceMode.Force);
    }
}
