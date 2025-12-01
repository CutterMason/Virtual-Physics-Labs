using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MotionGraph : MonoBehaviour
{
    public enum GraphType { Position, Velocity, Acceleration }

    [Header("Graph Type")]
    public GraphType graphType = GraphType.Velocity;

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
    public float movementEpsilon = 1e-6f;
    public float movementThreshold = 0.01f;
    public float maxReasonableAccel = 50f;
    public float accelSmoothFactor = 0.25f;

    [Header("Calibration")]
    public float accelScale = 1f;   // scale applied to acceleration values

    [Header("Velocity Scaling")]
    public float velocityScale = 1f;   // <<--- ADDED

    // internal buffers
    private float[] values;
    private int index;
    private float timer;

    private float physicsPrevVelZ = 0f;
    private float physicsVelZ = 0f;
    private float physicsAccelZ = 0f;

    private float prevPosZ_ForFallback = 0f;
    private bool prevPosInitialized = false;

    private float peakPosition = 0f;
    private float peakVelocity = 0f;
    private float peakAcceleration = 0f;

    private float accelSmoothed = 0f;

    private bool graphFrozen = false;
    private float lastRecordedValue = 0f;

    void Awake()
    {
        values = new float[maxPoints];

        SetupContainer(gridContainer);
        SetupContainer(positiveLineContainer);
        SetupContainer(negativeLineContainer);

        if (peakText)
            peakText.text = $"{graphType} Peak: 0.00";

        if (targetTransform)
        {
            prevPosZ_ForFallback = GetCurrentPositionZ();
            prevPosInitialized = true;
        }

        if (targetRigidbody)
        {
            physicsPrevVelZ = GetCurrentVelocityZ();
            physicsVelZ = physicsPrevVelZ;
            physicsAccelZ = 0f;
            accelSmoothed = 0f;
        }
    }

    void FixedUpdate()
    {
        if (targetRigidbody)
        {
            float v = GetCurrentVelocityZ();
            physicsAccelZ = (v - physicsPrevVelZ) / Mathf.Max(Time.fixedDeltaTime, 1e-9f);

            physicsPrevVelZ = v;
            physicsVelZ = v;
        }
        else
        {
            float posZ = GetCurrentPositionZ();

            if (!prevPosInitialized)
            {
                prevPosZ_ForFallback = posZ;
                prevPosInitialized = true;
                physicsVelZ = 0f;
                physicsAccelZ = 0f;
            }
            else
            {
                float v = (posZ - prevPosZ_ForFallback) / Mathf.Max(Time.fixedDeltaTime, 1e-9f);
                physicsAccelZ = (v - physicsVelZ) / Mathf.Max(Time.fixedDeltaTime, 1e-9f);
                physicsVelZ = v;
                prevPosZ_ForFallback = posZ;
            }
        }

        float accelClamped = Mathf.Clamp(physicsAccelZ, -maxReasonableAccel, maxReasonableAccel);
        accelSmoothed = Mathf.Lerp(accelSmoothed, accelClamped, accelSmoothFactor);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < updateInterval) return;
        timer = 0f;

        float posZ = GetCurrentPositionZ();
        float velZ = physicsVelZ;
        float accelZ = accelSmoothed;

        bool isMoving = Mathf.Abs(velZ) > movementThreshold;
        if (!isMoving)
        {
            if (!graphFrozen)
            {
                graphFrozen = true;
                lastRecordedValue = (index == 0) ? values[(maxPoints - 1)] : values[index - 1];
            }

            if (yAxisLabel)
            {
                if (graphType == GraphType.Acceleration)
                    yAxisLabel.text = $"{graphType}: {0.00f:F2}";
                else
                    yAxisLabel.text = $"{graphType}: {lastRecordedValue:F2}";
            }

            if (xAxisLabel) xAxisLabel.text = $"t={Time.time:F1}s";

            UpdatePeakDisplay();
            return;
        }

        if (graphFrozen)
            graphFrozen = false;

        float newValue = 0f;

        switch (graphType)
        {
            case GraphType.Position:
                newValue = posZ;
                peakPosition = Mathf.Max(peakPosition, Mathf.Abs(newValue));
                break;

            case GraphType.Velocity:
                newValue = velZ * velocityScale;   // <<--- APPLIED HERE
                peakVelocity = Mathf.Max(peakVelocity, Mathf.Abs(newValue));
                break;

            case GraphType.Acceleration:
                newValue = accelZ * accelScale;
                peakAcceleration = Mathf.Max(peakAcceleration, Mathf.Abs(newValue));
                break;
        }

        lastRecordedValue = newValue;

        AddValue(newValue);
        RedrawGraph();

        if (yAxisLabel) yAxisLabel.text = $"{graphType}: {newValue:F2}";
        if (xAxisLabel) xAxisLabel.text = $"t={Time.time:F1}s";

        UpdatePeakDisplay();
    }

    float GetCurrentPositionZ()
    {
        if (!targetTransform) return 0f;

        float tz = targetTransform.position.z;
        float rz = referenceTransform ? referenceTransform.position.z : 0f;
        return tz - rz;
    }

    float GetCurrentVelocityZ()
    {
        if (!targetRigidbody) return 0f;

        float refVel = 0f;
        if (referenceTransform)
        {
            Rigidbody rb = referenceTransform.GetComponent<Rigidbody>();
            if (rb) refVel = rb.linearVelocity.z;
        }

        return targetRigidbody.linearVelocity.z - refVel;
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
        }

        peakText.text = $"{graphType} Peak: {peak:F2}";
    }
}
