using UnityEngine;

public class ForceTableLabController : MonoBehaviour
{
    private enum SimState
    {
        Editing,
        Animating,
        Settled
    }

    [Header("Center")]
    [SerializeField] private Transform tableCenter;
    [SerializeField] private Transform centerRingVisual;

    [Header("Pulleys")]
    [SerializeField] private Transform pulley1;
    [SerializeField] private Transform pulley2;
    [SerializeField] private Transform pulley3;

    [Header("Weights")]
    [SerializeField] private Transform weight1Object;
    [SerializeField] private Transform weight2Object;
    [SerializeField] private Transform weight3Object;

    [Header("Rope Anchors - Rope 1")]
    [SerializeField] private Transform rope1CenterAnchor;
    [SerializeField] private Transform rope1PulleyTopAnchor;
    [SerializeField] private Transform rope1PulleyBottomAnchor;
    [SerializeField] private Transform rope1WeightAnchor;

    [Header("Rope Anchors - Rope 2")]
    [SerializeField] private Transform rope2CenterAnchor;
    [SerializeField] private Transform rope2PulleyTopAnchor;
    [SerializeField] private Transform rope2PulleyBottomAnchor;
    [SerializeField] private Transform rope2WeightAnchor;

    [Header("Rope Anchors - Rope 3")]
    [SerializeField] private Transform rope3CenterAnchor;
    [SerializeField] private Transform rope3PulleyTopAnchor;
    [SerializeField] private Transform rope3PulleyBottomAnchor;
    [SerializeField] private Transform rope3WeightAnchor;

    [Header("Layout")]
    [SerializeField] private float tableRadius = 0.35f;
    [SerializeField] private float pulleyYHeight = 0.0f;
    [SerializeField] private float weightHangDistance = 0.45f;
    [SerializeField] private float minSeparationDegrees = 15f;

    [Header("Simulation")]
    [SerializeField] private float equilibriumTolerance = 0.10f;
    [SerializeField] private float fullMotionNetForce = 3.0f;
    [SerializeField] private float ringShiftDistance = 0.03f;
    [SerializeField] private float weightTravelDistance = 0.08f;
    [SerializeField] private float simulationDuration = 0.8f;

    [Header("Current Setup")]
    [SerializeField] private float angle1 = 60f;
    [SerializeField] private float angle2 = 180f;
    [SerializeField] private float angle3 = 300f;

    [SerializeField] private float weight1 = 1f;
    [SerializeField] private float weight2 = 1f;
    [SerializeField] private float weight3 = 1f;

    [Header("Debug Result")]
    [SerializeField] private bool isInEquilibrium;
    [SerializeField] private Vector2 netForce;
    [SerializeField] private float netForceMagnitude;
    [SerializeField] private float netForceAngleDegrees;

    [Header("Pulley Rotation")]
    [SerializeField] private Vector3 pulleyRotationOffset = new Vector3(0f, 180f, 0f);

    private SimState state = SimState.Editing;

    private Vector3 baseTableCenterPosition;
    private Vector3 baseRingPosition;

    private Vector3 pulley1EditPos;
    private Vector3 pulley2EditPos;
    private Vector3 pulley3EditPos;

    private Vector3 weight1EditPos;
    private Vector3 weight2EditPos;
    private Vector3 weight3EditPos;

    private Vector3 ringAnimStartPos;
    private Vector3 weight1AnimStartPos;
    private Vector3 weight2AnimStartPos;
    private Vector3 weight3AnimStartPos;

    private Vector3 ringAnimTargetPos;
    private Vector3 weight1AnimTargetPos;
    private Vector3 weight2AnimTargetPos;
    private Vector3 weight3AnimTargetPos;

    private float animTimer;

    private Rigidbody weight1Rb;
    private Rigidbody weight2Rb;
    private Rigidbody weight3Rb;

    private Vector3 rope1CenterLocal;
    private Vector3 rope1PulleyTopLocal;
    private Vector3 rope1PulleyBottomLocal;
    private Vector3 rope1WeightLocal;

    private Vector3 rope2CenterLocal;
    private Vector3 rope2PulleyTopLocal;
    private Vector3 rope2PulleyBottomLocal;
    private Vector3 rope2WeightLocal;

    private Vector3 rope3CenterLocal;
    private Vector3 rope3PulleyTopLocal;
    private Vector3 rope3PulleyBottomLocal;
    private Vector3 rope3WeightLocal;

