using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class LockOnCamera : MonoBehaviour
{
    [Header("Edit Mode Gate")]
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

    private Collider targetCollider; // collider used for overlap sizing (we�ll pick a good one)
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
        if (!EditModeActive)
        {
            ForceUnlock();
            return;
        }

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
        RestoreTargetRigidbody();

        if (!isLocked && target == null && currentIndex == -1) return;

        isLocked = false;
        target = null;
        currentIndex = -1;
        targetCollider = null;
        flipView = false;
    }

    // --- IMPORTANT: store *movable roots*, not arbitrary "Scaled" children ---
    void FindAllScaledObjects()
    {
        scaledObjects.Clear();

        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;
            if (!obj.name.Contains("Scaled")) continue;

            // Prefer rigidbody root if present (so movement works after adding RB)
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

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        // Allow click on Scaled object OR a child of a Scaled object
        if (!hit.transform.name.Contains("Scaled") &&
            (hit.transform.root == null || !hit.transform.root.name.Contains("Scaled")))
            return;

        SelectTargetFromHit(hit);
    }

    void SelectTargetFromHit(RaycastHit hit)
    {
        // Grab RB from what you clicked (or its parents)
        Rigidbody rb = hit.rigidbody != null ? hit.rigidbody : hit.transform.GetComponentInParent<Rigidbody>();

        if (rb != null)
        {
            target = rb.transform;   // move RB root
            targetRb = rb;

            // Choose a collider that belongs to that RB (better than "any child collider somewhere")
            // Prefer the collider we actually clicked if it�s on same RB:
            if (hit.collider != null && hit.collider.attachedRigidbody == rb)
                targetCollider = hit.collider;
            else
                targetCollider = rb.GetComponentInChildren<Collider>();
        }
        else
        {
            target = hit.transform;
            targetRb = null;
            targetCollider = hit.collider != null ? hit.collider : target.GetComponent<Collider>();
        }

        isLocked = true;
        flipView = false;

        initialRotation = target.rotation;
        initialPosition = target.position;

        if (targetRb != null)
        {
            targetRbWasKinematic = targetRb.isKinematic;
            targetRbHadGravity = targetRb.useGravity;

            // In edit mode, make it movable and not affected by physics
            targetRb.isKinematic = true;
            targetRb.useGravity = false;
            targetRb.linearVelocity = Vector3.zero;
            targetRb.angularVelocity = Vector3.zero;
        }

        // Keep list fresh (in case you spawned new objects)
        if (scaledObjects.Count == 0) FindAllScaledObjects();
        currentIndex = scaledObjects.IndexOf(target);
    }

    void HandleTabCycle()
    {
        if (scaledObjects.Count == 0)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Unlock previous selection cleanly before switching
            RestoreTargetRigidbody();

            currentIndex++;
            if (currentIndex >= scaledObjects.Count)
                currentIndex = 0;

            target = scaledObjects[currentIndex];
            isLocked = true;
            flipView = false;

            initialRotation = target.rotation;
            initialPosition = target.position;

            // Recompute rb/collider based on the new target root
            targetRb = target.GetComponent<Rigidbody>();
            targetCollider = target.GetComponentInChildren<Collider>();

            if (targetRb != null)
            {
                targetRbWasKinematic = targetRb.isKinematic;
                targetRbHadGravity = targetRb.useGravity;

                targetRb.isKinematic = true;
                targetRb.useGravity = false;
                targetRb.linearVelocity = Vector3.zero;
                targetRb.angularVelocity = Vector3.zero;
            }
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
            ForceUnlock();
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
            // While paused, transform move is most stable; otherwise MovePosition is fine for kinematic RBs
            if (GameControls.IsPaused || targetRb == null)
                target.position = nextPos;
            else
                targetRb.MovePosition(nextPos);

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
            if (GameControls.IsPaused || targetRb == null)
                target.position = nextPos;
            else
                targetRb.MovePosition(nextPos);

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

            if (GameControls.IsPaused) Physics.SyncTransforms();
        }
    }

    bool IsPositionClear(Vector3 nextPos, Quaternion nextRot)
    {
        // This is the big fix: ignore ALL colliders on the same rigidbody/hierarchy,
        // otherwise OverlapBox blocks movement forever once an RB is present.

        Vector3 halfExtents = targetCollider.bounds.extents * 0.98f;

        Collider[] hits = Physics.OverlapBox(
            nextPos,
            halfExtents,
            nextRot,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hits.Length; i++)
        {
            Collider c = hits[i];
            if (!c) continue;

            // Ignore the clicked collider
            if (c == targetCollider) continue;

            // Ignore anything in our own hierarchy (children/parent colliders)
            if (c.transform == target) continue;
            if (c.transform.IsChildOf(target)) continue;

            // Ignore colliders on the same rigidbody (very common)
            if (targetRb != null && c.attachedRigidbody == targetRb) continue;

            return false;
        }

        return true;
    }
}