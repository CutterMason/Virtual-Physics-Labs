using UnityEngine;

public class MotionGraphTargetBinder : MonoBehaviour
{
    [Header("References")]
    public PlayModeLockOnCamera playModeLockOnCamera;
    public MotionGraph velocityGraph;
    public MotionGraph accelerationGraph;
    public MotionGraph positionGraph;

    [Header("Optional")]
    public Transform referenceTransform;

    private Transform currentTarget;

    void Update()
    {
        TryBindTargetFromCamera();
    }

    void TryBindTargetFromCamera()
    {
        Transform target = GetTargetFromCamera();

        if (target == null || target == currentTarget)
            return;

        currentTarget = target;
        ApplyTargetToGraphs(target);
    }

    Transform GetTargetFromCamera()
    {
        if (playModeLockOnCamera == null)
            return null;

        return playModeLockOnCamera.CurrentTarget;
    }

    void ApplyTargetToGraphs(Transform target)
    {
        if (velocityGraph != null)
            velocityGraph.SetTarget(target, referenceTransform);

        if (accelerationGraph != null)
            accelerationGraph.SetTarget(target, referenceTransform);

        if (positionGraph != null)
            positionGraph.SetTarget(target, referenceTransform);
    }
}