    private void Awake()
    {
        if (tableCenter == null)
        {
            Debug.LogError("ForceTableLabController: Table Center is not assigned.");
            enabled = false;
            return;
        }

        if (centerRingVisual == null)
            centerRingVisual = tableCenter;

        baseTableCenterPosition = tableCenter.position;
        baseRingPosition = centerRingVisual.position;

        weight1Rb = GetWeightRigidbody(weight1Object);
        weight2Rb = GetWeightRigidbody(weight2Object);
        weight3Rb = GetWeightRigidbody(weight3Object);

        ForceWeightKinematic(weight1Rb);
        ForceWeightKinematic(weight2Rb);
        ForceWeightKinematic(weight3Rb);

        CacheAnchorLocalOffsets();
    }

    private void Start()
    {
        ClampAngles();
        BuildEditPose();
        ApplyEditPose();
        UpdateAllRopeAnchors();
    }

    private void Update()
    {
        ForceWeightKinematic(weight1Rb);
        ForceWeightKinematic(weight2Rb);
        ForceWeightKinematic(weight3Rb);

        switch (state)
        {
            case SimState.Editing:
                BuildEditPose();
                ApplyEditPose();
                break;

            case SimState.Animating:
                UpdateAnimation();
                break;

            case SimState.Settled:
                ApplySettledPose();
                break;
        }

        UpdateAllRopeAnchors();
    }

    private void OnValidate()
    {
        tableRadius = Mathf.Max(0.01f, tableRadius);
        weightHangDistance = Mathf.Max(0.01f, weightHangDistance);
        minSeparationDegrees = Mathf.Clamp(minSeparationDegrees, 1f, 90f);
        equilibriumTolerance = Mathf.Max(0.0001f, equilibriumTolerance);
        fullMotionNetForce = Mathf.Max(0.0001f, fullMotionNetForce);
        ringShiftDistance = Mathf.Max(0f, ringShiftDistance);
        weightTravelDistance = Mathf.Max(0f, weightTravelDistance);
        simulationDuration = Mathf.Max(0.01f, simulationDuration);

        ClampAngles();
    }

    public void SetSetupValues(float a1, float a2, float a3, float w1, float w2, float w3)
    {
        angle1 = NormalizeAngle(a1);
        angle2 = NormalizeAngle(a2);
        angle3 = NormalizeAngle(a3);

        weight1 = Mathf.Max(0f, w1);
        weight2 = Mathf.Max(0f, w2);
        weight3 = Mathf.Max(0f, w3);

        ClampAngles();

        if (state == SimState.Editing)
        {
            BuildEditPose();
            ApplyEditPose();
            UpdateAllRopeAnchors();
        }
    }

    public void BeginSimulation()
    {
        ClampAngles();
        BuildEditPose();
        ApplyEditPose();

        ComputeForceResult();

        ringAnimStartPos = centerRingVisual != null ? centerRingVisual.position : baseRingPosition;
        weight1AnimStartPos = weight1Object != null ? weight1Object.position : Vector3.zero;
        weight2AnimStartPos = weight2Object != null ? weight2Object.position : Vector3.zero;
        weight3AnimStartPos = weight3Object != null ? weight3Object.position : Vector3.zero;

        if (isInEquilibrium || netForceMagnitude <= 0.0001f)
        {
            ringAnimTargetPos = baseRingPosition;
            weight1AnimTargetPos = weight1EditPos;
            weight2AnimTargetPos = weight2EditPos;
            weight3AnimTargetPos = weight3EditPos;
        }
        else
        {
            Vector2 netDir2 = netForce.normalized;
            Vector3 ringOffset = new Vector3(netDir2.x, 0f, netDir2.y) * GetMotionStrength01() * ringShiftDistance;

            ringAnimTargetPos = baseRingPosition + ringOffset;

            float moveScale = GetMotionStrength01() * weightTravelDistance;

            weight1AnimTargetPos = weight1EditPos + Vector3.down * (GetAlignment(angle1, netDir2) * moveScale);
            weight2AnimTargetPos = weight2EditPos + Vector3.down * (GetAlignment(angle2, netDir2) * moveScale);
            weight3AnimTargetPos = weight3EditPos + Vector3.down * (GetAlignment(angle3, netDir2) * moveScale);
        }

        animTimer = 0f;
        state = SimState.Animating;
    }

    public void ReturnToEditMode()
    {
        state = SimState.Editing;
        BuildEditPose();
        ApplyEditPose();
        UpdateAllRopeAnchors();
    }

    public void ResetToDefaults()
    {
        angle1 = 60f;
        angle2 = 180f;
        angle3 = 300f;

        weight1 = 1f;
        weight2 = 1f;
        weight3 = 1f;

        ClampAngles();
        ReturnToEditMode();
    }

