using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Auth;

public class LoadManager : MonoBehaviour
{
    private FirebaseFirestore db;
    private FirebaseAuth auth;

    void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
    }

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

        if (saves == null) return;

        foreach (var save in saves)
        {
            Debug.Log($"Save: {save.saveName}, Timestamp:{save.timestamp}, JSON: {save.jsonData}");
        }
    }
}

[FirestoreData]
public class LabSave
{
    [FirestoreProperty] public string saveName { get; set; }
    [FirestoreProperty] public Timestamp timestamp { get; set; }
    [FirestoreProperty] public string jsonData { get; set; }
}
