using UnityEngine;

public class MotionGraphTargetBinder : MonoBehaviour
{
    [Header("References")]
    public PlayModeLockOnCamera playModeCamera;
    public MotionGraph motionGraph;

    [Header("Optional")]
    public Transform referenceTransform;

    private Transform lastTarget;

    void Update()
    {
        if (playModeCamera == null || motionGraph == null)
            return;

        Transform currentTarget = playModeCamera.GetCurrentTarget();

        if (!playModeCamera.IsLockedOn() || currentTarget == null)
        {
            if (lastTarget != null)
            {
                ClearGraphTarget();
                lastTarget = null;
            }
            return;
        }

        if (currentTarget != lastTarget)
        {
            ApplyGraphTarget(currentTarget);
            lastTarget = currentTarget;
        }
    }

    void ApplyGraphTarget(Transform target)
    {
        motionGraph.targetTransform = target;
        motionGraph.targetRigidbody = target.GetComponent<Rigidbody>();
        motionGraph.referenceTransform = referenceTransform;
    }

    void ClearGraphTarget()
    {
        motionGraph.targetTransform = null;
        motionGraph.targetRigidbody = null;
    }
}
