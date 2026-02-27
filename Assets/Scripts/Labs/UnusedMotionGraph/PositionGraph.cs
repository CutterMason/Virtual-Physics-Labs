using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PositionLineGraph : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform graphPanel;
    public RectTransform lineContainer;
    public TextMeshProUGUI yAxisLabel;
    public TextMeshProUGUI xAxisLabel;

    [Header("Graph Settings")]
    public int maxPoints = 100;
    public float updateInterval = 0.05f;
    public float yScale = 10f; // pixels per meter
    public Color lineColor = Color.green;
    public float lineWidth = 2f;

    [Header("Target Objects")]
    public Transform targetObject;        // Position we track
    public Transform referenceTransform;  // Zero point + direction

    private List<Vector2> points = new List<Vector2>();
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            timer = 0f;

            float pos = GetRelativePosition();
            AddPoint(pos);
            DrawLine();
        }
    }

    // Calculate signed position relative to the reference
    float GetRelativePosition()
    {
        if (targetObject == null || referenceTransform == null)
            return 0f;

        // Direction in which we measure position
        Vector3 direction = (targetObject.position - referenceTransform.position).normalized;

        // Vector from reference to target
        Vector3 delta = targetObject.position - referenceTransform.position;

        // Signed distance (project vector onto direction)
        float signedDistance = Vector3.Dot(delta, direction);

        return signedDistance;
    }

    void AddPoint(float value)
    {
        float yPos = value * yScale + graphPanel.rect.height / 2f;
        float xPos = points.Count * (graphPanel.rect.width / maxPoints);

        points.Add(new Vector2(xPos, yPos));

        if (points.Count > maxPoints)
        {
            points.RemoveAt(0);

            for (int i = 0; i < points.Count; i++)
            {
                points[i] = new Vector2(i * (graphPanel.rect.width / maxPoints), points[i].y);
            }
        }

        // Update labels
        if (yAxisLabel != null)
            yAxisLabel.text = $"Position: {value:F2} m";

        if (xAxisLabel != null)
            xAxisLabel.text = $"Time: {Time.time:F1}s";
    }

    void DrawLine()
    {
        foreach (Transform child in lineContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < points.Count - 1; i++)
        {
            DrawSegment(points[i], points[i + 1]);
        }
    }

    void DrawSegment(Vector2 start, Vector2 end)
    {
        GameObject segment = new GameObject("LineSegment", typeof(Image));
        segment.transform.SetParent(lineContainer, false);

        Image img = segment.GetComponent<Image>();
        img.color = lineColor;

        RectTransform rt = segment.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;

        Vector2 dir = end - start;
        float length = dir.magnitude;

        rt.sizeDelta = new Vector2(length, lineWidth);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = start;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0, 0, angle);
    }
}
