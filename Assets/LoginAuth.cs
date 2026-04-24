using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginAuth : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    [Header("Role (Radio Toggles)")]
    public Toggle studentToggle;
    public Toggle teacherToggle;
    public ToggleGroup roleToggleGroup;  
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

            if (!studentToggle.isOn && !teacherToggle.isOn)
            {
                studentToggle.isOn = true;
            }
        }
        else
        {
            messageText.text = "Firebase initialization error: " + status.ToString();
            Debug.LogError($"Firebase dependency error: {status}");
        }
    }

    bool IsValidEmailForRole(string email, string role)
    {
        if (string.IsNullOrEmpty(role))
            return false;

        email = email.ToLowerInvariant();

        if (role == "Student" && email.EndsWith("@buffs.wtamu.edu"))
            return true;

        if (role == "Teacher" && email.EndsWith("@wtamu.edu"))
            return true;

        return false;
    }

    public void OnCreateAccountButton()
    {
        string email = emailInput.text.Trim().ToLowerInvariant();
        string password = passwordInput.text;
        string role = GetSelectedRole();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            messageText.text = "Please fill in all fields.";
            return;
        }

        if (string.IsNullOrEmpty(role))
        {
            messageText.text = "Please select Student or Teacher.";
            return;
        }

        if (!IsValidEmailForRole(email, role))
        {
            messageText.text = $"You must use a valid {role} WTAMU email.";
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
        string email = emailInput.text.Trim().ToLowerInvariant();
        string password = passwordInput.text;
        string role = GetSelectedRole();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            messageText.text = "Please fill in all fields.";
            return;
        }

        if (string.IsNullOrEmpty(role))
        {
            messageText.text = "Please select Student or Teacher.";
            return;
        }

        if (!IsValidEmailForRole(email, role))
        {
            messageText.text = $"Invalid email for {role}.";
            return;
        }

        messageText.text = "Checking credentials...";

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                messageText.text = "Login failed: Invalid email or password.";
                Debug.LogError(task.Exception);
                return;
            }

            FirebaseUser user = task.Result.User;
            messageText.text = "Login successful! Loading profile...";

            db.Collection("users").Document(user.UserId).GetSnapshotAsync().ContinueWithOnMainThread(docTask =>
            {
                if (docTask.IsFaulted || docTask.IsCanceled)
                {
                    messageText.text = "Error fetching user role.";
                    Debug.LogError(docTask.Exception);
                    return;
                }

                if (docTask.Result.Exists)
                {
                    string storedRole = docTask.Result.GetValue<string>("role");
                    messageText.text = $"Welcome, {storedRole}!";

                    if (SessionManager.Instance != null)
                    {
                        SessionManager.Instance.UserId = user.UserId;
                        SessionManager.Instance.UserEmail = user.Email;
                        SessionManager.Instance.UserRole = storedRole;
                    }

                    SceneManager.LoadScene("MainMenuUI");
                }
                else
                {
                    messageText.text = "User profile missing in database.";
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
        return null;
    }
}