using TMPro;
using UnityEngine;

public class PublishUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject publishPanel;
    public TMP_InputField publishNameInput;

    [Header("Optional")]
    public GameObject savePanel;

    private void Start()
    {
        if (publishPanel != null)
            publishPanel.SetActive(false);
    }

    public void OpenPublishPanel()
    {
        if (savePanel != null)
            savePanel.SetActive(false);

        if (publishPanel != null)
            publishPanel.SetActive(true);

        if (publishNameInput != null)
        {
            publishNameInput.text = "";
            publishNameInput.Select();
            publishNameInput.ActivateInputField();
        }
    }

    public void ClosePublishPanel()
    {
        if (publishPanel != null)
            publishPanel.SetActive(false);
    }

    public async void ConfirmPublish()
    {
        if (publishNameInput == null)
        {
            Debug.LogError("[PublishUIManager] publishNameInput is not assigned.");
            return;
        }

        string publishName = publishNameInput.text.Trim();

        if (string.IsNullOrEmpty(publishName))
        {
            Debug.LogWarning("[PublishUIManager] Publish name cannot be empty.");
            return;
        }

        if (PublishedLabManager.Instance == null)
        {
            Debug.LogError("[PublishUIManager] PublishedLabManager.Instance is null.");
            return;
        }

        await PublishedLabManager.Instance.PublishCurrentLab(publishName);

        ClosePublishPanel();
    }
}
