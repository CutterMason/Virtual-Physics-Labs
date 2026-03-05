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
            Debug.LogError("Cannot load, no user logged in.");
            return null;
        }

        string uid = auth.CurrentUser.UserId;

        QuerySnapshot snapshot = await db
            .Collection("users")
            .Document(uid)
            .Collection("labSaves")
            .GetSnapshotAsync();

        List<LabSave> list = new List<LabSave>();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            list.Add(doc.ConvertTo<LabSave>());
        }

        Debug.Log("Loaded " + list.Count + " saves.");
        return list;
    }

    public async void TestLoad()
    {
        var saves = await LoadAllSaves();
        if (saves == null || saves.Count == 0)
        {
            Debug.Log("No saves found.");
            return;
        }
        StartLoadFromJson(saves[0].jsonData);
    }

    public void StartLoadFromJson(string jsonData)
    {
        pendingSave = JsonUtility.FromJson<SceneSaveData>(jsonData);

        if (pendingSave == null || string.IsNullOrEmpty(pendingSave.sceneName))
        {
            Debug.LogError("[LoadManager] Invalid save JSON or missing sceneName.");
            return;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        SceneManager.LoadScene(pendingSave.sceneName);
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
                    if (PrefabRegistry.Instance == null)
                    {
                        Debug.LogError("[LoadManager] PrefabRegistry.Instance is null (make sure it exists and persists).");
                        continue;
                    }

                    GameObject prefab = PrefabRegistry.Instance.GetPrefab(objData.prefabName);
                    if (prefab == null)
                    {
                        Debug.LogError("[LoadManager] Could not spawn object, missing prefab: " + objData.prefabName);
                        continue;
                    }

                    GameObject spawned = Instantiate(prefab);
                    so = spawned.GetComponent<SavableObject>();

                    if (so == null)
                    {
                        Debug.LogError("[LoadManager] Spawned prefab has no SavableObject: " + objData.prefabName);
                        Destroy(spawned);
                        continue;
                    }

                    so.uniqueId = objData.id;
                    Debug.Log($"[LoadManager] Spawned new object {objData.prefabName} with ID {objData.id}");
                }
            }

            Transform t = so.transform;
            t.position = new Vector3(objData.px, objData.py, objData.pz);
            t.eulerAngles = new Vector3(objData.rx, objData.ry, objData.rz);
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
}