    private void UpdateAnimation()
    {
        animTimer += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(animTimer / simulationDuration);
        float eased = Mathf.SmoothStep(0f, 1f, t);

        if (centerRingVisual != null)
            centerRingVisual.position = Vector3.Lerp(ringAnimStartPos, ringAnimTargetPos, eased);

        if (weight1Object != null)
            weight1Object.position = Vector3.Lerp(weight1AnimStartPos, weight1AnimTargetPos, eased);

        if (weight2Object != null)
            weight2Object.position = Vector3.Lerp(weight2AnimStartPos, weight2AnimTargetPos, eased);

        if (weight3Object != null)
            weight3Object.position = Vector3.Lerp(weight3AnimStartPos, weight3AnimTargetPos, eased);

        ApplyPulleyPoseForCurrentRing();

        if (t >= 1f)
            state = SimState.Settled;
    }

    private void ApplySettledPose()
    {
        if (centerRingVisual != null)
            centerRingVisual.position = ringAnimTargetPos;

        if (weight1Object != null)
            weight1Object.position = weight1AnimTargetPos;

        if (weight2Object != null)
            weight2Object.position = weight2AnimTargetPos;

        if (weight3Object != null)
            weight3Object.position = weight3AnimTargetPos;

        ApplyPulleyPoseForCurrentRing();
    }

    private void BuildEditPose()
    {
        pulley1EditPos = GetPulleyPosition(angle1);
        pulley2EditPos = GetPulleyPosition(angle2);
        pulley3EditPos = GetPulleyPosition(angle3);

        weight1EditPos = pulley1EditPos + Vector3.down * weightHangDistance;
        weight2EditPos = pulley2EditPos + Vector3.down * weightHangDistance;
        weight3EditPos = pulley3EditPos + Vector3.down * weightHangDistance;
    }

    private void ApplyEditPose()
    {
        if (centerRingVisual != null)
            centerRingVisual.position = baseRingPosition;

        SetPulleyPose(pulley1, pulley1EditPos);
        SetPulleyPose(pulley2, pulley2EditPos);
        SetPulleyPose(pulley3, pulley3EditPos);

        if (weight1Object != null) weight1Object.position = weight1EditPos;
        if (weight2Object != null) weight2Object.position = weight2EditPos;
        if (weight3Object != null) weight3Object.position = weight3EditPos;
    }

    private void ApplyPulleyPoseForCurrentRing()
    {
        SetPulleyPose(pulley1, pulley1EditPos);
        SetPulleyPose(pulley2, pulley2EditPos);
        SetPulleyPose(pulley3, pulley3EditPos);
    }

