using UnityEngine;

public class PendulumConnector : MonoBehaviour
{
    public Transform swivel; // top pivot point
    public Transform bob;    // swinging object

    public float thickness = 0.1f; // X/Z thickness

    void Update()
    {
        if (swivel == null || bob == null) 
            return;

        // 1. Position at midpoint
        transform.position = (swivel.position + bob.position) / 2f;

        // 2. Point the connector towards the bob
        transform.up = bob.position - swivel.position;

        // 3. Stretch along Y axis to match length
        float distance = Vector3.Distance(swivel.position, bob.position);
        transform.localScale = new Vector3(thickness, distance, thickness);
    }
}
