using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class LockOnCamera : MonoBehaviour
{
    [Header("Edit Mode Gate")]
    // Change this line if your flag/property is named differently.
    private bool EditModeActive => GameControls.IsEditMode;

    [Header("Camera Settings")]
    public float distance = 1f;
    [Range(0f, 89f)]
    public float verticalAngle = 45f;
    public float horizontalAngle = 0f;
    public float smoothSpeed = 100f;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float verticalMoveSpeed = 3f;
    public float rotationSpeed = 100f;

    private readonly List<Transform> scaledObjects = new();
    private Transform target;

    private float targetHorizontalAngle;
    private bool isLocked = false;

    private Vector3 currentRotationAxis = Vector3.up;
    private bool flipView = false;

    private Quaternion initialRotation;
    private Vector3 initialPosition;

    private int currentIndex = -1;

    private Collider targetCollider; // store collider of the object being moved

    private Rigidbody targetRb;
    private bool targetRbWasKinematic;
    private bool targetRbHadGravity;

    void Start()
    {
        targetHorizontalAngle = horizontalAngle;

        if (EditModeActive)
            FindAllScaledObjects();
    }

    void Update()
    {
        // If we're not in edit mode, hard-disable behavior and exit.
        if (!EditModeActive)
        {
            ForceUnlock();
            return;
        }

        // In edit mode, allow behavior
        HandleMouseSelection();
        HandleUnlock();
        HandleTabCycle();
        HandleFlipViewManual();
        HandleRotation();
        HandleAxisChange();
        HandleTransformReset();
        HandleMovement();
        HandleVerticalMovement();
    }

    void LateUpdate()
    {
        if (!EditModeActive)
            return;

        if (!isLocked || target == null)
            return;

        float dt = Time.unscaledDeltaTime;

        float appliedAngle = horizontalAngle;
        if (flipView)
            appliedAngle += 180f;

        horizontalAngle = Mathf.LerpAngle(horizontalAngle, targetHorizontalAngle, smoothSpeed * dt);

        float verticalRad = verticalAngle * Mathf.Deg2Rad;
        float horizontalRad = appliedAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            distance * Mathf.Cos(verticalRad) * Mathf.Sin(horizontalRad),
            distance * Mathf.Sin(verticalRad),
            distance * Mathf.Cos(verticalRad) * Mathf.Cos(horizontalRad)
        );

        Vector3 desiredPosition = target.position + offset;

        // If paused, snap (prevents micro-jitter from smoothing while editing)
        if (GameControls.IsPaused)
        {
            transform.position = desiredPosition;
            transform.LookAt(target);
            return;
        }

        // Otherwise smooth normally
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * dt);
        transform.LookAt(target);
    }

    void ForceUnlock()
    {
        // Leaving edit mode: stop controlling anything immediately
        if (!isLocked && target == null && currentIndex == -1) return;

        isLocked = false;
        target = null;
        currentIndex = -1;
        targetCollider = null;
        flipView = false;
    }

    void FindAllScaledObjects()
    {
        scaledObjects.Clear();
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj != null && obj.name.Contains("Scaled"))
                scaledObjects.Add(obj.transform);
        }
    }

    void HandleMouseSelection()
    {
        // If physics sim is off, keep collider data in sync for raycasts.
        if (GameControls.IsPaused) Physics.SyncTransforms();

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.name.Contains("Scaled"))
                {
                    target = hit.transform;
                    isLocked = true;

                    initialRotation = target.rotation;
                    initialPosition = target.position;

                    targetCollider = target.GetComponent<Collider>();

                    // --- NEW: handle rigidbody safely while paused/editing ---
                    targetRb = target.GetComponent<Rigidbody>();
                    if (targetRb != null)
                    {
                        targetRbWasKinematic = targetRb.isKinematic;
                        targetRbHadGravity = targetRb.useGravity;

                        // Make it safe to move by transform/MovePosition during pause/edit mode
                        targetRb.isKinematic = true;
                        targetRb.useGravity = false;
                        targetRb.linearVelocity = Vector3.zero;
                        targetRb.angularVelocity = Vector3.zero;
                    }

                    currentIndex = scaledObjects.IndexOf(target);
                    flipView = false;
                }
            }
        }
    }

    void HandleTabCycle()
    {
        if (!isLocked || scaledObjects.Count == 0)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            currentIndex++;
            if (currentIndex >= scaledObjects.Count)
                currentIndex = 0;

            target = scaledObjects[currentIndex];
            initialRotation = target.rotation;
            initialPosition = target.position;
            targetCollider = target.GetComponent<Collider>();
            flipView = false;
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
        {
            RestoreTargetRigidbody();
            isLocked = false;
            target = null;
            currentIndex = -1;
            targetCollider = null;
            flipView = false;
        }
    }

    void RestoreTargetRigidbody()
    {
        if (targetRb != null)
        {
            targetRb.isKinematic = targetRbWasKinematic;
            targetRb.useGravity = targetRbHadGravity;
            targetRb = null;
        }
    }

    void HandleMovement()
    {
        if (!isLocked || target == null || targetCollider == null)
            return;

        float dt = Time.unscaledDeltaTime;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(h, 0f, v) * moveSpeed * dt;
        if (move.sqrMagnitude < 0.000001f)
            return;

        if (GameControls.IsPaused) Physics.SyncTransforms();

        Vector3 nextPos = target.position + move;

        if (IsPositionClear(nextPos, target.rotation))
        {
            // KEY CHANGE: while paused, move transform directly (NOT MovePosition)
            if (GameControls.IsPaused || targetRb == null)
            {
                target.position = nextPos;
            }
            else
            {
                targetRb.MovePosition(nextPos);
            }

            if (GameControls.IsPaused) Physics.SyncTransforms();
        }
    }

    void HandleVerticalMovement()
    {
        if (!isLocked || target == null || targetCollider == null)
            return;

        float dt = Time.unscaledDeltaTime;

        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.Q)) move = Vector3.down * verticalMoveSpeed * dt;
        if (Input.GetKey(KeyCode.E)) move = Vector3.up * verticalMoveSpeed * dt;

        if (move.sqrMagnitude < 0.000001f)
            return;

        if (GameControls.IsPaused) Physics.SyncTransforms();

        Vector3 nextPos = target.position + move;

        if (IsPositionClear(nextPos, target.rotation))
        {
            // KEY CHANGE: while paused, move transform directly (NOT MovePosition)
            if (GameControls.IsPaused || targetRb == null)
            {
                target.position = nextPos;
            }
            else
            {
                targetRb.MovePosition(nextPos);
            }

            if (GameControls.IsPaused) Physics.SyncTransforms();
        }
    }

    void HandleRotation()
    {
        if (!isLocked || target == null)
            return;

        float dt = Time.unscaledDeltaTime;

        if (Input.GetKey(KeyCode.R))
            target.Rotate(currentRotationAxis * rotationSpeed * dt, Space.World);
    }

    void HandleAxisChange()
    {
        if (!isLocked || target == null)
            return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (currentRotationAxis == Vector3.right)
                currentRotationAxis = Vector3.up;
            else if (currentRotationAxis == Vector3.up)
                currentRotationAxis = Vector3.forward;
            else
                currentRotationAxis = Vector3.right;
        }
    }

    void HandleTransformReset()
    {
        if (!isLocked || target == null)
            return;

        if (Input.GetKeyDown(KeyCode.Y))
        {
            target.position = initialPosition;
            target.rotation = initialRotation;
            flipView = false;
        }
    }
    bool IsPositionClear(Vector3 nextPos, Quaternion nextRot)
    {
        Vector3 halfExtents = targetCollider.bounds.extents;

        // Use a slightly smaller box so we don't "stick" on tiny contacts
        halfExtents *= 0.98f;

        Collider[] hits = Physics.OverlapBox(
            nextPos,
            halfExtents,
            nextRot,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;
            if (hits[i] == targetCollider) continue; // ignore self

            // If you want to ignore certain layers, filter here
            return false;
        }

        return true;
    }

}