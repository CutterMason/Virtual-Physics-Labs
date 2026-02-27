using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VelocityLineGraph : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform graphPanel;
    public RectTransform lineContainer;
    public TextMeshProUGUI yAxisLabel;
    public TextMeshProUGUI xAxisLabel;

    [Header("Graph Settings")]
    public int maxPoints = 100;
    public float updateInterval = 0.05f;
    public float yScale = 10f; // pixels per unit
    public Color lineColor = Color.green;
    public float lineWidth = 2f;

    [Header("Target Objects")]
    public Rigidbody targetRigidbody;      // The object we're tracking
    public Transform referenceTransform;   // The reference object

    private List<Vector2> points = new List<Vector2>();
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            AddPoint(GetRelativeVelocity());
            DrawLine();
        }
    }

    // Calculates velocity relative to the reference
    float GetRelativeVelocity()
    {
        if (targetRigidbody == null || referenceTransform == null)
            return 0f;

        Vector3 direction = (targetRigidbody.position - referenceTransform.position).normalized;
        float relativeVelocity = Vector3.Dot(targetRigidbody.linearVelocity, direction); // signed
        return relativeVelocity;
    }

    void AddPoint(float value)
    {
        float yPos = value * yScale + graphPanel.rect.height / 2f; // shift zero to middle
        float xPos = points.Count * (graphPanel.rect.width / maxPoints);
        points.Add(new Vector2(xPos, yPos));

        if (points.Count > maxPoints)
        {
            points.RemoveAt(0);
            // shift points left
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = new Vector2(i * (graphPanel.rect.width / maxPoints), points[i].y);
            }
        }

        // Update labels
        if (yAxisLabel != null)
            yAxisLabel.text = $"Velocity: {value:F2}";
        if (xAxisLabel != null)
            xAxisLabel.text = $"Time: {Time.time:F1}s";
    }

    void DrawLine()
    {
        foreach (Transform child in lineContainer)
        {
            Destroy(child.gameObject);
        }

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
