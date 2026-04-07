using UnityEngine;

public class NewtonsFirstLawLab : MonoBehaviour
{
    [Header("Center Point")]
    [SerializeField] private Transform tableCenter;

    [Header("Pulley Objects")]
    [SerializeField] private Transform pulley1;
    [SerializeField] private Transform pulley2;
    [SerializeField] private Transform pulley3;

    [Header("Weight Objects")]
    [SerializeField] private Transform weight1Object;
    [SerializeField] private Transform weight2Object;
    [SerializeField] private Transform weight3Object;

    [Header("Circle Settings")]
    [SerializeField] private float tableRadius = 0.35f;
    [SerializeField] private float pulleyYHeight = 0.0f;
    [SerializeField] private float minSeparationDegrees = 15f;

    [Header("Weight Follow Settings")]
    [SerializeField] private float weightHangDistance = 0.45f;
    [SerializeField] private bool weightsFollowInEditMode = true;

    [Header("Current Values")]
    [SerializeField] private float angle1 = 0f;
    [SerializeField] private float angle2 = 120f;
    [SerializeField] private float angle3 = 240f;

    [SerializeField] private float weight1 = 0.25f;
    [SerializeField] private float weight2 = 0.25f;
    [SerializeField] private float weight3 = 0.25f;

    private Rigidbody weight1Rb;
    private Rigidbody weight2Rb;
    private Rigidbody weight3Rb;

    private void Awake()
    {
        if (weight1Object != null) weight1Rb = weight1Object.GetComponent<Rigidbody>();
        if (weight2Object != null) weight2Rb = weight2Object.GetComponent<Rigidbody>();
        if (weight3Object != null) weight3Rb = weight3Object.GetComponent<Rigidbody>();
    }

    private void Start()
    {
        ClampAngles();
        ApplyValues();
    }

    private void Update()
    {
        // Keep weights visually attached to pulleys while editing.
        if (weightsFollowInEditMode && GameControls.IsPaused && GameControls.IsEditMode)
        {
            FollowWeightsToPulleys();
        }
    }

    private void OnValidate()
    {
        minSeparationDegrees = Mathf.Clamp(minSeparationDegrees, 1f, 90f);
        tableRadius = Mathf.Max(0.01f, tableRadius);
        weightHangDistance = Mathf.Max(0.01f, weightHangDistance);

        ClampAngles();

        if (Application.isPlaying)
            ApplyValues();
    }

    public void SetPulleyValues(float a1, float a2, float a3, float w1, float w2, float w3)
    {
        angle1 = NormalizeAngle(a1);
        angle2 = NormalizeAngle(a2);
        angle3 = NormalizeAngle(a3);

        weight1 = Mathf.Max(0f, w1);
        weight2 = Mathf.Max(0f, w2);
        weight3 = Mathf.Max(0f, w3);

        ClampAngles();
        ApplyValues();
    }

    public void ApplyValues()
    {
        PositionPulley(pulley1, angle1);
        PositionPulley(pulley2, angle2);
        PositionPulley(pulley3, angle3);

        if (weightsFollowInEditMode && GameControls.IsPaused && GameControls.IsEditMode)
        {
            FollowWeightsToPulleys();
        }
    }

    private void ClampAngles()
    {
        angle1 = NormalizeAngle(angle1);
        angle2 = NormalizeAngle(angle2);
        angle3 = NormalizeAngle(angle3);

        angle1 = Mathf.Clamp(angle1, 0f, 360f - (2f * minSeparationDegrees));
        angle2 = Mathf.Clamp(angle2, angle1 + minSeparationDegrees, 360f - minSeparationDegrees);
        angle3 = Mathf.Clamp(angle3, angle2 + minSeparationDegrees, 360f);

        angle1 = NormalizeAngle(angle1);
        angle2 = NormalizeAngle(angle2);
        angle3 = NormalizeAngle(angle3);
    }

    private void PositionPulley(Transform pulley, float angleDegrees)
    {
        if (pulley == null || tableCenter == null)
            return;

        float radians = angleDegrees * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Cos(radians) * tableRadius,
            pulleyYHeight,
            Mathf.Sin(radians) * tableRadius
        );

        pulley.position = tableCenter.position + offset;

        Vector3 lookDirection = tableCenter.position - pulley.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.0001f)
        {
            pulley.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    private void FollowWeightsToPulleys()
    {
        FollowWeight(weight1Object, weight1Rb, pulley1);
        FollowWeight(weight2Object, weight2Rb, pulley2);
        FollowWeight(weight3Object, weight3Rb, pulley3);
    }

    private void FollowWeight(Transform weightObj, Rigidbody weightRb, Transform pulley)
    {
        if (weightObj == null || pulley == null)
            return;

        Vector3 newPosition = pulley.position;
        newPosition.y -= weightHangDistance;

        if (weightRb != null)
        {
            weightRb.linearVelocity = Vector3.zero;
            weightRb.angularVelocity = Vector3.zero;
            weightRb.MovePosition(newPosition);
        }
        else
        {
            weightObj.position = newPosition;
        }
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0f)
            angle += 360f;
        return angle;
    }

    public float GetAngle1() => angle1;
    public float GetAngle2() => angle2;
    public float GetAngle3() => angle3;

    public float GetWeight1() => weight1;
    public float GetWeight2() => weight2;
    public float GetWeight3() => weight3;
}
