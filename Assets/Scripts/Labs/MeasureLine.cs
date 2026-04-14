using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MeasureLine : MonoBehaviour
{
    public Camera cam;
    public LineRenderer lineRenderer;
    public TextMeshProUGUI distanceText;
    bool isMeasuringEnabled = false;

    private Vector3[] points = new Vector3[2];
    private int pointCount = 0;

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

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            TryGetPoint();
        }
    }

    public void ToggleMeasuring()
    {
        isMeasuringEnabled = !isMeasuringEnabled;

        if (!isMeasuringEnabled)
        {
            pointCount = 0;
            lineRenderer.positionCount = 0;
        }
    }

    void TryGetPoint()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            points[pointCount] = hit.point;
            pointCount++;

            if (pointCount == 2)
            {
                DrawLine();
                DisplayDistance();
                pointCount = 0;
            }
        }
    }

    void DrawLine()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, points[0]);
        lineRenderer.SetPosition(1, points[1]);
    }

    void DisplayDistance()
    {
        float distance = Vector3.Distance(points[0], points[1]);

        if (distanceText != null)
        {
            distanceText.text = "Distance: " + distance.ToString("F6") + " meters";
        }
    }
}