    private void SetPulleyPose(Transform pulley, Vector3 position)
    {
        if (pulley == null)
            return;

        pulley.position = position;

        Vector3 ringPos = centerRingVisual != null ? centerRingVisual.position : baseRingPosition;
        Vector3 lookDirection = ringPos - pulley.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(lookDirection);
            pulley.rotation = lookRot * Quaternion.Euler(pulleyRotationOffset);
        }
    }

    private Vector3 GetPulleyPosition(float angleDegrees)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;

        return baseTableCenterPosition + new Vector3(
            Mathf.Cos(radians) * tableRadius,
            pulleyYHeight,
            Mathf.Sin(radians) * tableRadius
        );
    }

    private void ComputeForceResult()
    {
        Vector2 f1 = DirectionFromAngle(angle1) * weight1;
        Vector2 f2 = DirectionFromAngle(angle2) * weight2;
        Vector2 f3 = DirectionFromAngle(angle3) * weight3;

        netForce = f1 + f2 + f3;
        netForceMagnitude = netForce.magnitude;

        if (netForceMagnitude > 0.0001f)
            netForceAngleDegrees = NormalizeAngle(Mathf.Atan2(netForce.y, netForce.x) * Mathf.Rad2Deg);
        else
            netForceAngleDegrees = 0f;

        isInEquilibrium = netForceMagnitude <= equilibriumTolerance;
    }

    private float GetMotionStrength01()
    {
        return Mathf.Clamp01(netForceMagnitude / fullMotionNetForce);
    }

    private float GetAlignment(float angleDegrees, Vector2 netDir)
    {
        return Vector2.Dot(DirectionFromAngle(angleDegrees), netDir);
    }

    private Vector2 DirectionFromAngle(float angleDegrees)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
    }

    private void ClampAngles()
    {
        angle1 = NormalizeAngle(angle1);
        angle2 = NormalizeAngle(angle2);
        angle3 = NormalizeAngle(angle3);

        angle1 = Mathf.Clamp(angle1, 0f, 360f - (2f * minSeparationDegrees));
        angle2 = Mathf.Clamp(angle2, angle1 + minSeparationDegrees, 360f - minSeparationDegrees);
        angle3 = Mathf.Clamp(angle3, angle2 + minSeparationDegrees, 360f);
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0f)
            angle += 360f;
        return angle;
    }

    private Rigidbody GetWeightRigidbody(Transform weightTransform)
    {
        if (weightTransform == null)
            return null;

        Rigidbody rb = weightTransform.GetComponent<Rigidbody>();
        if (rb == null)
            rb = weightTransform.GetComponentInChildren<Rigidbody>();

        return rb;
    }

    private void ForceWeightKinematic(Rigidbody rb)
    {
        if (rb == null)
            return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    private void CacheAnchorLocalOffsets()
    {
        rope1CenterLocal = GetLocalOffset(centerRingVisual, rope1CenterAnchor);
        rope1PulleyTopLocal = GetLocalOffset(pulley1, rope1PulleyTopAnchor);
        rope1PulleyBottomLocal = GetLocalOffset(pulley1, rope1PulleyBottomAnchor);
        rope1WeightLocal = GetLocalOffset(weight1Object, rope1WeightAnchor);

        rope2CenterLocal = GetLocalOffset(centerRingVisual, rope2CenterAnchor);
        rope2PulleyTopLocal = GetLocalOffset(pulley2, rope2PulleyTopAnchor);
        rope2PulleyBottomLocal = GetLocalOffset(pulley2, rope2PulleyBottomAnchor);
        rope2WeightLocal = GetLocalOffset(weight2Object, rope2WeightAnchor);

        rope3CenterLocal = GetLocalOffset(centerRingVisual, rope3CenterAnchor);
        rope3PulleyTopLocal = GetLocalOffset(pulley3, rope3PulleyTopAnchor);
        rope3PulleyBottomLocal = GetLocalOffset(pulley3, rope3PulleyBottomAnchor);
        rope3WeightLocal = GetLocalOffset(weight3Object, rope3WeightAnchor);
    }

    private Vector3 GetLocalOffset(Transform parent, Transform child)
    {
        if (parent == null || child == null)
            return Vector3.zero;

        return parent.InverseTransformPoint(child.position);
    }

    private void UpdateAllRopeAnchors()
    {
        UpdateRopeAnchors(
            centerRingVisual, pulley1, weight1Object,
            rope1CenterAnchor, rope1PulleyTopAnchor, rope1PulleyBottomAnchor, rope1WeightAnchor,
            rope1CenterLocal, rope1PulleyTopLocal, rope1PulleyBottomLocal, rope1WeightLocal
        );

        UpdateRopeAnchors(
            centerRingVisual, pulley2, weight2Object,
            rope2CenterAnchor, rope2PulleyTopAnchor, rope2PulleyBottomAnchor, rope2WeightAnchor,
            rope2CenterLocal, rope2PulleyTopLocal, rope2PulleyBottomLocal, rope2WeightLocal
        );

        UpdateRopeAnchors(
            centerRingVisual, pulley3, weight3Object,
            rope3CenterAnchor, rope3PulleyTopAnchor, rope3PulleyBottomAnchor, rope3WeightAnchor,
            rope3CenterLocal, rope3PulleyTopLocal, rope3PulleyBottomLocal, rope3WeightLocal
        );
    }

    private void UpdateRopeAnchors(
        Transform ring,
        Transform pulley,
        Transform weightObj,
        Transform centerAnchor,
        Transform pulleyTopAnchor,
        Transform pulleyBottomAnchor,
        Transform weightAnchor,
        Vector3 centerLocal,
        Vector3 pulleyTopLocal,
        Vector3 pulleyBottomLocal,
        Vector3 weightLocal)
    {
        if (ring != null && centerAnchor != null)
            centerAnchor.position = ring.TransformPoint(centerLocal);

        if (pulley != null && pulleyTopAnchor != null)
            pulleyTopAnchor.position = pulley.TransformPoint(pulleyTopLocal);

        if (pulley != null && pulleyBottomAnchor != null)
            pulleyBottomAnchor.position = pulley.TransformPoint(pulleyBottomLocal);

        if (weightObj != null && weightAnchor != null)
            weightAnchor.position = weightObj.TransformPoint(weightLocal);
    }

    public float GetAngle1() => angle1;
    public float GetAngle2() => angle2;
    public float GetAngle3() => angle3;

    public float GetWeight1() => weight1;
    public float GetWeight2() => weight2;
    public float GetWeight3() => weight3;

    public bool IsInEquilibrium() => isInEquilibrium;
    public Vector2 GetNetForce() => netForce;
    public float GetNetForceMagnitude() => netForceMagnitude;
    public float GetNetForceAngle() => netForceAngleDegrees;
}
