using UnityEngine;
using System.Collections;

public class InelasticCollision : MonoBehaviour
{
    private ToyCar car;
    private bool merged = false;

    void Start()
    {
        car = GetComponent<ToyCar>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (merged) return;

        ToyCar other = collision.collider.GetComponent<ToyCar>();
        if (other == null) return;

        StartCoroutine(MergeWithJoint(other));
    }

    private IEnumerator MergeWithJoint(ToyCar other)
    {
        merged = true;

        Rigidbody A = car.rb;
        Rigidbody B = other.rb;

        float mA = A.mass;
        float mB = B.mass;

        float vA = A.linearVelocity.x;
        float vB = B.linearVelocity.x;

        float finalV = (mA * vA + mB * vB) / (mA + mB);

        yield return new WaitForFixedUpdate();

        A.linearVelocity = new Vector3(finalV, 0, 0);
        B.linearVelocity = new Vector3(finalV, 0, 0);

        FixedJoint joint = A.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = B;
        joint.breakForce = Mathf.Infinity;
        joint.breakTorque = Mathf.Infinity;

        Debug.Log("Cars stuck with joint, final velocity: " + finalV);
    }
}
