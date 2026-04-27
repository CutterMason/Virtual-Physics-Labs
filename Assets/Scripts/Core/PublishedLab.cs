using Firebase.Firestore;

[FirestoreData]
public class PublishedLab
{
    [FirestoreProperty]
    public string saveName { get; set; }

    [FirestoreProperty]
    public string sceneName { get; set; }

    [FirestoreProperty]
    public string jsonData { get; set; }

    [FirestoreProperty]
    public Timestamp timestamp { get; set; }

    [FirestoreProperty]
    public string publishedByUid { get; set; }

    [FirestoreProperty]
    public string publishedByEmail { get; set; }

    [FirestoreProperty]
    public bool isPublished { get; set; }
}
