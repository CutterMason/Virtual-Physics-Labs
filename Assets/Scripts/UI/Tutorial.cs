using System.Collections.Generic;
using UnityEngine;

public class SlideshowController : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject mainPanel;

    [Header("Slides")]
    [SerializeField] private List<GameObject> slides = new List<GameObject>();

    private int currentSlideIndex = 0;

    private void Start()
    {
        ShowSlide(currentSlideIndex);
        if (mainPanel != null)
            mainPanel.SetActive(false);
    }

    private void ShowSlide(int index)
    {
        if (index < 0) index = 0;
        if (index >= slides.Count) index = slides.Count - 1;

        currentSlideIndex = index;

        foreach (GameObject slide in slides)
        {
            slide.SetActive(false);
        }

        if (slides.Count > 0)
        {
            slides[currentSlideIndex].SetActive(true);
        }
    }

    public void NextSlide()
    {
        if (currentSlideIndex < slides.Count - 1)
            ShowSlide(currentSlideIndex + 1);
    }

    public void PreviousSlide()
    {
        if (currentSlideIndex > 0)
            ShowSlide(currentSlideIndex - 1);
    }

    public void Open()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
            mainPanel.transform.SetAsLastSibling(); // bring to front
        }
    }

    public void Close()
    {
        if (mainPanel != null)
            mainPanel.SetActive(false);
    }
}