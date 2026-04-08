using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ForceSliderUI : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public Slider slider;
    public float returnSpeed = 30f;

    private bool isDragging = false;

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

}