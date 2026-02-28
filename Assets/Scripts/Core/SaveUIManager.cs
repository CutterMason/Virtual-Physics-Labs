using UnityEngine;
using TMPro;

public class SaveUIManager : MonoBehaviour
{
    public GameObject savePanel;       // Your UI popup
    public TMP_InputField saveNameInput;
    public SaveManager saveManager;    // Reference to the Firebase save system

    public void OpenSavePanel()
    {
        if (!GameControls.CanSave)
        {
            Debug.LogWarning("Save is only available in Edit Mode.");
            return;
        }

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
        if (string.IsNullOrEmpty(saveName)) return;

        string jsonData = saveManager.SerializeScene();
        await saveManager.SaveLab(saveName, jsonData);

        CloseSavePanel();
    }
}
