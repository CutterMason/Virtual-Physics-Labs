using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using TMPro;

public class SaveManager : MonoBehaviour
{
    FirebaseAuth auth;
    FirebaseFirestore db;

    [Header("Experiment UI")]
    public TMP_InputField notepadInput;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
            TestSave();

        if (Input.GetKeyDown(KeyCode.F6))
            FindObjectOfType<LoadManager>().TestLoad();
    }

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    public async Task SaveLab(string saveName, string jsonData)
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("Cannot save — no user logged in.");
            return;
        }

        string uid = auth.CurrentUser.UserId;

        DocumentReference saveRef = db
            .Collection("users")
            .Document(uid)
            .Collection("labSaves")
            .Document(saveName);

        var saveObj = new
        {
            saveName = saveName,
            timestamp = Timestamp.GetCurrentTimestamp(),
            jsonData = jsonData
        };

        await saveRef.SetAsync(saveObj);
        Debug.Log("Lab saved successfully: " + saveName);
    }

   
    public async void TestSave()
    {
        string randomName = "TestSave_" + Random.Range(1000, 9999);
        string jsonData = "{ \"test\": true }";

        await SaveLab(randomName, jsonData);
    }

    public string SerializeScene()
    {
        SceneSaveData save = new SceneSaveData();
        save.sceneName = SceneManager.GetActiveScene().name;

        SavableObject[] savables = Resources.FindObjectsOfTypeAll<SavableObject>();

        Debug.Log($"[SaveManager] Found {savables.Length} SavableObject(s) to save.");

        foreach (var so in savables)
        {
            // NEW — skip preset scene objects
            if (so.isPresetObject)
                continue;

            if (!so.gameObject.scene.IsValid())
                continue;

            Transform t = so.transform;

            ObjectSaveData data = new ObjectSaveData();
            data.id = so.uniqueId;

            string rawName = so.gameObject.name.Replace("(Clone)", "");
            int parenIndex = rawName.IndexOf(" (");
            if (parenIndex >= 0)
            {
                rawName = rawName.Substring(0, parenIndex);
            }
            data.prefabName = rawName.Trim();   // final key used for registry lookup

            data.px = t.position.x;
            data.py = t.position.y;
            data.pz = t.position.z;

            data.rx = t.eulerAngles.x;
            data.ry = t.eulerAngles.y;
            data.rz = t.eulerAngles.z;

            data.active = so.gameObject.activeSelf;

            save.objects.Add(data);
        }

        Debug.Log($"[SaveManager] Saving {save.objects.Count} object(s).");

        save.experimentData = new ExperimentData
        {
            sliderValue = 0f,
            toggleValue = false,
            timerValue = 0f,
            notepadText = notepadInput != null ? notepadInput.text : ""
        };

        string json = JsonUtility.ToJson(save);
        Debug.Log($"[SaveManager] JSON:\n{json}");
        return json;
    }


}