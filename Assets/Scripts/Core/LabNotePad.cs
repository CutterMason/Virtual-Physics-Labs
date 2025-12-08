using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Firebase
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

public class LabNotepad : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_InputField notepadInput;

    [Header("Lab Identity")]
    [Tooltip("If empty, the scene name will be used as the lab key.")]
    public string labIdOverride;

    // --- Local in-memory persistence across restarts (already working idea) ---
    private static Dictionary<string, string> notesByLab = new Dictionary<string, string>();

    // --- Firebase ---
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private bool firebaseReady = false;

    private string LabKey
    {
        get
        {
            if (!string.IsNullOrEmpty(labIdOverride))
                return labIdOverride;

            return SceneManager.GetActiveScene().name;
        }
    }

    private void Awake()
    {
        if (notepadInput == null)
        {
            notepadInput = GetComponent<TMP_InputField>();
        }
    }

    private async void Start()
    {
        // 1) Initialize Firebase (if not already initialized elsewhere)
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status == DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;
            firebaseReady = true;
        }
        else
        {
            Debug.LogError($"Firebase dependency error in LabNotepad: {status}");
            firebaseReady = false;
        }

        // 2) Restore from local memory (restart case)
        if (notesByLab.TryGetValue(LabKey, out string localText))
        {
            notepadInput.text = localText;
        }

        // 3) Then try to pull from Firebase, which will override local if present
        if (firebaseReady && auth.CurrentUser != null)
        {
            await LoadFromFirebase();
        }
    }

    private void OnEnable()
    {
        if (notepadInput != null)
        {
            notepadInput.onValueChanged.AddListener(OnNotepadChanged);
        }
    }

    private void OnDisable()
    {
        if (notepadInput != null)
        {
            notepadInput.onValueChanged.RemoveListener(OnNotepadChanged);
        }
    }

    private void OnNotepadChanged(string newText)
    {
        notesByLab[LabKey] = newText;
    }

    // ---------------- Firebase Public API ----------------

    // Call this from a Save button or from your "Save Lab" code
    public void SaveButtonClicked()
    {
        if (!firebaseReady)
        {
            Debug.LogWarning("LabNotepad: Firebase not ready, cannot save notes.");
            return;
        }

        // fire-and-forget async
        _ = SaveToFirebase();
    }

    // Call this from a Load button (optional) or your "Load Lab" code
    public void LoadButtonClicked()
    {
        if (!firebaseReady)
        {
            Debug.LogWarning("LabNotepad: Firebase not ready, cannot load notes.");
            return;
        }

        _ = LoadFromFirebase();
    }

    // Actually save to Firestore
    private async Task SaveToFirebase()
    {
        if (auth == null || db == null)
        {
            Debug.LogWarning("LabNotepad: auth/db null in SaveToFirebase.");
            return;
        }

        var user = auth.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("LabNotepad: No logged in user, cannot save notes.");
            return;
        }

        string uid = user.UserId;
        string labKey = LabKey;

        string docId = $"{uid}_{labKey}";
        DocumentReference docRef = db.Collection("labNotes").Document(docId);

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "uid",          uid },
            { "labId",        labKey },
            { "notepadText",  notepadInput.text },
            { "updatedAt",    Timestamp.GetCurrentTimestamp() }
        };

        try
        {
            await docRef.SetAsync(data, SetOptions.MergeAll);
            Debug.Log($"LabNotepad: Notes saved for user {uid}, lab {labKey}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LabNotepad: Error saving notes to Firebase: {e}");
        }
    }

    // Load from Firestore (used in Start + Load button)
    private async Task LoadFromFirebase()
    {
        if (auth == null || db == null)
        {
            Debug.LogWarning("LabNotepad: auth/db null in LoadFromFirebase.");
            return;
        }

        var user = auth.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("LabNotepad: No logged in user, cannot load notes.");
            return;
        }

        string uid = user.UserId;
        string labKey = LabKey;

        string docId = $"{uid}_{labKey}";
        DocumentReference docRef = db.Collection("labNotes").Document(docId);

        try
        {
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists && snapshot.ContainsField("notepadText"))
            {
                string cloudText = snapshot.GetValue<string>("notepadText");

                // Update UI + local cache
                notepadInput.text = cloudText;
                notesByLab[labKey] = cloudText;

                Debug.Log($"LabNotepad: Loaded notes from Firebase for {uid}, lab {labKey}");
            }
            else
            {
                Debug.Log($"LabNotepad: No existing notes found in Firebase for {uid}, lab {labKey}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LabNotepad: Error loading notes from Firebase: {e}");
        }
    }
}
