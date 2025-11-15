using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;

public class SaveManager : MonoBehaviour
{
    FirebaseAuth auth;
    FirebaseFirestore db;

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
}