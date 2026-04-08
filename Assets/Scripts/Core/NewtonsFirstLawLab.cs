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

    [Header("Rope Anchor Points")]
    // For each rope: center->top pulley, top pulley->weight
    [SerializeField] private Transform rope1CenterAnchor;
    [SerializeField] private Transform rope1PulleyTopAnchor;
    [SerializeField] private Transform rope1PulleyBottomAnchor;
    [SerializeField] private Transform rope1WeightAnchor;

    [SerializeField] private Transform rope2CenterAnchor;
    [SerializeField] private Transform rope2PulleyTopAnchor;
    [SerializeField] private Transform rope2PulleyBottomAnchor;
    [SerializeField] private Transform rope2WeightAnchor;

    [SerializeField] private Transform rope3CenterAnchor;
    [SerializeField] private Transform rope3PulleyTopAnchor;
    [SerializeField] private Transform rope3PulleyBottomAnchor;
    [SerializeField] private Transform rope3WeightAnchor;

    [Header("Circle Settings")]
    [SerializeField] private float tableRadius = 0.35f;
    [SerializeField] private float pulleyYHeight = 0.0f;
    [SerializeField] private float minSeparationDegrees = 15f;

    [Header("Weight Follow Settings")]
    [SerializeField] private float weightHangDistance = 0.45f;
    [SerializeField] private bool weightsFollowInEditMode = true;

    [Header("Anchor Offsets")]
    [SerializeField] private Vector3 centerAnchorOffset = Vector3.zero;
    [SerializeField] private Vector3 pulleyTopAnchorOffset = Vector3.zero;
    [SerializeField] private Vector3 pulleyBottomAnchorOffset = Vector3.zero;
    [SerializeField] private Vector3 weightAnchorOffset = Vector3.zero;

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

    private bool weight1OriginalGravity;
    private bool weight2OriginalGravity;
    private bool weight3OriginalGravity;

    private void Awake()
    {
        if (weight1Object != null)
        {
            weight1Rb = weight1Object.GetComponent<Rigidbody>();
            if (weight1Rb == null)
                weight1Rb = weight1Object.GetComponentInChildren<Rigidbody>();

            if (weight1Rb != null)
                weight1OriginalGravity = weight1Rb.useGravity;
        }

        if (weight2Object != null)
        {
            weight2Rb = weight2Object.GetComponent<Rigidbody>();
            if (weight2Rb == null)
                weight2Rb = weight2Object.GetComponentInChildren<Rigidbody>();

            if (weight2Rb != null)
                weight2OriginalGravity = weight2Rb.useGravity;
        }

        if (weight3Object != null)
        {
            weight3Rb = weight3Object.GetComponent<Rigidbody>();
            if (weight3Rb == null)
                weight3Rb = weight3Object.GetComponentInChildren<Rigidbody>();

            if (weight3Rb != null)
                weight3OriginalGravity = weight3Rb.useGravity;
        }

        Debug.Log($"Weight1 RB: {weight1Rb}");
        Debug.Log($"Weight2 RB: {weight2Rb}");
        Debug.Log($"Weight3 RB: {weight3Rb}");
    }

    private void Start()
    {
        ClampAngles();
        ApplyValues();
        UpdateWeightPhysicsState();
    }

    private void Update()
    {

        PositionPulley(pulley1, angle1);
        PositionPulley(pulley2, angle2);
        PositionPulley(pulley3, angle3);


        UpdateWeightPhysicsState();


        if (weightsFollowInEditMode && GameControls.IsPaused && GameControls.IsEditMode)
        {
            FollowWeightsToPulleys();
        }

      
        UpdateAllRopeAnchors();
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

        UpdateAllRopeAnchors();
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
            pulley.rotation = Quaternion.LookRotation(lookDirection);
    }

    private void FollowWeightsToPulleys()
    {
        FollowWeight(weight1Object, weight1Rb, pulley1);
        FollowWeight(weight2Object, weight2Rb, pulley2);
        FollowWeight(weight3Object, weight3Rb, pulley3);
    }

    private void FollowWeight(Transform weightObj, Rigidbody weightRb, Transform pulley)
    {
        if (pulley == null)
            return;

        Vector3 newPosition = pulley.position;
        newPosition.y -= weightHangDistance;

        if (weightRb != null)
        {
            weightRb.position = newPosition;
            weightRb.rotation = Quaternion.identity;
        }
        else if (weightObj != null)
        {
            weightObj.position = newPosition;
        }
    }

    private void UpdateAllRopeAnchors()
    {
        UpdateRopeAnchors(
            pulley1, weight1Object,
            rope1CenterAnchor, rope1PulleyTopAnchor, rope1PulleyBottomAnchor, rope1WeightAnchor
        );

        UpdateRopeAnchors(
            pulley2, weight2Object,
            rope2CenterAnchor, rope2PulleyTopAnchor, rope2PulleyBottomAnchor, rope2WeightAnchor
        );

        UpdateRopeAnchors(
            pulley3, weight3Object,
            rope3CenterAnchor, rope3PulleyTopAnchor, rope3PulleyBottomAnchor, rope3WeightAnchor
        );
    }

    private void UpdateRopeAnchors(
    Transform pulley,
    Transform weightObj,
    Transform centerAnchor,
    Transform pulleyTopAnchor,
    Transform pulleyBottomAnchor,
    Transform weightAnchor)
    {
        

        if (pulley != null && pulleyTopAnchor != null)
            pulleyTopAnchor.position = pulley.position + pulleyTopAnchorOffset;

        if (pulley != null && pulleyBottomAnchor != null)
            pulleyBottomAnchor.position = pulley.position + pulleyBottomAnchorOffset;

        if (weightObj != null && weightAnchor != null)
            weightAnchor.position = weightObj.position + weightAnchorOffset;
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0f)
            angle += 360f;
        return angle;
    }

    private void UpdateWeightPhysicsState()
    {
        bool lockWeights = GameControls.IsPaused && GameControls.IsEditMode;

        SetWeightPhysics(weight1Rb, lockWeights, weight1OriginalGravity);
        SetWeightPhysics(weight2Rb, lockWeights, weight2OriginalGravity);
        SetWeightPhysics(weight3Rb, lockWeights, weight3OriginalGravity);
    }

    private void SetWeightPhysics(Rigidbody rb, bool lockWeights, bool originalGravity)
    {
        if (rb == null) return;

        if (lockWeights)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }
        else
        {
            rb.isKinematic = false;
            rb.useGravity = originalGravity;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public float GetAngle1() => angle1;
    public float GetAngle2() => angle2;
    public float GetAngle3() => angle3;

    public float GetWeight1() => weight1;
    public float GetWeight2() => weight2;
    public float GetWeight3() => weight3;
}
