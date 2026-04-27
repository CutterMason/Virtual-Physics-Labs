using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PublishedLabsUI : MonoBehaviour
{
    public PublishedLabManager publishedLabManager;
    public Transform buttonGroup;
    public GameObject buttonTemplate;

    private List<GameObject> spawnedButtons = new List<GameObject>();

    private void Start()
    {
        RefreshList();
    }

    public async void RefreshList()
    {
        Debug.Log("[PublishedLabsUI] RefreshList started.");

        if (publishedLabManager == null)
        {
            publishedLabManager = PublishedLabManager.Instance;
        }

        if (publishedLabManager == null)
        {
            Debug.LogError("[PublishedLabsUI] No PublishedLabManager found.");
            return;
        }

        if (buttonGroup == null)
        {
            Debug.LogError("[PublishedLabsUI] buttonGroup is not assigned.");
            return;
        }

        if (buttonTemplate == null)
        {
            Debug.LogError("[PublishedLabsUI] buttonTemplate is not assigned.");
            return;
        }

        var publishedLabs = await publishedLabManager.LoadAllPublishedLabs();

        if (publishedLabs == null)
        {
            Debug.LogError("[PublishedLabsUI] publishedLabs came back null.");
            return;
        }

        Debug.Log("[PublishedLabsUI] Received " + publishedLabs.Count + " published lab(s).");

        foreach (var b in spawnedButtons)
        {
            Destroy(b);
        }

        spawnedButtons.Clear();

        buttonTemplate.SetActive(false);

        foreach (var lab in publishedLabs)
        {
            Debug.Log("[PublishedLabsUI] Creating button for: " + lab.saveName);

            GameObject btn = Instantiate(buttonTemplate, buttonGroup);
            btn.SetActive(true);

            TMP_Text buttonText = btn.GetComponentInChildren<TMP_Text>(true);

            if (buttonText != null)
            {
                buttonText.text = lab.saveName;
            }
            else
            {
                Debug.LogWarning("[PublishedLabsUI] Button has no TMP_Text child.");
            }

            Button button = btn.GetComponent<Button>();

            if (button != null)
            {
                PublishedLab capturedLab = lab;

                button.onClick.AddListener(() =>
                {
                    Debug.Log("Loading published lab: " + capturedLab.saveName);
                    publishedLabManager.StartLoadPublishedLab(capturedLab);
                });
            }
            else
            {
                Debug.LogWarning("[PublishedLabsUI] Button template has no Button component.");
            }

            spawnedButtons.Add(btn);
        }
    }
}