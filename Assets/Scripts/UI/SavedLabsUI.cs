using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class SavedLabsUI : MonoBehaviour
{
    public LoadManager loadManager;
    public Transform buttonGroup;      // Parent that contains Lab1, Lab2, ...
    public GameObject buttonTemplate;  // One button used to clone

    private List<GameObject> spawnedButtons = new List<GameObject>();

    private void Start()
    {
        RefreshList();
    }

    public async void RefreshList()
    {
        // Get saves from Firebase
        var saves = await loadManager.LoadAllSaves();
        if (saves == null) return;

        // Hide old buttons
        foreach (var b in spawnedButtons) Destroy(b);
        spawnedButtons.Clear();

        // Create a button for each save
        foreach (var save in saves)
        {
            GameObject btn = Instantiate(buttonTemplate, buttonGroup);
            btn.SetActive(true);

            // Set button label
            btn.GetComponentInChildren<TMP_Text>().text = save.saveName;

            // Add click listener
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("Loading save: " + save.saveName);
                loadManager.StartLoadFromSave(save);
            });

            spawnedButtons.Add(btn);
        }
    }
}
