using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Acceleration2Graph : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform graphPanel;            
    public RectTransform gridContainer;         
    public RectTransform positiveLineContainer; 
    public RectTransform negativeLineContainer; 
    public TextMeshProUGUI yAxisLabel;
    public TextMeshProUGUI xAxisLabel;

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
    public Transform referenceTransform;

    private float[] values;
    private int index;
    private float timer;
    private List<RectTransform> gridPool = new List<RectTransform>();

    private Vector3 previousVelocity;

    void Awake()
    {
        values = new float[maxPoints];
        previousVelocity = Vector3.zero;

        SetupContainer(gridContainer);
        SetupContainer(positiveLineContainer);
        SetupContainer(negativeLineContainer);
    }

    void SetupContainer(RectTransform container)
    {
        if (container == null || graphPanel == null) return;
        container.pivot = new Vector2(0.5f, 0.5f);
        container.anchorMin = container.anchorMax = new Vector2(0.5f, 0.5f);
        container.anchoredPosition = Vector2.zero;
        container.sizeDelta = graphPanel.sizeDelta;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            float dt = timer;
            timer = 0f;

            Vector3 currentVelocity = targetRigidbody ? targetRigidbody.linearVelocity : Vector3.zero;
            float relativeCurrentVel = GetRelativeVelocity(currentVelocity);
            float relativePrevVel = GetRelativeVelocity(previousVelocity);

            float acceleration = (relativeCurrentVel - relativePrevVel) / dt;
            previousVelocity = currentVelocity;

            AddValue(acceleration);
            RedrawGraph();

            if (yAxisLabel) yAxisLabel.text = $"Acc: {acceleration:F2}";
            if (xAxisLabel) xAxisLabel.text = $"t={Time.time:F1}s";
        }
    }

    float GetRelativeVelocity(Vector3 vel)
    {
        if (!targetRigidbody || !referenceTransform) return 0f;
        Vector3 dir = (targetRigidbody.position - referenceTransform.position).normalized;
        return Vector3.Dot(vel, dir);
    }

    void AddValue(float v)
    {
        values[index] = v;
        index = (index + 1) % maxPoints;
    }

    void RedrawGraph()
    {
        if (graphPanel == null) return;

        float width = graphPanel.rect.width;
        float height = graphPanel.rect.height;

        // Autoscale Y-axis
        float maxAbs = 1f;
        for (int i = 0; i < maxPoints; i++)
        {
            float val = Mathf.Abs(values[i]);
            if (val > maxAbs) maxAbs = val;
        }
        float scale = (height / 2f) / maxAbs;

        DrawGrid(width, height);

        foreach (Transform t in positiveLineContainer) Destroy(t.gameObject);
        foreach (Transform t in negativeLineContainer) Destroy(t.gameObject);

        for (int i = 0; i < maxPoints - 1; i++)
        {
            int a = (index + i) % maxPoints;
            int b = (index + i + 1) % maxPoints;

            Vector2 start = new Vector2(
                -width / 2f + i * (width / (maxPoints - 1)),
                values[a] * scale
            );
            Vector2 end = new Vector2(
                -width / 2f + (i + 1) * (width / (maxPoints - 1)),
                values[b] * scale
            );

            if (values[a] >= 0 || values[b] >= 0)
                DrawSegment(positiveLineContainer, start, end, positiveColor);
            if (values[a] < 0 || values[b] < 0)
                DrawSegment(negativeLineContainer, start, end, negativeColor);
        }
    }

    void DrawSegment(RectTransform container, Vector2 start, Vector2 end, Color color)
    {
        GameObject segment = new GameObject("LineSegment", typeof(Image));
        segment.transform.SetParent(container, false);
        Image img = segment.GetComponent<Image>();
        img.color = color;

        RectTransform rt = segment.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);

        Vector2 dir = end - start;
        float length = dir.magnitude;
        rt.sizeDelta = new Vector2(length, lineWidth);
        rt.anchoredPosition = start;
        rt.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    void DrawGrid(float width, float height)
    {
        if (gridContainer == null) return;

        int needed = gridLinesX + gridLinesY + 1;
        while (gridPool.Count < needed)
        {
            GameObject g = new GameObject("GridLine", typeof(Image));
            g.transform.SetParent(gridContainer, false);
            Image img = g.GetComponent<Image>();
            img.color = gridColor;

            RectTransform rt = g.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            gridPool.Add(rt);
        }

        // Vertical lines
        for (int i = 0; i < gridLinesX; i++)
        {
            float x = -width / 2f + i * (width / (gridLinesX - 1));
            RectTransform rt = gridPool[i];
            rt.sizeDelta = new Vector2(1, height);
            rt.anchoredPosition = new Vector2(x, 0);
            rt.GetComponent<Image>().color = gridColor;
        }

        // Horizontal lines
        for (int j = 0; j < gridLinesY; j++)
        {
            float y = -height / 2f + j * (height / (gridLinesY - 1));
            RectTransform rt = gridPool[gridLinesX + j];
            rt.sizeDelta = new Vector2(width, 1);
            rt.anchoredPosition = new Vector2(0, y);
            rt.GetComponent<Image>().color = gridColor;
        }

        // Zero line
        RectTransform zeroLine = gridPool[gridLinesX + gridLinesY];
        zeroLine.sizeDelta = new Vector2(width, 2);
        zeroLine.anchoredPosition = Vector2.zero;
        zeroLine.GetComponent<Image>().color = zeroLineColor;
    }
}
