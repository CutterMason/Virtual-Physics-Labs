using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ForceSliderUI : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public Slider slider;
    public float returnSpeed = 30f;

    private bool isDragging = false;

    public Outline outline;


    void Start()
    {
        if (outline != null)
            outline.enabled = false; // start off
    }

    void Update()
    {
        if (!isDragging)
        {
            slider.value = Mathf.Lerp(slider.value, 0f, Time.deltaTime * returnSpeed);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void Highlight()
    {
        if (outline != null)
            outline.enabled = true;
    }

    public void Unhighlight()
    {
        if (outline != null)
            outline.enabled = false;
    }
}