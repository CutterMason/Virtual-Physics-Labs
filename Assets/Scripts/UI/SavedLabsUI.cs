using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class SavedLabsUI : MonoBehaviour
{
    public LoadManager loadManager;
    public Transform buttonGroup;
    public GameObject buttonTemplate;

    private List<GameObject> spawnedButtons = new List<GameObject>();

    private void Start()
    {
        RefreshList();
    }

    public async void RefreshList()
    {
        if (loadManager == null)
            loadManager = LoadManager.Instance;

        if (loadManager == null)
        {
            Debug.LogError("[SavedLabsUI] No LoadManager found.");
            return;
        }

        var saves = await loadManager.LoadAllSaves();
        if (saves == null) return;

        foreach (var b in spawnedButtons)
            Destroy(b);

        spawnedButtons.Clear();

        buttonTemplate.SetActive(false);

        foreach (var save in saves)
        {
            GameObject btn = Instantiate(buttonTemplate, buttonGroup);
            btn.SetActive(true);

            TMP_Text text = btn.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
                text.text = save.saveName;

            Button button = btn.GetComponent<Button>();

            if (button != null)
            {
                LabSave capturedSave = save;

                button.onClick.AddListener(() =>
                {
                    Debug.Log("[SavedLabsUI] Loading save: " + capturedSave.saveName);

                    if (LoadManager.Instance != null)
                    {
                        LoadManager.Instance.StartLoadFromSave(capturedSave);
                    }
                    else
                    {
                        Debug.LogError("[SavedLabsUI] LoadManager.Instance is null.");
                    }
                });
            }

            spawnedButtons.Add(btn);
        }
    }
}