using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class PropertyPanelV2 : MonoBehaviour
{
    [Header("Sliders")]
    public Slider massSlider;
    public Slider sizeSlider;
    
    [Header("Buttons")]
    public Button applyButton;
    public Button resetButton;
    public Button closeButton;
    public Button deleteButton;

    [Header("Value Labels")]
    public TMP_Text massValueText;
    public TMP_Text sizeValueText;
    public TMP_Text propText;

    [Header("Panel Position")]
    public RectTransform panelRect;
    public float panelOffset = 150f;
    private Camera cam;
    private PhysicsObject targetObject;
    private PropertyManagerV2 manager;

    private float temp_mass;
    private float temp_size;

    void Start()
    {
        cam = Camera.main;
    }

    void PositionPanelNearObject(Transform target)
    {
        if (panelRect == null || cam == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(target.position);

        float left = screenPos.x;
        float right = Screen.width - screenPos.x;
        float bottom = screenPos.y;
        float top = Screen.height - screenPos.y;

        Vector3 panelPos = screenPos;
        float max = Mathf.Max(left, right, top, bottom);

        if (max == right)
            panelPos += Vector3.right * panelOffset;
        else if (max == left)
            panelPos += Vector3.left * panelOffset;
        else if (max == top)
            panelPos += Vector3.up * panelOffset;
        else
            panelPos += Vector3.down * panelOffset;

        panelRect.position = panelPos;
    }

    public void Open(GameObject obj, PropertyManagerV2 mgr)
    {
        manager = mgr;
        targetObject = obj.GetComponent<PhysicsObject>();
       
        if (targetObject == null)
        {
            Debug.LogWarning($"No PhysicsObject script found on {obj.name}");
            gameObject.SetActive(false);
            return;
        }
        if(propText != null)
            propText.text = $"Selected: {obj.name}";

        PositionPanelNearObject(obj.transform);

        massSlider.value = targetObject.mass;
        sizeSlider.value = targetObject.size.x;

        temp_mass = massSlider.value;
        temp_size = sizeSlider.value;

        massSlider.onValueChanged.RemoveAllListeners();
        sizeSlider.onValueChanged.RemoveAllListeners();
        applyButton.onClick.RemoveAllListeners();
        resetButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        massSlider.onValueChanged.AddListener(v => {
            temp_mass = v;
            UpdateValueLabel(massValueText, v, "kg");
        });
        sizeSlider.onValueChanged.AddListener(v => {
            temp_size = v;
            UpdateValueLabel(sizeValueText, v, "m³");
        });

        applyButton.onClick.AddListener(ApplyChanges);
        resetButton.onClick.AddListener(ResetToOriginal);
        closeButton.onClick.AddListener(ClosePanel);

        // Update labels with units
        UpdateValueLabel(massValueText, massSlider.value, "kg");
        UpdateValueLabel(sizeValueText, sizeSlider.value, "m³");

        gameObject.SetActive(true);
    }

    private void UpdateValueLabel(TMP_Text label, float value, string unit)
    {
        if (label != null)
            label.text = $"{value:0.00} {unit}";
    }

    public void ApplyChanges()
    {
        if (targetObject == null) return;

        targetObject.ApplyChanges(temp_mass, Vector3.one * temp_size);
    }

    public void ResetToOriginal()
    {
        if (targetObject == null) return;

        targetObject.ResetToOriginal();

        massSlider.value = targetObject.mass;
        sizeSlider.value = targetObject.size.x;

        temp_mass = massSlider.value;
        temp_size = sizeSlider.value;

        UpdateValueLabel(massValueText, temp_mass, "kg");
        UpdateValueLabel(sizeValueText, temp_size, "m³");
    }

    public void ClosePanel()
    {
        if (propText != null)
            propText.text = "";
        manager.CloseCurrentPanel();
    }
    public void DeleteObject()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("No object selected to delete.");
            return;
        }
        Destroy(targetObject.gameObject);
        targetObject = null;
    }
}