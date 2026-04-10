using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class VerticalSpringOscillator : MonoBehaviour
{
    [Header("Anchor")]
    public Transform springAnchor;

    [Header("Spring Parameters")]
    public float springStrength = 10.65f;
    public float damping = 1.5f;

    [Header("UI Text Fields")]
    public TMP_Text L0Text;
    public TMP_Text LText;

    [Header("Stop Detection")]
    public float stopThreshold = 0.02f;
    public float stopTime = 1f;

    private Rigidbody rb;
    private float L0;

    private float stillTimer = 0f;
    private bool hasSettled = false;

    private float finalL;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        if (springAnchor == null)
        {
            Debug.LogError("Spring Anchor not assigned!");
            return;
        }

        L0 = Vector3.Distance(transform.position, springAnchor.position);

        // hide UI at start
        if (L0Text != null) L0Text.text = "";
        if (LText != null) LText.text = "";
    }

    void FixedUpdate()
    {
        float m = rb.mass;
        float g = 9.81f;
        float dt = Time.fixedDeltaTime;

        float anchorY = springAnchor.position.y;
        float y = transform.position.y;

        // equilibrium offset
        float x_eq = (m * g) / springStrength;
        float equilibriumY = anchorY - (L0 + x_eq);

        // physics
        float displacement = y - equilibriumY;
        float springForce = -springStrength * displacement;
        float dampingForce = -damping * rb.linearVelocity.y;

        float acceleration = (springForce + dampingForce) / m;

        rb.linearVelocity += new Vector3(0, acceleration * dt, 0);
        rb.MovePosition(rb.position + rb.linearVelocity * dt);

        // STOP DETECTION
        float speed = Mathf.Abs(rb.linearVelocity.y);

        if (!hasSettled)
        {
            if (speed < stopThreshold)
            {
                stillTimer += dt;

                if (stillTimer >= stopTime)
                {
                    hasSettled = true;

                    finalL = L0 + x_eq;

                    if (L0Text != null)
                        L0Text.text = "L0: " + L0.ToString("F3");

                    if (LText != null)
                        LText.text = "L: " + finalL.ToString("F3");
                }
            }
            else
            {
                stillTimer = 0f;
            }
        }
    }
}