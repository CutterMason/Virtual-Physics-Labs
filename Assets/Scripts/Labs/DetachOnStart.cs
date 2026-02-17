using UnityEngine;

public class DetachOnResume : MonoBehaviour
{
    private bool hasDetached = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Keep marble frozen while part of mechanism
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        // When game is resumed and we haven't detached yet
        if (!hasDetached && !GameControls.IsPaused)
        {
            Detach();
        }
    }

    void Detach()
    {
        transform.SetParent(null, true); // keep world position
        hasDetached = true;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}
