using Firebase.Firestore;

[FirestoreData]
public class LabSave
{
    [FirestoreProperty] public string saveName { get; set; }
    [FirestoreProperty] public Timestamp timestamp { get; set; }
    [FirestoreProperty] public string jsonData { get; set; }
}