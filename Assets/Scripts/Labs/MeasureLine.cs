using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class MeasureLine : MonoBehaviour
{
    public Camera cam;
    public LineRenderer lineRenderer;
    public TextMeshProUGUI distanceText;
    bool isMeasuringEnabled = false;

    private List<Vector3> points = new List<Vector3>();
    private float totalDistance = 0f;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        if (!isMeasuringEnabled)
            return;

        if (EventSystem.current != null)
        {
            if (Input.touchCount > 0)
            {
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    return;
            }
            else
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryGetPoint();
        }

        if (points.Count > 0)
        {
            PreviewLine();
        }
    }

    public void ToggleMeasuring()
    {
        isMeasuringEnabled = !isMeasuringEnabled;

        if (!isMeasuringEnabled)
        {
            ResetMeasurement();
        }
    }

    void TryGetPoint()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            points.Add(hit.point);

            if (points.Count > 1)
            {
                totalDistance += Vector3.Distance(
                    points[points.Count - 2],
                    points[points.Count - 1]
                );
            }

            DrawLine();
            UpdateDistanceText();
        }
    }

    void DrawLine()
    {
        lineRenderer.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, points[i]);
        }
    }

    void PreviewLine()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            lineRenderer.positionCount = points.Count + 1;

            for (int i = 0; i < points.Count; i++)
            {
                lineRenderer.SetPosition(i, points[i]);
            }

            lineRenderer.SetPosition(points.Count, hit.point);
        }
    }

    void UpdateDistancePreview()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            float previewDistance = totalDistance;

            if (points.Count > 0)
            {
                previewDistance += Vector3.Distance(
                    points[points.Count - 1],
                    hit.point
                );
            }

            if (distanceText != null)
            {
                distanceText.text = "Distance: " + previewDistance.ToString("F3") + " meters";
            }
        }
    }

    void UpdateDistanceText()
    {
        if (distanceText != null)
        {
            distanceText.text = "Distance: " + totalDistance.ToString("F3") + " meters";
        }
    }

    void ResetMeasurement()
    {
        points.Clear();
        totalDistance = 0f;
        lineRenderer.positionCount = 0;

        if (distanceText != null)
        {
            distanceText.text = "Distance: 0 meters";
        }
    }
}