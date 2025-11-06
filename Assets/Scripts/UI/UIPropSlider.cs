using System.Runtime.CompilerServices;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PropertyPanel : MonoBehaviour
{
    [Header("Sliders")]
    public Slider massSlider;
    public Slider sizeSlider;
    public Slider speedSlider;

    [Header("Buttons")]
    public Button applyButton;
    public Button resetButton;
    public Button closeButton;

    private PhysicsObject targetObject;
    private PropertyEditorManager manager;
    //previously had live effect on sliders but crashed application, now switch to an apply button
    private float temp_size;
    private float temp_speed;
    private float temp_mass;

    //all methods public to allow Unity to call
    public void Open(GameObject obj, PropertyEditorManager mgr)
    {
        manager = mgr;
        targetObject = obj.GetComponent<PhysicsObject>();
        if (targetObject == null)  //this is used to make sure all assets have script "change_properties"
        {
            Debug.LogWarning($"No PhysicsObject script found on {obj.name}");
            gameObject.SetActive(false);
            return;
        }
        // Load the object's current values into the UI
        sizeSlider.value = targetObject.size.x;   // assuming uniform scaling
        massSlider.value = targetObject.mass;
        speedSlider.value = targetObject.speed;

        temp_mass = massSlider.value;
        temp_size = sizeSlider.value;
        temp_speed = speedSlider.value;

        massSlider.onValueChanged.RemoveAllListeners();
        sizeSlider.onValueChanged.RemoveAllListeners();
        speedSlider.onValueChanged.RemoveAllListeners();

        // Update pending variables when sliders move
        massSlider.onValueChanged.AddListener(v => temp_mass = v);
        sizeSlider.onValueChanged.AddListener(v => temp_size = v);
        speedSlider.onValueChanged.AddListener(v => temp_speed = v);

        // Activate/deactivate speed slider dynamically
        speedSlider.gameObject.SetActive(true);

        applyButton.onClick.RemoveAllListeners();
        applyButton.onClick.AddListener(ApplyChanges);

        resetButton.onClick.RemoveAllListeners();
        resetButton.onClick.AddListener(ResetToOriginal);

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(ClosePanel);
    }

    public void ApplyChanges()
    {
        if (targetObject == null) return;

         targetObject.ApplyChanges(
            massSlider.value,
            Vector3.one * sizeSlider.value,
            speedSlider.value
        );
    }
    private void OnSliderChanged()
    {
        targetObject.ApplyChanges(
            massSlider.value,
            Vector3.one * sizeSlider.value,
            speedSlider.value
        );
    }
    //all the refrences will change part of the asset
    //Ex: car(mass, size, speed)
    public void Masschanged(float value)
    {
        targetObject.ApplyChanges(value, targetObject.size, targetObject.speed);
    }

    public void Sizechanged(float value)
    {
        Vector3 newsize = value * Vector3.one;
        targetObject.ApplyChanges(targetObject.mass, newsize, targetObject.speed); //need to look how to pass vector in new assignment
    }
    
    public void Speedchanged(float value)
    {
        targetObject.ApplyChanges(targetObject.mass, targetObject.size, value);
    }

    public void ResetToOriginal()
    {
        if (targetObject == null) return;

        targetObject.ResetToOriginal();
        // Refresh UI after reset
        sizeSlider.value = targetObject.size.x;
        massSlider.value = targetObject.mass;
        speedSlider.value = targetObject.speed;
        temp_mass = massSlider.value;
        temp_size = sizeSlider.value;
        temp_speed = speedSlider.value;
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false); //makes it inactive instead or ridding completley
    }
}