using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MotionGraph : MonoBehaviour
{
    public enum GraphType { Position, Velocity, Acceleration, Force }
    public enum AxisMode { Auto, X, Y, Z }

    [Header("Graph Type")]
    public GraphType graphType = GraphType.Velocity;

    [Header("Axis")]
    public AxisMode axisMode = AxisMode.Auto;

    [Header("UI Elements")]
    public RectTransform graphPanel;
    public RectTransform gridContainer;
    public RectTransform positiveLineContainer;
    public RectTransform negativeLineContainer;
    public TextMeshProUGUI yAxisLabel;
    public TextMeshProUGUI xAxisLabel;
    public TextMeshProUGUI peakText;

    [Header("Graph Settings")]
    public int maxPoints = 150;
    public float updateInterval = 0.05f;
    public float minAutoScale = 1f;
    public int gridLinesX = 6;
    public int gridLinesY = 6;
    public float lineWidth = 2f;

    [Header("Colors")]
    public Color positiveColor = Color.green;
    public Color negativeColor = Color.red;
    public Color gridColor = new Color(1, 1, 1, 0.15f);
    public Color zeroLineColor = Color.white;

    [Header("Target Objects")]
    public Rigidbody targetRigidbody;
    public Transform targetTransform;
    public Transform referenceTransform;

    [Header("Movement & Accel Settings")]
    public float movementThreshold = 0.01f;
    public float maxReasonableAccel = 50f;
    public float accelSmoothFactor = 0.25f;
    public float axisSwitchThreshold = 0.05f;

    [Header("Calibration")]
    public float accelScale = 1f;

    [Header("Velocity Scaling")]
    public float velocityScale = 1f;

    private float[] values;
    private int index;
    private float timer;

    private Vector3 physicsPrevVelocity = Vector3.zero;
    private Vector3 physicsVelocity = Vector3.zero;
    private Vector3 physicsAcceleration = Vector3.zero;
    private Vector3 smoothedAcceleration = Vector3.zero;

    private Vector3 prevPositionForFallback = Vector3.zero;
    private Vector3 initialRelativePosition = Vector3.zero;
    private bool prevPosInitialized = false;

    private float peakPosition = 0f;
    private float peakVelocity = 0f;
    private float peakAcceleration = 0f;
    private float peakForce = 0f;

    private bool graphFrozen = false;
    private float lastRecordedValue = 0f;

    private AxisMode currentAutoAxis = AxisMode.Y;

    void Awake()
    {
        values = new float[maxPoints];

        SetupContainer(gridContainer);
        SetupContainer(positiveLineContainer);
        SetupContainer(negativeLineContainer);

        if (peakText)
            peakText.text = $"{graphType} Peak: 0.00 {GetUnits()}";

        if (xAxisLabel)
            xAxisLabel.text = "";

        InitializeTrackingState();
        RedrawGraph();
    }

    void FixedUpdate()
    {
        if (!HasValidTarget())
            return;

        if (targetRigidbody)
        {
            Vector3 v = GetCurrentVelocityVector();
            physicsAcceleration = (v - physicsPrevVelocity) / Mathf.Max(Time.fixedDeltaTime, 1e-9f);

            physicsPrevVelocity = v;
            physicsVelocity = v;
        }
        else
        {
            Vector3 pos = GetCurrentPositionVector();

            if (!prevPosInitialized)
            {
                prevPositionForFallback = pos;
                prevPosInitialized = true;
                physicsVelocity = Vector3.zero;
                physicsAcceleration = Vector3.zero;
            }
            else
            {
                Vector3 v = (pos - prevPositionForFallback) / Mathf.Max(Time.fixedDeltaTime, 1e-9f);
                physicsAcceleration = (v - physicsVelocity) / Mathf.Max(Time.fixedDeltaTime, 1e-9f);
                physicsVelocity = v;
                prevPositionForFallback = pos;
            }
        }

        physicsAcceleration.x = Mathf.Clamp(physicsAcceleration.x, -maxReasonableAccel, maxReasonableAccel);
        physicsAcceleration.y = Mathf.Clamp(physicsAcceleration.y, -maxReasonableAccel, maxReasonableAccel);
        physicsAcceleration.z = Mathf.Clamp(physicsAcceleration.z, -maxReasonableAccel, maxReasonableAccel);

        smoothedAcceleration = Vector3.Lerp(smoothedAcceleration, physicsAcceleration, accelSmoothFactor);

        if (axisMode == AxisMode.Auto)
            UpdateAutoAxis();
    }

    void Update()
    {
        if (!HasValidTarget())
            return;

        timer += Time.deltaTime;
        if (timer < updateInterval) return;
        timer = 0f;

        Vector3 pos = (graphType == GraphType.Position)
            ? GetPositionDisplacementVector()
            : GetCurrentPositionVector();

        Vector3 vel = physicsVelocity;
        Vector3 accel = smoothedAcceleration;

        Vector3 force = Vector3.zero;
        if (targetRigidbody != null)
            force = smoothedAcceleration * targetRigidbody.mass;

        float posValue = GetAxisValue(pos);
        float velValue = GetAxisValue(vel);
        float accelValue = GetAxisValue(accel);
        float forceValue = GetAxisValue(force);

        bool isMoving = vel.magnitude > movementThreshold;
        if (!isMoving)
        {
            if (!graphFrozen)
            {
                graphFrozen = true;
                lastRecordedValue = (index == 0) ? values[maxPoints - 1] : values[index - 1];
            }

            if (yAxisLabel)
            {
                if (graphType == GraphType.Acceleration || graphType == GraphType.Force)
                    yAxisLabel.text = $"{graphType} ({GetAxisLabel()}): {0.00f:F2} {GetUnits()}";
                else
                    yAxisLabel.text = $"{graphType} ({GetAxisLabel()}): {lastRecordedValue:F2} {GetUnits()}";
            }

            UpdatePeakDisplay();
            return;
        }

        if (graphFrozen)
            graphFrozen = false;

        float newValue = 0f;

        switch (graphType)
        {
            case GraphType.Position:
                newValue = posValue;
                peakPosition = Mathf.Max(peakPosition, Mathf.Abs(newValue));
                break;

            case GraphType.Velocity:
                newValue = velValue * velocityScale;
                peakVelocity = Mathf.Max(peakVelocity, Mathf.Abs(newValue));
                break;

            case GraphType.Acceleration:
                newValue = accelValue * accelScale;
                peakAcceleration = Mathf.Max(peakAcceleration, Mathf.Abs(newValue));
                break;

            case GraphType.Force:
                newValue = forceValue;
                peakForce = Mathf.Max(peakForce, Mathf.Abs(newValue));
                break;
        }

        lastRecordedValue = newValue;

        AddValue(newValue);
        RedrawGraph();

        if (yAxisLabel)
            yAxisLabel.text = $"{graphType} ({GetAxisLabel()}): {newValue:F2} {GetUnits()}";

        UpdatePeakDisplay();
    }

    public void SetTarget(Transform newTarget, Transform newReference = null)
    {
        targetTransform = newTarget;
        referenceTransform = newReference;

        if (targetTransform != null)
            targetRigidbody = targetTransform.GetComponent<Rigidbody>();
        else
            targetRigidbody = null;

        ResetGraph();
        InitializeTrackingState();
    }

    public void ResetGraph()
    {
        if (values == null || values.Length != maxPoints)
            values = new float[maxPoints];
        else
            System.Array.Clear(values, 0, values.Length);

        index = 0;
        timer = 0f;

        graphFrozen = false;
        lastRecordedValue = 0f;

        peakPosition = 0f;
        peakVelocity = 0f;
        peakAcceleration = 0f;
        peakForce = 0f;

        physicsPrevVelocity = Vector3.zero;
        physicsVelocity = Vector3.zero;
        physicsAcceleration = Vector3.zero;
        smoothedAcceleration = Vector3.zero;

        prevPositionForFallback = Vector3.zero;
        initialRelativePosition = Vector3.zero;
        prevPosInitialized = false;

        currentAutoAxis = AxisMode.Y;

        if (yAxisLabel)
            yAxisLabel.text = $"{graphType}: 0.00 {GetUnits()}";

        if (peakText)
            peakText.text = $"{graphType} Peak: 0.00 {GetUnits()}";

        RedrawGraph();
    }

    private void InitializeTrackingState()
    {
        if (targetTransform)
        {
            Vector3 currentPos = GetCurrentPositionVector();
            prevPositionForFallback = currentPos;
            initialRelativePosition = currentPos;
            prevPosInitialized = true;
        }
        else
        {
            prevPosInitialized = false;
            prevPositionForFallback = Vector3.zero;
            initialRelativePosition = Vector3.zero;
        }

        if (targetRigidbody)
        {
            physicsPrevVelocity = GetCurrentVelocityVector();
            physicsVelocity = physicsPrevVelocity;
            physicsAcceleration = Vector3.zero;
            smoothedAcceleration = Vector3.zero;
        }
        else
        {
            physicsPrevVelocity = Vector3.zero;
            physicsVelocity = Vector3.zero;
            physicsAcceleration = Vector3.zero;
            smoothedAcceleration = Vector3.zero;
        }

        if (axisMode == AxisMode.Auto)
            UpdateAutoAxis();
    }

    private bool HasValidTarget()
    {
        return targetTransform != null;
    }

    private void UpdateAutoAxis()
    {
        Vector3 source = Vector3.zero;

        switch (graphType)
        {
            case GraphType.Position:
                source = GetAbsoluteVector(GetPositionDisplacementVector());
                break;

            case GraphType.Velocity:
                source = GetAbsoluteVector(physicsVelocity);
                break;

            case GraphType.Acceleration:
                source = GetAbsoluteVector(smoothedAcceleration);
                break;

            case GraphType.Force:
                source = targetRigidbody ? GetAbsoluteVector(smoothedAcceleration * targetRigidbody.mass) : Vector3.zero;
                break;
        }

        float currentStrength = GetAxisValue(source, currentAutoAxis);

        AxisMode bestAxis = currentAutoAxis;
        float bestValue = currentStrength;

        if (source.x > bestValue + axisSwitchThreshold)
        {
            bestAxis = AxisMode.X;
            bestValue = source.x;
        }

        if (source.y > bestValue + axisSwitchThreshold)
        {
            bestAxis = AxisMode.Y;
            bestValue = source.y;
        }

        if (source.z > bestValue + axisSwitchThreshold)
        {
            bestAxis = AxisMode.Z;
            bestValue = source.z;
        }

        currentAutoAxis = bestAxis;
    }

    private Vector3 GetCurrentPositionVector()
    {
        if (!targetTransform) return Vector3.zero;

        Vector3 targetPos = targetTransform.position;
        Vector3 refPos = referenceTransform ? referenceTransform.position : Vector3.zero;
        return targetPos - refPos;
    }

    private Vector3 GetPositionDisplacementVector()
    {
        return GetCurrentPositionVector() - initialRelativePosition;
    }

    private Vector3 GetCurrentVelocityVector()
    {
        if (!targetRigidbody) return Vector3.zero;

        Vector3 refVel = Vector3.zero;
        if (referenceTransform)
        {
            Rigidbody rb = referenceTransform.GetComponent<Rigidbody>();
            if (rb) refVel = rb.linearVelocity;
        }

        return targetRigidbody.linearVelocity - refVel;
    }

    private float GetAxisValue(Vector3 v)
    {
        AxisMode axis = axisMode == AxisMode.Auto ? currentAutoAxis : axisMode;
        return GetAxisValue(v, axis);
    }

    private float GetAxisValue(Vector3 v, AxisMode axis)
    {
        switch (axis)
        {
            case AxisMode.X: return v.x;
            case AxisMode.Y: return v.y;
            case AxisMode.Z: return v.z;
            default: return v.x;
        }
    }

    private Vector3 GetAbsoluteVector(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    private string GetAxisLabel()
    {
        AxisMode axis = axisMode == AxisMode.Auto ? currentAutoAxis : axisMode;
        return axis.ToString();
    }

    private string GetUnits()
    {
        switch (graphType)
        {
            case GraphType.Position: return "m";
            case GraphType.Velocity: return "m/s";
            case GraphType.Acceleration: return "m/s²";
            case GraphType.Force: return "N";
            default: return "";
        }
    }

    void AddValue(float v)
    {
        values[index] = v;
        index = (index + 1) % maxPoints;
    }

    void SetupContainer(RectTransform c)
    {
        if (!c || !graphPanel) return;
        c.pivot = new Vector2(0.5f, 0.5f);
        c.anchorMin = c.anchorMax = new Vector2(0.5f, 0.5f);
        c.sizeDelta = graphPanel.sizeDelta;
        c.anchoredPosition = Vector2.zero;
    }

    void RedrawGraph()
    {
        if (!graphPanel) return;

        float width = graphPanel.rect.width;
        float height = graphPanel.rect.height;

        float maxAbs = minAutoScale;
        for (int i = 0; i < maxPoints; i++)
            maxAbs = Mathf.Max(maxAbs, Mathf.Abs(values[i]));

        if (maxAbs <= 0f) maxAbs = 1f;

        float scale = (height / 2f) / maxAbs;

        DrawGrid(width, height);

        foreach (Transform t in positiveLineContainer) Destroy(t.gameObject);
        foreach (Transform t in negativeLineContainer) Destroy(t.gameObject);

        for (int i = 0; i < maxPoints - 1; i++)
        {
            int a = (index + i) % maxPoints;
            int b = (index + i + 1) % maxPoints;

            Vector2 start = new Vector2(-width / 2f + i * (width / (maxPoints - 1)), values[a] * scale);
            Vector2 end = new Vector2(-width / 2f + (i + 1) * (width / (maxPoints - 1)), values[b] * scale);

            if (values[a] >= 0 || values[b] >= 0)
                DrawSegment(positiveLineContainer, start, end, positiveColor);

            if (values[a] < 0 || values[b] < 0)
                DrawSegment(negativeLineContainer, start, end, negativeColor);
        }
    }

    void DrawGrid(float width, float height)
    {
        foreach (Transform t in gridContainer) Destroy(t.gameObject);

        for (int i = 0; i < gridLinesX; i++)
        {
            float x = -width / 2f + i * (width / (gridLinesX - 1));
            DrawGridLine(new Vector2(x, 0), new Vector2(1, height));
        }

        for (int j = 0; j < gridLinesY; j++)
        {
            float y = -height / 2f + j * (height / (gridLinesY - 1));
            DrawGridLine(new Vector2(0, y), new Vector2(width, 1));
        }

        DrawGridLine(new Vector2(0, 0), new Vector2(width, 2), zeroLineColor);
    }

    void DrawGridLine(Vector2 pos, Vector2 size, Color? colorOverride = null)
    {
        GameObject g = new GameObject("GridLine", typeof(Image));
        g.transform.SetParent(gridContainer, false);

        Image img = g.GetComponent<Image>();
        img.color = colorOverride ?? gridColor;

        RectTransform rt = g.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    void DrawSegment(RectTransform container, Vector2 start, Vector2 end, Color col)
    {
        GameObject seg = new GameObject("LineSegment", typeof(Image));
        seg.transform.SetParent(container, false);

        Image img = seg.GetComponent<Image>();
        img.color = col;

        RectTransform rt = seg.GetComponent<RectTransform>();
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);

        Vector2 dir = end - start;
        rt.sizeDelta = new Vector2(dir.magnitude, lineWidth);
        rt.anchoredPosition = start;
        rt.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    void UpdatePeakDisplay()
    {
        if (!peakText) return;

        float peak = 0f;
        switch (graphType)
        {
            case GraphType.Position: peak = peakPosition; break;
            case GraphType.Velocity: peak = peakVelocity; break;
            case GraphType.Acceleration: peak = peakAcceleration; break;
            case GraphType.Force: peak = peakForce; break;
        }

        peakText.text = $"{graphType} Peak ({GetAxisLabel()}): {peak:F2} {GetUnits()}";
    }
}