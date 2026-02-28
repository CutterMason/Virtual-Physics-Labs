using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class LockOnCamera : MonoBehaviour
{
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

    private List<Transform> scaledObjects = new List<Transform>();
    private Transform target;

    private float targetHorizontalAngle;
    private bool isLocked = false;

    private Vector3 currentRotationAxis = Vector3.up;
    private bool flipView = false;

    private Quaternion initialRotation;
    private Vector3 initialPosition;

    private int currentIndex = -1;

    private Collider targetCollider; // store collider of the object being moved

    void Start()
    {
        targetHorizontalAngle = horizontalAngle;
        FindAllScaledObjects();
    }

    void Update()
    {
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
        if (!isLocked || target == null)
            return;

        float appliedAngle = horizontalAngle;
        if (flipView)
            appliedAngle += 180f;

        horizontalAngle = Mathf.LerpAngle(horizontalAngle, targetHorizontalAngle, smoothSpeed * Time.deltaTime);

        float verticalRad = verticalAngle * Mathf.Deg2Rad;
        float horizontalRad = appliedAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            distance * Mathf.Cos(verticalRad) * Mathf.Sin(horizontalRad),
            distance * Mathf.Sin(verticalRad),
            distance * Mathf.Cos(verticalRad) * Mathf.Cos(horizontalRad)
        );

        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.LookAt(target);
    }

    void FindAllScaledObjects()
    {
        scaledObjects.Clear();
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Scaled"))
                scaledObjects.Add(obj.transform);
        }
    }

    void HandleMouseSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.name.Contains("Scaled"))
                {
                    target = hit.transform;
                    isLocked = true;

                    initialRotation = target.rotation;
                    initialPosition = target.position;

                    targetCollider = target.GetComponent<Collider>();

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
            isLocked = false;
            target = null;
            currentIndex = -1;
        }
    }

    void HandleMovement()
    {
        if (!isLocked || target == null || targetCollider == null)
            return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0f, v) * moveSpeed * Time.deltaTime;

        // Only move if the path is clear
        if (!Physics.BoxCast(target.position, targetCollider.bounds.extents, move.normalized, out _, target.rotation, move.magnitude))
        {
            target.Translate(move, Space.World);
        }
    }

    void HandleVerticalMovement()
    {
        if (!isLocked || target == null || targetCollider == null)
            return;

        if (Input.GetKey(KeyCode.Q))
        {
            Vector3 move = Vector3.down * verticalMoveSpeed * Time.deltaTime;
            if (!Physics.BoxCast(target.position, targetCollider.bounds.extents, move.normalized, out _, target.rotation, move.magnitude))
                target.Translate(move, Space.World);
        }

        if (Input.GetKey(KeyCode.E))
        {
            Vector3 move = Vector3.up * verticalMoveSpeed * Time.deltaTime;
            if (!Physics.BoxCast(target.position, targetCollider.bounds.extents, move.normalized, out _, target.rotation, move.magnitude))
                target.Translate(move, Space.World);
        }
    }

    void HandleRotation()
    {
        if (!isLocked || target == null)
            return;

        if (Input.GetKey(KeyCode.R))
            target.Rotate(currentRotationAxis * rotationSpeed * Time.deltaTime, Space.World);
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
}