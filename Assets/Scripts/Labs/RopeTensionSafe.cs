using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class RopeTensionSafe : MonoBehaviour
{
    [Header("References")]
    public Rigidbody cart;      // cart Rigidbody (moves on Z+)
    public Rigidbody weight;    // hanging mass Rigidbody
    public Transform pulley;    // pulley world position

    [Header("Settings")]
    public float ropeLength = 7.05f;     // set to measured - small epsilon
    public float g = 9.81f;
    public float maxForce = 100f;        // clamp to avoid spikes

    // diagnostic
    Vector3 prevVelZ;
    float measuredAccel = 0f;

    void Start()
    {
        if (cart == null || weight == null || pulley == null)
        {
            Debug.LogError("[RopeTensionSafe] Assign cart, weight and pulley in inspector.");
            enabled = false;
            return;
        }

        // Warn if multiple components accidentally exist in scene
        var all = FindObjectsOfType<RopeTensionSafe>();
        if (all.Length > 1)
            Debug.LogWarning("[RopeTensionSafe] More than one RopeTensionSafe in scene — ensure only one instance controls the system.");

        prevVelZ = cart.linearVelocity;
    }

    void FixedUpdate()
    {
        // measure distance (cart -> pulley) + (weight -> pulley) as rope path
        float d1 = Vector3.Distance(cart.position, pulley.position);
        float d2 = Vector3.Distance(weight.position, pulley.position);
        float path = d1 + d2;

        // only apply when rope is taut
        if (path <= ropeLength) return;

        // compute true tension from actual hanging mass
        float tension = weight.mass * g;

        // compute rope direction at the cart side (from cart toward pulley)
        Vector3 ropeDirCart = (pulley.position - cart.position).normalized;

        // project tension onto world Z axis for cart (only Z+ movement)
        float cartForceAlongZ = Vector3.Dot(ropeDirCart * tension, Vector3.forward);

        // ensure we don't accidentally reverse sign /apply negative pulling
        cartForceAlongZ = Mathf.Max(0f, cartForceAlongZ);

        // clamp force magnitude
        cartForceAlongZ = Mathf.Clamp(cartForceAlongZ, 0f, maxForce);

        // apply force to cart along world Z+
        cart.AddForce(Vector3.forward * cartForceAlongZ, ForceMode.Force);

        // apply force to weight toward pulley (full vector along rope)
        Vector3 ropeDirWeight = (pulley.position - weight.position).normalized;
        Vector3 forceOnWeight = ropeDirWeight * tension; // pulls weight toward pulley
        // clamp magnitude too
        if (forceOnWeight.magnitude > maxForce)
            forceOnWeight = forceOnWeight.normalized * maxForce;

        weight.AddForce(forceOnWeight, ForceMode.Force);

        // Diagnostics: measure acceleration along Z
        float vZ = cart.linearVelocity.z;
        float sampleA = (vZ - prevVelZ.z) / Time.fixedDeltaTime;
        prevVelZ.z = vZ;

        // running low-pass average (simple)
        measuredAccel = measuredAccel * 0.8f + sampleA * 0.2f;

        // Theoretical acceleration if full tension acted along Z (no projection)
        float aTheoreticalFull = (weight.mass * g) / (cart.mass + weight.mass);

        // Theoretical acceleration with projection (approx)
        float aProjected = cartForceAlongZ / (cart.mass + weight.mass);

        Debug.LogFormat("[RopeSafe] path={0:F3} ropeLen={1:F3} cartFz={2:F3}N measuredA={3:F3}m/s2 theoryFull={4:F3} projTheory={5:F3}",
            path, ropeLength, cartForceAlongZ, measuredAccel, aTheoreticalFull, aProjected);
    }
}
