using UnityEngine;
using TMPro;

public class CubeController : MonoBehaviour
{
    [Header("Force Settings")]
    public ForceSliderUI forceSliderUI;   // single reference to the slider + UI behavior
    public float forceMultiplier = 2f;
    [Header("UI")]
    public TMP_Text forceText;             // show force value for this cube


    [Header("Cube Visuals")]
    public Color selectedColor = Color.yellow;

    private Rigidbody rb;
    private Renderer rend;
    private Color originalColor;

    private static CubeController selectedCube;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    void FixedUpdate()
    {
        if (selectedCube == this && forceSliderUI != null)
        {
            float force = forceSliderUI.slider.value * forceMultiplier;
            rb.AddForce(Vector3.left * force, ForceMode.Force);
        }
        if (forceText != null)
                forceText.text = forceSliderUI.slider.value.ToString("F1") + " N";
    }

    void OnMouseDown()
    {
        if (selectedCube != null && selectedCube != this)
            selectedCube.Deselect();

        selectedCube = this;
        Select();
    }

    void Select()
    {
        rend.material.color = selectedColor;

        if (forceSliderUI != null)
            forceSliderUI.Highlight();
    }

    void Deselect()
    {
        rend.material.color = originalColor;

        if (forceSliderUI != null)
            forceSliderUI.Unhighlight();
    }

    void Update()
    {
        //if slider is clicked it is ignored
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0) && !IsMouseOverThis())
        {
            if (selectedCube == this)
            {
                Deselect();
                selectedCube = null;
            }
        }
    }

    bool IsMouseOverThis()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
            return hit.collider.gameObject == gameObject;
        return false;
    }
}