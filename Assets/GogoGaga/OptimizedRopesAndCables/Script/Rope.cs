using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GogoGaga.OptimizedRopesAndCables
{
    [ExecuteAlways]
    [RequireComponent(typeof(LineRenderer))]
    public class Rope : MonoBehaviour
    {
        public event Action OnPointsChanged;

        [Header("Rope Transforms")]
        [Tooltip("The rope will start at this point")]
        [SerializeField] private Transform startPoint;
        public Transform StartPoint => startPoint;

        [Tooltip("This will move at the center hanging from the rope, like a necklace, for example")]
        [SerializeField] private Transform midPoint;
        public Transform MidPoint => midPoint;

        [Tooltip("The rope will end at this point")]
        [SerializeField] private Transform endPoint;
        public Transform EndPoint => endPoint;

        [Header("Rope Settings")]
        [Tooltip("How many points should the rope have, 2 would be a triangle with straight lines, 100 would be a very flexible rope with many parts")]
        [Range(2, 100)] public int linePoints = 10;

        [Tooltip("Value highly dependent on use case, a metal cable would have high stiffness, a rubber rope would have a low one")]
        public float stiffness = 350f;

        [Tooltip("0 is no damping, 50 is a lot")]
        public float damping = 15f;

        [Tooltip("How long is the rope, it will hang more or less from starting point to end point depending on this value")]
        public float ropeLength = 15f;

        [Tooltip("The Rope width set at start (changing this value during run time will produce no effect)")]
        public float ropeWidth = 0.1f;

        [Header("Rational Bezier Weight Control")]
        [Tooltip("Adjust the middle control point weight for the Rational Bezier curve")]
        [Range(1, 15)] public float midPointWeight = 1f;
        private const float StartPointWeight = 1f;
        private const float EndPointWeight = 1f;

        [Header("Midpoint Position")]
        [Tooltip("Position of the midpoint along the line between start and end points")]
        [Range(0.25f, 0.75f)] public float midPointPosition = 0.5f;

        private Vector3 currentValue;
        private Vector3 currentVelocity;
        private Vector3 targetValue;
        public Vector3 otherPhysicsFactors { get; set; }

        private const float valueThreshold = 0.01f;
        private const float velocityThreshold = 0.01f;

        private LineRenderer lineRenderer;

        private Vector3 prevStartPointPosition;
        private Vector3 prevEndPointPosition;
        private float prevMidPointPosition;
        private float prevMidPointWeight;

        private float prevLineQuality;
        private float prevRopeWidth;
        private float prevStiffness;
        private float prevDamping;
        private float prevRopeLength;

        public bool IsPrefab => gameObject.scene.rootCount == 0;

        private void Start()
        {
            InitializeLineRenderer();

            if (AreEndPointsValid())
            {
                RecalculateRope();
            }
        }

        private void OnValidate()
        {
            InitializeLineRenderer();

            if (!Application.isPlaying)
            {
                if (AreEndPointsValid())
                {
                    RecalculateRope();
                }
                else if (lineRenderer != null)
                {
                    lineRenderer.positionCount = 0;
                }
            }
        }

        private void InitializeLineRenderer()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }

            lineRenderer.startWidth = ropeWidth;
            lineRenderer.endWidth = ropeWidth;
        }

        private void LateUpdate()
        {
            if (IsPrefab)
                return;

            if (!AreEndPointsValid())
                return;

            if (Application.isPlaying)
            {
                // Always redraw after everything else has moved this frame
                SetSplinePointImmediate();
            }
            else
            {
                if (IsPointsMoved() || IsRopeSettingsChanged())
                {
                    RecalculateRope();
                    NotifyPointsChanged();
                }
            }

            CachePreviousValues();
        }

        private bool AreEndPointsValid()
        {
            return startPoint != null && endPoint != null;
        }

        private void SetSplinePointImmediate()
        {
            if (lineRenderer.positionCount != linePoints + 1)
            {
                lineRenderer.positionCount = linePoints + 1;
            }

            // Directly use the true midpoint so the rope does not lag/smear
            Vector3 mid = GetMidPoint();
            targetValue = mid;
            currentValue = mid;
            currentVelocity = Vector3.zero;

            if (midPoint != null)
            {
                midPoint.position = GetRationalBezierPoint(
                    startPoint.position,
                    mid,
                    endPoint.position,
                    midPointPosition,
                    StartPointWeight,
                    midPointWeight,
                    EndPointWeight
                );
            }

            for (int i = 0; i < linePoints; i++)
            {
                float t = i / (float)linePoints;
                Vector3 p = GetRationalBezierPoint(
                    startPoint.position,
                    mid,
                    endPoint.position,
                    t,
                    StartPointWeight,
                    midPointWeight,
                    EndPointWeight
                );

                lineRenderer.SetPosition(i, p);
            }

            lineRenderer.SetPosition(linePoints, endPoint.position);
        }

        private float CalculateYFactorAdjustment(float weight)
        {
            float k = Mathf.Lerp(0.493f, 0.323f, Mathf.InverseLerp(1f, 15f, weight));
            float w = 1f + k * Mathf.Log(weight);
            return w;
        }

        private Vector3 GetMidPoint()
        {
            Vector3 startPointPosition = startPoint.position;
            Vector3 endPointPosition = endPoint.position;
            Vector3 midpos = Vector3.Lerp(startPointPosition, endPointPosition, midPointPosition);

            float yFactor = (ropeLength - Mathf.Min(Vector3.Distance(startPointPosition, endPointPosition), ropeLength))
                            / CalculateYFactorAdjustment(midPointWeight);

            midpos.y -= yFactor;
            return midpos;
        }

        private Vector3 GetRationalBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t, float w0, float w1, float w2)
        {
            Vector3 wp0 = w0 * p0;
            Vector3 wp1 = w1 * p1;
            Vector3 wp2 = w2 * p2;

            float denominator = w0 * Mathf.Pow(1 - t, 2)
                              + 2 * w1 * (1 - t) * t
                              + w2 * Mathf.Pow(t, 2);

            Vector3 point = (wp0 * Mathf.Pow(1 - t, 2)
                           + wp1 * 2 * (1 - t) * t
                           + wp2 * Mathf.Pow(t, 2)) / denominator;

            return point;
        }

        public Vector3 GetPointAt(float t)
        {
            if (!AreEndPointsValid())
            {
                Debug.LogError("StartPoint or EndPoint is not assigned.", gameObject);
                return Vector3.zero;
            }

            return GetRationalBezierPoint(
                startPoint.position,
                currentValue,
                endPoint.position,
                t,
                StartPointWeight,
                midPointWeight,
                EndPointWeight
            );
        }

        private void FixedUpdate()
        {
            if (IsPrefab || !Application.isPlaying || !AreEndPointsValid())
                return;

            // Kept only so the asset still compiles cleanly if something else relies on these values.
            // The rope is now drawn directly in LateUpdate without spring lag.
            SimulatePhysics();
        }

        private void SimulatePhysics()
        {
            float dampingFactor = Mathf.Max(0, 1 - damping * Time.fixedDeltaTime);
            Vector3 acceleration = (targetValue - currentValue) * stiffness * Time.fixedDeltaTime;

            currentVelocity = currentVelocity * dampingFactor + acceleration + otherPhysicsFactors;
            currentValue += currentVelocity * Time.fixedDeltaTime;

            if (Vector3.Distance(currentValue, targetValue) < valueThreshold &&
                currentVelocity.magnitude < velocityThreshold)
            {
                currentValue = targetValue;
                currentVelocity = Vector3.zero;
            }
        }

        private void OnDrawGizmos()
        {
            if (!AreEndPointsValid())
                return;

            Vector3 midPos = GetMidPoint();
        }

        public void SetStartPoint(Transform newStartPoint, bool instantAssign = false)
        {
            startPoint = newStartPoint;
            prevStartPointPosition = startPoint == null ? Vector3.zero : startPoint.position;

            if (instantAssign || newStartPoint == null)
            {
                RecalculateRope();
            }

            NotifyPointsChanged();
        }

        public void SetMidPoint(Transform newMidPoint, bool instantAssign = false)
        {
            midPoint = newMidPoint;
            prevMidPointPosition = midPoint == null ? 0.5f : midPointPosition;

            if (instantAssign || newMidPoint == null)
            {
                RecalculateRope();
            }

            NotifyPointsChanged();
        }

        public void SetEndPoint(Transform newEndPoint, bool instantAssign = false)
        {
            endPoint = newEndPoint;
            prevEndPointPosition = endPoint == null ? Vector3.zero : endPoint.position;

            if (instantAssign || newEndPoint == null)
            {
                RecalculateRope();
            }

            NotifyPointsChanged();
        }

        public void RecalculateRope()
        {
            if (!AreEndPointsValid())
            {
                if (lineRenderer != null)
                {
                    lineRenderer.positionCount = 0;
                }
                return;
            }

            currentValue = GetMidPoint();
            targetValue = currentValue;
            currentVelocity = Vector3.zero;

            SetSplinePointImmediate();
        }

        private void NotifyPointsChanged()
        {
            OnPointsChanged?.Invoke();
        }

        private bool IsPointsMoved()
        {
            bool startPointMoved = startPoint.position != prevStartPointPosition;
            bool endPointMoved = endPoint.position != prevEndPointPosition;
            return startPointMoved || endPointMoved;
        }

        private bool IsRopeSettingsChanged()
        {
            bool lineQualityChanged = !Mathf.Approximately(linePoints, prevLineQuality);
            bool ropeWidthChanged = !Mathf.Approximately(ropeWidth, prevRopeWidth);
            bool stiffnessChanged = !Mathf.Approximately(stiffness, prevStiffness);
            bool dampingChanged = !Mathf.Approximately(damping, prevDamping);
            bool ropeLengthChanged = !Mathf.Approximately(ropeLength, prevRopeLength);
            bool midPointPositionChanged = !Mathf.Approximately(midPointPosition, prevMidPointPosition);
            bool midPointWeightChanged = !Mathf.Approximately(midPointWeight, prevMidPointWeight);

            return lineQualityChanged
                   || ropeWidthChanged
                   || stiffnessChanged
                   || dampingChanged
                   || ropeLengthChanged
                   || midPointPositionChanged
                   || midPointWeightChanged;
        }

        private void CachePreviousValues()
        {
            prevStartPointPosition = startPoint.position;
            prevEndPointPosition = endPoint.position;
            prevMidPointPosition = midPointPosition;
            prevMidPointWeight = midPointWeight;

            prevLineQuality = linePoints;
            prevRopeWidth = ropeWidth;
            prevStiffness = stiffness;
            prevDamping = damping;
            prevRopeLength = ropeLength;
        }
    }
}