using UnityEngine;
using TMPro;

public class SaveUIManager : MonoBehaviour
{
    public GameObject savePanel;       // Your UI popup
    public TMP_InputField saveNameInput;
    public SaveManager saveManager;    // Reference to the Firebase save system

    public void OpenSavePanel()
    {
        savePanel.SetActive(true);
    }

    public void CloseSavePanel()
    {
        savePanel.SetActive(false);
        saveNameInput.text = "";
    }

    public async void ConfirmSave()
    {
        string saveName = saveNameInput.text;

        if (string.IsNullOrEmpty(saveName))
        {
            Debug.LogWarning("Save name is empty!");
            return;
        }

        string jsonData = "{ \"placeholder\": true }";

        await saveManager.SaveLab(saveName, jsonData);

        CloseSavePanel();
    }
}
