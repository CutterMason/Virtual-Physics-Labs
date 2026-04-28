using UnityEngine;
using System.Collections.Generic;

public class PlayModeLockOnCamera : MonoBehaviour
{
    [Header("Play Mode Gate")]
    private bool PlayModeActive => !GameControls.IsEditMode;

    [Header("Camera Settings")]
    public float distance = 1f;
    [Range(0f, 89f)]
    public float verticalAngle = 45f;
    public float horizontalAngle = 0f;
    public float smoothSpeed = 10f;

    [Header("UI Panels To Show While Locked")]
    public GameObject[] lockOnPanels;

    private readonly List<Transform> scaledObjects = new();
    private Transform target;

    private float targetHorizontalAngle;
    private bool isLocked = false;
    private bool flipView = false;
    private int currentIndex = -1;

    public Transform CurrentTarget => target;

    void Start()
    {
        targetHorizontalAngle = horizontalAngle;

        if (PlayModeActive)
            FindAllScaledObjects();

        SetLockOnPanels(false);
    }

    void Update()
    {
        if (!PlayModeActive)
        {
            ForceUnlock();
            return;
        }

        if (scaledObjects.Count == 0)
            FindAllScaledObjects();

        HandleMouseSelection();
        HandleUnlock();
        HandleTabCycle();
        HandleFlipViewManual();
    }

    void LateUpdate()
    {
        if (!PlayModeActive)
            return;

        if (!isLocked || target == null)
            return;

        float dt = Time.unscaledDeltaTime;

        horizontalAngle = Mathf.LerpAngle(horizontalAngle, targetHorizontalAngle, smoothSpeed * dt);

        float appliedAngle = horizontalAngle;
        if (flipView)
            appliedAngle += 180f;

        float verticalRad = verticalAngle * Mathf.Deg2Rad;
        float horizontalRad = appliedAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            distance * Mathf.Cos(verticalRad) * Mathf.Sin(horizontalRad),
            distance * Mathf.Sin(verticalRad),
            distance * Mathf.Cos(verticalRad) * Mathf.Cos(horizontalRad)
        );

        Vector3 desiredPosition = target.position + offset;

        if (GameControls.IsPaused)
        {
            transform.position = desiredPosition;
            transform.LookAt(target);
            return;
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * dt);
        transform.LookAt(target);
    }

    void ForceUnlock()
    {
        if (!isLocked && target == null && currentIndex == -1)
            return;

        isLocked = false;
        target = null;
        currentIndex = -1;
        flipView = false;

        SetLockOnPanels(false);
    }

    void FindAllScaledObjects()
    {
        scaledObjects.Clear();

        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;
            if (!obj.name.Contains("Scaled")) continue;

            Rigidbody rb = obj.GetComponentInParent<Rigidbody>();
            if (rb != null)
            {
                if (!scaledObjects.Contains(rb.transform))
                    scaledObjects.Add(rb.transform);
            }
            else
            {
                if (!scaledObjects.Contains(obj.transform))
                    scaledObjects.Add(obj.transform);
            }
        }
    }

    void HandleMouseSelection()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Camera cam = Camera.main;
        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        if (!hit.transform.name.Contains("Scaled") &&
            (hit.transform.root == null || !hit.transform.root.name.Contains("Scaled")))
            return;

        SelectTargetFromHit(hit);
    }

    void SelectTargetFromHit(RaycastHit hit)
    {
        Rigidbody rb = hit.rigidbody != null ? hit.rigidbody : hit.transform.GetComponentInParent<Rigidbody>();

        if (rb != null)
            target = rb.transform;
        else
            target = hit.transform;

        isLocked = true;
        flipView = false;

        if (scaledObjects.Count == 0)
            FindAllScaledObjects();

        currentIndex = scaledObjects.IndexOf(target);

        SetLockOnPanels(true);
    }

    void HandleTabCycle()
    {
        if (scaledObjects.Count == 0)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            currentIndex++;
            if (currentIndex >= scaledObjects.Count)
                currentIndex = 0;

            target = scaledObjects[currentIndex];
            isLocked = true;
            flipView = false;

            SetLockOnPanels(true);
        }
    }

    void HandleFlipViewManual()
    {
        if (!isLocked || target == null)
            return;

        if (Input.GetKeyDown(KeyCode.Z))
            flipView = !flipView;
    }

    void HandleUnlock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ForceUnlock();
    }

    void SetLockOnPanels(bool show)
    {
        if (lockOnPanels == null) return;

        for (int i = 0; i < lockOnPanels.Length; i++)
        {
            if (lockOnPanels[i] != null)
                lockOnPanels[i].SetActive(show);
        }
    }

    public void LockOntoTarget(Transform newTarget)
    {
        if (newTarget == null)
            return;

        target = newTarget;
        isLocked = true;
        flipView = false;

        if (scaledObjects.Count == 0)
            FindAllScaledObjects();

        currentIndex = scaledObjects.IndexOf(target);

        SetLockOnPanels(true);
    }

    public bool IsLockedOn => isLocked && target != null;
}