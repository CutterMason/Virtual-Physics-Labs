using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Player Properties")]
    public float speed = 50f;
    public float size = 1f;
    public float mass = 1f;

    [Header("UI Sliders")]
    public Slider speedSlider;
    public Slider massSlider;
    public Slider sizeSlider;
    public Button ResProp;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Initialize sliders to current values
        if (speedSlider != null)
            speedSlider.value = speed;

        if (massSlider != null)
            massSlider.value = mass;

        // Add listeners to sliders & buttons
        if (speedSlider != null)
            speedSlider.onValueChanged.AddListener(UpdateSpeed);

        if (massSlider != null)
            massSlider.onValueChanged.AddListener(UpdateMass);

        if (sizeSlider != null)
            sizeSlider.onValueChanged.AddListener(UpdateSize);    

        if (ResProp != null)
            ResProp.onClick.AddListener(ResetProperties);    
    }

    void UpdateSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void UpdateMass(float newMass)
    {
        mass = newMass;
        transform.localScale = new Vector3(newMass, newMass, 1f);
    }

    void UpdateSize(float newSize)
    {
        size = newSize;
        transform.localScale = new Vector3(newSize, newSize, 1f);
    }

    void ResetProperties()
    {
        speed = 50f;
        mass = 1f;
        size = 1f;
    }

    void FixedUpdate()
    {
        // Example movement (optional)
        rb.linearVelocity = transform.right * speed * Time.fixedDeltaTime;
    }
}

