using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PulleyRopeSystem_Rigid : MonoBehaviour
{
    [Header("References")]
    public Rigidbody cart;
    public Rigidbody hangingWeight;
    public Transform pulleyPoint;

    [Header("Rope Settings")]
    public float ropeLength = 5f;

    private LineRenderer lr;
    private ConfigurableJoint cartJoint;
    private ConfigurableJoint weightJoint;

    void Start()
    {
        if (!cart || !hangingWeight || !pulleyPoint)
        {
            Debug.LogError("Missing references (cart, weight, or pulleyPoint).");
            return;
        }

        // LineRenderer setup
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 3; // cart -> pulley -> weight
        lr.startWidth = 0.03f;
        lr.endWidth = 0.03f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.gray;
        lr.endColor = Color.gray;

        // Cart joint
        cartJoint = cart.gameObject.AddComponent<ConfigurableJoint>();
        cartJoint.autoConfigureConnectedAnchor = false;
        cartJoint.connectedAnchor = pulleyPoint.position;
        cartJoint.xMotion = ConfigurableJointMotion.Limited;
        cartJoint.yMotion = ConfigurableJointMotion.Limited;
        cartJoint.zMotion = ConfigurableJointMotion.Limited;

        SoftJointLimit cartLimit = new SoftJointLimit();
        cartLimit.limit = ropeLength / 2f;
        cartJoint.linearLimit = cartLimit;

        // Weight joint
        weightJoint = hangingWeight.gameObject.AddComponent<ConfigurableJoint>();
        weightJoint.autoConfigureConnectedAnchor = false;
        weightJoint.connectedAnchor = pulleyPoint.position;
        weightJoint.xMotion = ConfigurableJointMotion.Limited;
        weightJoint.yMotion = ConfigurableJointMotion.Limited;
        weightJoint.zMotion = ConfigurableJointMotion.Limited;

        SoftJointLimit weightLimit = new SoftJointLimit();
        weightLimit.limit = ropeLength / 2f;
        weightJoint.linearLimit = weightLimit;
    }

    void LateUpdate()
    {
        if (!lr) return;

        lr.SetPosition(0, cart.position);
        lr.SetPosition(1, pulleyPoint.position);
        lr.SetPosition(2, hangingWeight.position);
    }
}