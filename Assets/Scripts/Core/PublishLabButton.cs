using TMPro;
using UnityEngine;

public class PublishLabButton : MonoBehaviour
{
    public TMP_InputField publishNameInput;

    public async void PublishLab()
    {
        if (PublishedLabManager.Instance == null)
        {
            Debug.LogError("[PublishLabButton] PublishedLabManager.Instance is null.");
            return;
        }

        if (publishNameInput == null)
        {
            Debug.LogError("[PublishLabButton] No publishNameInput assigned.");
            return;
        }

        string publishName = publishNameInput.text;

        await PublishedLabManager.Instance.PublishCurrentLab(publishName);
    }
}
