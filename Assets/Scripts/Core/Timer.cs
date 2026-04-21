using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerController : MonoBehaviour
{
    [Header("Timer UI Elements")]
    public GameObject timerPanel;   
    public TextMeshProUGUI timerText;
    public Button closeButton;                  // Close button on the TimerPanel

    private bool isRunning = false;
    private float timer = 0f;

    private void Start()
    {
        timerPanel.SetActive(false);      //always hidden at start
        closeButton.onClick.AddListener(CloseTimer);
    }

    private void Update()
    {
        // Toggle start/stop
        if (Input.GetKeyDown(KeyCode.T))
            isRunning = !isRunning;

        // Reset
        if (Input.GetKeyDown(KeyCode.N))
        {
            timer = 0;
            UpdateText();
        }

        // Update each frame
        if (isRunning)
        {
            timer += Time.deltaTime;
            UpdateText();
        }
    }

    private void UpdateText()
    {
        int minutes = (int)(timer / 60);
        int seconds = (int)(timer % 60);
        int milliseconds = (int)((timer * 1000) % 1000);

        timerText.text = $"{minutes:00}:{seconds:00}:{milliseconds:000}"; //limit to sec-min as hr is not neccesary
    }

    public void ShowTimer()
    {
        timerPanel.SetActive(true);             //if inactive brings active
    }
    public void CloseTimer()
    {
        isRunning = false;
        timerPanel.SetActive(false);
    }
}
