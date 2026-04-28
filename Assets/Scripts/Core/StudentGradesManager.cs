using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using TMPro;
using UnityEngine;

public class StudentGradesManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform gradeContentParent;
    public GameObject gradeRowTemplate;

    private FirebaseFirestore db;
    private FirebaseAuth auth;

    private const int TotalLabs = 12;

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

        if (gradeRowTemplate != null)
        {
            gradeRowTemplate.SetActive(false);
        }

        string studentId = GetCurrentStudentId();

        if (string.IsNullOrEmpty(studentId))
        {
            Debug.LogError("No student ID found. Make sure the user is logged in and SessionManager.UserId is set.");
            return;
        }

        LoadStudentGrades(studentId);
    }

    private string GetCurrentStudentId()
    {
        if (SessionManager.Instance != null && !string.IsNullOrEmpty(SessionManager.Instance.UserId))
        {
            return SessionManager.Instance.UserId;
        }

        if (auth != null && auth.CurrentUser != null)
        {
            return auth.CurrentUser.UserId;
        }

        return "";
    }

    private void LoadStudentGrades(string studentId)
    {
        db.Collection("grades").Document(studentId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to load student grades: " + task.Exception);
                return;
            }

            DocumentSnapshot snapshot = task.Result;

            if (!snapshot.Exists)
            {
                Debug.Log("No grades document found for this student. Creating empty grades.");
                CreateEmptyGrades(studentId);
                DisplayEmptyGrades();
                return;
            }

            DisplayGrades(snapshot);
        });
    }

    private void CreateEmptyGrades(string studentId)
    {
        Dictionary<string, object> emptyGrades = new Dictionary<string, object>();

        for (int i = 1; i <= TotalLabs; i++)
        {
            emptyGrades["lab" + i] = "";
        }

        db.Collection("grades").Document(studentId)
            .SetAsync(emptyGrades)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Failed to create empty grades: " + task.Exception);
                }
                else
                {
                    Debug.Log("Empty grades created for student.");
                }
            });
    }

    private void DisplayEmptyGrades()
    {
        ClearOldRows();

        for (int i = 1; i <= TotalLabs; i++)
        {
            CreateGradeRow(i, "");
        }
    }

    private void DisplayGrades(DocumentSnapshot snapshot)
    {
        ClearOldRows();

        for (int i = 1; i <= TotalLabs; i++)
        {
            string labKey = "lab" + i;
            string gradeValue = "";

            if (snapshot.ContainsField(labKey))
            {
                object value = snapshot.GetValue<object>(labKey);
                gradeValue = value?.ToString();
            }

            CreateGradeRow(i, gradeValue);
        }
    }



    private void CreateGradeRow(int labNumber, string gradeValue)
    {
        string[] names = {"Measurement & Error",
        "Newton's 1st Law", "Newton's 2nd Law", "Constant Acceleration", "Projectile Motion","Friction",
        "Energy", "Terminal Velocity", "Momentum", "Torque", "Pendulum Motion", "Mass on a Spring"};
        GameObject newRow = Instantiate(gradeRowTemplate, gradeContentParent);
        newRow.SetActive(true);

        TMP_Text labNameText = FindTMPTextByName(newRow.transform, "LabName");
        TMP_Text gradeText = FindTMPTextByName(newRow.transform, "GradeText");

        if (gradeText == null)
        {
            gradeText = FindTMPTextByName(newRow.transform, "GradeTxt");
        }

        TMP_InputField gradeInput = FindTMPInputByName(newRow.transform, "GradeInput");

        if (labNameText != null)
        {
            labNameText.text = "Lab " + labNumber + ": " + names[labNumber-1];
        }

        string displayValue = string.IsNullOrEmpty(gradeValue) ? "" : gradeValue;

        if (gradeText != null)
        {
            gradeText.text = displayValue;
        }

        if (gradeInput != null)
        {
            gradeInput.text = displayValue;
            gradeInput.interactable = false;
        }
    }

    private void ClearOldRows()
    {
        foreach (Transform child in gradeContentParent)
        {
            if (child.gameObject != gradeRowTemplate)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private TMP_Text FindTMPTextByName(Transform parent, string childName)
    {
        TMP_Text[] texts = parent.GetComponentsInChildren<TMP_Text>(true);

        foreach (TMP_Text text in texts)
        {
            if (text.gameObject.name == childName)
            {
                return text;
            }
        }

        return null;
    }

    private TMP_InputField FindTMPInputByName(Transform parent, string childName)
    {
        TMP_InputField[] inputs = parent.GetComponentsInChildren<TMP_InputField>(true);

        foreach (TMP_InputField input in inputs)
        {
            if (input.gameObject.name == childName)
            {
                return input;
            }
        }

        return null;
    }
}