using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PublishedLabManager : MonoBehaviour
{
    public static PublishedLabManager Instance;

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    [Header("References")]
    public SaveManager saveManager;

    [Header("Optional")]
    public bool requireInstructorRole = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        if (saveManager == null)
            saveManager = FindObjectOfType<SaveManager>();
    }

    private void EnsureFirebase()
    {
        if (auth == null) auth = FirebaseAuth.DefaultInstance;
        if (db == null) db = FirebaseFirestore.DefaultInstance;
    }

    public async Task PublishCurrentLab(string publishName)
    {
        EnsureFirebase();

        if (auth.CurrentUser == null)
        {
            Debug.LogError("[PublishedLabManager] Cannot publish. No user logged in.");
            return;
        }

        if (string.IsNullOrWhiteSpace(publishName))
        {
            Debug.LogError("[PublishedLabManager] Cannot publish. Publish name is empty.");
            return;
        }

        if (saveManager == null)
        {
            saveManager = FindObjectOfType<SaveManager>();

            if (saveManager == null)
            {
                Debug.LogError("[PublishedLabManager] Cannot publish. No SaveManager found.");
                return;
            }
        }

        if (requireInstructorRole)
        {
            bool isInstructor = await CurrentUserIsInstructor();

            if (!isInstructor)
            {
                Debug.LogError("[PublishedLabManager] Current user is not an instructor. Publish denied.");
                return;
            }
        }

        string jsonData = saveManager.SerializeScene();
        string cleanName = publishName.Trim();

        DocumentReference publishRef = db
            .Collection("publishedLabs")
            .Document(cleanName);

        PublishedLab publishedLab = new PublishedLab
        {
            saveName = cleanName,
            sceneName = SceneManager.GetActiveScene().name,
            jsonData = jsonData,
            timestamp = Timestamp.GetCurrentTimestamp(),
            publishedByUid = auth.CurrentUser.UserId,
            publishedByEmail = auth.CurrentUser.Email,
            isPublished = true
        };

        await publishRef.SetAsync(publishedLab);

        Debug.Log("[PublishedLabManager] Published global lab: " + cleanName);
    }

    public async Task<List<PublishedLab>> LoadAllPublishedLabs()
    {
        EnsureFirebase();

        QuerySnapshot snapshot = await db
         .Collection("publishedLabs")
         .WhereEqualTo("isPublished", true)
         .GetSnapshotAsync();

        List<PublishedLab> publishedLabs = new List<PublishedLab>();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            PublishedLab lab = doc.ConvertTo<PublishedLab>();
            publishedLabs.Add(lab);
        }

        Debug.Log("[PublishedLabManager] Loaded " + publishedLabs.Count + " published labs.");

        return publishedLabs;
    }

    public void StartLoadPublishedLab(PublishedLab lab)
    {
        if (lab == null)
        {
            Debug.LogError("[PublishedLabManager] Cannot load. PublishedLab is null.");
            return;
        }

        if (string.IsNullOrEmpty(lab.jsonData))
        {
            Debug.LogError("[PublishedLabManager] Cannot load. PublishedLab has no jsonData.");
            return;
        }

        if (LoadManager.Instance == null)
        {
            Debug.LogError("[PublishedLabManager] Cannot load. LoadManager.Instance is null.");
            return;
        }

        LoadManager.Instance.StartLoadFromJson(lab.jsonData);
    }

    private async Task<bool> CurrentUserIsInstructor()
    {
        EnsureFirebase();

        if (auth.CurrentUser == null)
            return false;

        string uid = auth.CurrentUser.UserId;

        DocumentReference userRef = db
            .Collection("users")
            .Document(uid);

        DocumentSnapshot snapshot = await userRef.GetSnapshotAsync();

        if (!snapshot.Exists)
        {
            Debug.LogWarning("[PublishedLabManager] User document does not exist.");
            return false;
        }

        Dictionary<string, object> data = snapshot.ToDictionary();

        if (data.ContainsKey("role"))
        {
            string role = data["role"].ToString().ToLower();
            return role == "instructor" || role == "teacher";
        }

        Debug.LogWarning("[PublishedLabManager] User document has no role field.");
        return false;
    }
}
