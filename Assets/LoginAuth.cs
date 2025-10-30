using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;  
using UnityEngine.UI;

public class LoginAuth : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Toggle studentToggle;
    public Toggle teacherToggle;
    public TMP_Text messageText;

    FirebaseAuth auth;
    FirebaseFirestore db;

    async void Start()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status == DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;
            messageText.text = "Firebase ready!";
            Debug.Log("Firebase initialized.");
        }
        else
        {
            messageText.text = "Firebase initialization error: " + status.ToString();
            Debug.LogError($"Firebase dependency error: {status}");
        }
    }

    public void OnRegisterButton()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string role = GetSelectedRole();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            messageText.text = "Please fill in all fields.";
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                messageText.text = "Signup failed: " + task.Exception?.Message;
                Debug.LogError(task.Exception);
                return;
            }

            FirebaseUser user = task.Result.User;
            Debug.Log($"Account created: {user.Email} ({role})");

            var data = new System.Collections.Generic.Dictionary<string, object>
            {
                { "email", email },
                { "role", role },
                { "createdAt", Timestamp.GetCurrentTimestamp() }
            };

            db.Collection("users").Document(user.UserId).SetAsync(data).ContinueWithOnMainThread(writeTask =>
            {
                if (writeTask.IsFaulted || writeTask.IsCanceled)
                {
                    messageText.text = "User created, but Firestore write failed.";
                    Debug.LogError(writeTask.Exception);
                }
                else
                {
                    messageText.text = $"Account created as {role}";
                }
            });
        });
    }

   
    public void OnLoginButton()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            messageText.text = "Please fill in all fields.";
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                messageText.text = "Login failed: " + task.Exception?.Message;
                Debug.LogError(task.Exception);
                return;
            }

            FirebaseUser user = task.Result.User;
            messageText.text = "Logged in as " + user.Email;

  
            db.Collection("users").Document(user.UserId).GetSnapshotAsync().ContinueWithOnMainThread(docTask =>
            {
                if (docTask.IsCompleted && docTask.Result.Exists)
                {
                    string role = docTask.Result.GetValue<string>("role");
                    Debug.Log($"Role: {role}");
                    messageText.text = $"Welcome, {role}!";
                  
                }
            });
        });
    }

    
    private string GetSelectedRole()
    {
        if (teacherToggle != null && teacherToggle.isOn)
            return "Teacher";
        if (studentToggle != null && studentToggle.isOn)
            return "Student";
        return "Student";
    }
}