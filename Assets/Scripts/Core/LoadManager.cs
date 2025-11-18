using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine.SceneManagement;

public class LoadManager : MonoBehaviour
{
    private FirebaseFirestore db;
    private FirebaseAuth auth;

    void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
    }

    // Load all save documents for UI debugging
    public async Task<List<LabSave>> LoadAllSaves()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("Cannot load — no user logged in.");
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

        // Load the first save for testing
        string json = saves[0].jsonData;
        StartCoroutine(LoadSceneFromJson(json));
    }

    // ------------------------------
    // Load Scene State
    // ------------------------------
    public IEnumerator LoadSceneFromJson(string jsonData)
    {
        SceneSaveData save = JsonUtility.FromJson<SceneSaveData>(jsonData);

        // 1. Load saved scene
        AsyncOperation op = SceneManager.LoadSceneAsync(save.sceneName);
        yield return op;

        // 2. Restore objects after scene is loaded
        yield return new WaitForEndOfFrame();

        RestoreObjects(save);
        RestoreExperiment(save);

        Debug.Log("Scene loaded and state restored.");
    }

    private void RestoreObjects(SceneSaveData save)
    {
     
        SavableObject[] existing = FindObjectsOfType<SavableObject>();
        var lookup = new Dictionary<string, SavableObject>();

        foreach (var so in existing)
            lookup[so.uniqueId] = so;

        Debug.Log($"[LoadManager] Existing SavableObjects in scene: {existing.Length}");

        foreach (var objData in save.objects)
        {
            SavableObject so;

         
            if (!lookup.TryGetValue(objData.id, out so))
            {
               
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
                    continue;
                }

                so.uniqueId = objData.id;
                Debug.Log($"[LoadManager] Spawned new object {objData.prefabName} with ID {objData.id}");
            }
            else
            {
                Debug.Log($"[LoadManager] Restoring existing object {objData.prefabName} with ID {objData.id}");
            }

            
            Transform t = so.transform;
            t.position = new Vector3(objData.px, objData.py, objData.pz);
            t.eulerAngles = new Vector3(objData.rx, objData.ry, objData.rz);
            so.gameObject.SetActive(objData.active);
        }

        Debug.Log("[LoadManager] RestoreObjects complete for " + save.objects.Count + " object(s).");
    }

    private void RestoreExperiment(SceneSaveData save)
    {
        // You can populate this later when you have real UI / experiment values.
    }

    public void StartLoadFromSave(LabSave save)
    {
        StartCoroutine(LoadSceneFromJson(save.jsonData));
    }

}
