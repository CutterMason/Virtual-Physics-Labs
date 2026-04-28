using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class LoadManager : MonoBehaviour
{
    public static LoadManager Instance;

    private FirebaseFirestore db;
    private FirebaseAuth auth;

    private SceneSaveData pendingSave;

    private string currentLoadedJson;
    private bool wasLoadedFromSave;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
    }

    private void EnsureFirebase()
    {
        if (db == null) db = FirebaseFirestore.DefaultInstance;
        if (auth == null) auth = FirebaseAuth.DefaultInstance;
    }

    public async Task<List<LabSave>> LoadAllSaves()
    {
        EnsureFirebase();

        if (auth == null)
        {
            Debug.LogError("[LoadManager] FirebaseAuth is null.");
            return null;
        }

        if (auth.CurrentUser == null)
        {
            Debug.LogError("[LoadManager] Cannot load, no user logged in.");
            return null;
        }

        string uid = auth.CurrentUser.UserId;

        QuerySnapshot snapshot = await db
            .Collection("users")
            .Document(uid)
            .Collection("labSaves")
            .OrderByDescending("timestamp")
            .GetSnapshotAsync();

        List<LabSave> list = new List<LabSave>();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            list.Add(doc.ConvertTo<LabSave>());
        }

        Debug.Log("[LoadManager] Loaded " + list.Count + " saves.");
        return list;
    }

    public async void TestLoad()
    {
        var saves = await LoadAllSaves();

        if (saves == null || saves.Count == 0)
        {
            Debug.Log("[LoadManager] No saves found.");
            return;
        }

        Debug.Log("[LoadManager] Test loading newest save: " + saves[0].saveName);

        StartLoadFromJson(saves[0].jsonData);
    }

    public void StartLoadFromSave(LabSave save)
    {
        if (save == null)
        {
            Debug.LogError("[LoadManager] StartLoadFromSave called with null save.");
            return;
        }

        StartLoadFromJson(save.jsonData);
    }

    public void StartLoadFromJson(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData))
        {
            Debug.LogError("[LoadManager] Cannot load. jsonData is empty.");
            return;
        }

        currentLoadedJson = jsonData;
        wasLoadedFromSave = true;

        pendingSave = JsonUtility.FromJson<SceneSaveData>(jsonData);

        if (pendingSave == null || string.IsNullOrEmpty(pendingSave.sceneName))
        {
            Debug.LogError("[LoadManager] Invalid save JSON or missing sceneName.");
            return;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        Time.timeScale = 1f;
        Physics.autoSimulation = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(pendingSave.sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (pendingSave == null)
        {
            Debug.LogWarning("[LoadManager] Scene loaded but no pendingSave exists.");
            return;
        }

        RestoreObjects(pendingSave);
        RestoreExperiment(pendingSave);

        // Important:
        // Do NOT force GameControls state here.
        // Let GameControls.Start() initialize the scene normally.
        // The loader should only restore saved lab data.

        Debug.Log("[LoadManager] Scene loaded and state restored via sceneLoaded.");

        pendingSave = null;
    }

    private void RestoreObjects(SceneSaveData save)
    {
        if (save.objects == null)
        {
            Debug.LogWarning("[LoadManager] Save has no objects list.");
            return;
        }

        SavableObject[] existing = FindObjectsOfType<SavableObject>(true);
        var lookup = new Dictionary<string, SavableObject>();

        foreach (var so in existing)
        {
            if (!string.IsNullOrEmpty(so.uniqueId))
                lookup[so.uniqueId] = so;
        }

        Debug.Log($"[LoadManager] Existing SavableObjects in scene: {existing.Length}");
        Debug.Log($"[LoadManager] Save contains {save.objects.Count} object(s).");

        foreach (var objData in save.objects)
        {
            if (objData == null || string.IsNullOrEmpty(objData.id))
                continue;

            SavableObject so = null;
            bool found = lookup.TryGetValue(objData.id, out so);

            if (objData.isPresetObject)
            {
                if (!found || so == null)
                {
                    Debug.LogError($"[LoadManager] Preset object missing in scene! ID={objData.id}, nameKey={objData.prefabName}");
                    continue;
                }
            }
            else
{
    if (!found || so == null)
    {
        PrefabRegistry registry = PrefabRegistry.Instance;

        if (registry == null)
        {
            registry = FindObjectOfType<PrefabRegistry>(true);

            if (registry != null)
            {
                PrefabRegistry.Instance = registry;
                Debug.LogWarning("[LoadManager] PrefabRegistry.Instance was null, but a registry was found in the scene.");
            }
        }

        if (registry == null)
        {
            Debug.LogError("[LoadManager] PrefabRegistry.Instance is null. Make sure a PrefabRegistry object exists in this lab scene.");
            continue;
        }

        GameObject prefab = registry.GetPrefab(objData.prefabName);

        if (prefab == null)
        {
            Debug.LogError("[LoadManager] Could not spawn object, missing prefab: " + objData.prefabName);
            continue;
        }

        GameObject spawned = Instantiate(prefab);
        so = spawned.GetComponent<SavableObject>();

        if (so == null)
        {
            so = spawned.GetComponentInChildren<SavableObject>();
        }

        if (so == null)
        {
            Debug.LogError("[LoadManager] Spawned prefab has no SavableObject: " + objData.prefabName);
            Destroy(spawned);
            continue;
        }

        so.uniqueId = objData.id;
        so.prefab = prefab;
        so.prefabName = prefab.name;
        so.isPresetObject = false;

        Debug.Log($"[LoadManager] Spawned new object {objData.prefabName} with ID {objData.id}");
    }
}

            Transform t = so.transform;

            t.position = new Vector3(objData.px, objData.py, objData.pz);
            t.eulerAngles = new Vector3(objData.rx, objData.ry, objData.rz);

            // This keeps your original save/load behavior.
            so.gameObject.SetActive(objData.active);
        }

        Debug.Log("[LoadManager] RestoreObjects complete.");
    }

    private void RestoreExperiment(SceneSaveData save)
    {
        if (save.experimentData == null)
        {
            Debug.Log("[LoadManager] No experimentData in save; skipping restore.");
            return;
        }

        LabNotepad labNotepad = FindObjectOfType<LabNotepad>(true);

        if (labNotepad != null && labNotepad.notepadInput != null)
        {
            string text = save.experimentData.notepadText ?? "";
            labNotepad.notepadInput.text = text;

            Debug.Log($"[LoadManager] Restored notepad text ({text.Length} chars).");
        }
        else
        {
            Debug.LogWarning("[LoadManager] Could not find LabNotepad to restore notes.");
        }
    }

    public bool HasLoadedSave()
    {
        return wasLoadedFromSave && !string.IsNullOrEmpty(currentLoadedJson);
    }

    public void RestartLoadedSave()
    {
        if (!HasLoadedSave())
        {
            Debug.LogWarning("[LoadManager] No loaded save JSON found. Restarting current scene normally.");

            Time.timeScale = 1f;
            Physics.autoSimulation = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        Debug.Log("[LoadManager] Restarting from saved/published JSON.");

        StartLoadFromJson(currentLoadedJson);
    }

    public void ClearLoadedSaveState()
    {
        currentLoadedJson = null;
        wasLoadedFromSave = false;

        Debug.Log("[LoadManager] Cleared loaded save state.");
    }
}