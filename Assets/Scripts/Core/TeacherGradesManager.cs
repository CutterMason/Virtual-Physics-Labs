using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeacherGradesManager : MonoBehaviour
{
    [Header("Main Panels")]
    public GameObject gradesPanel;
    public GameObject studentSelectPanel;

    [Header("Grade UI")]
    public Transform gradeContentParent;
    public GameObject gradeRowTemplate;
    public Button saveGradesButton;
    public TMP_Text selectedStudentLabel;

    [Header("Student Select UI")]
    public Transform studentListContentParent;
    public GameObject studentButtonTemplate;

    private FirebaseFirestore db;

    private const int TotalLabs = 12;

    private string selectedStudentId = "";
    private string selectedStudentEmail = "";

    private readonly Dictionary<int, TMP_InputField> gradeInputs = new Dictionary<int, TMP_InputField>();

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        if (gradeRowTemplate != null)
        {
            gradeRowTemplate.SetActive(false);
        }

        if (studentButtonTemplate != null)
        {
            studentButtonTemplate.SetActive(false);
        }

        if (gradesPanel != null)
        {
            gradesPanel.SetActive(true);
        }

        if (studentSelectPanel != null)
        {
            studentSelectPanel.SetActive(false);
        }

        if (saveGradesButton != null)
        {
            saveGradesButton.onClick.RemoveAllListeners();
            saveGradesButton.onClick.AddListener(SaveGrades);
            saveGradesButton.interactable = false;
        }

        if (selectedStudentLabel != null)
        {
            selectedStudentLabel.text = "No student selected";
        }

        CreateEmptyGradeRows();
    }

    public void OpenStudentSelectPanel()
    {
        if (gradesPanel != null)
        {
            gradesPanel.SetActive(false);
        }

        if (studentSelectPanel != null)
        {
            studentSelectPanel.SetActive(true);
            studentSelectPanel.transform.SetAsLastSibling();
        }

        LoadStudentList();
    }

    public void CloseStudentSelectPanel()
    {
        if (studentSelectPanel != null)
        {
            studentSelectPanel.SetActive(false);
        }

        if (gradesPanel != null)
        {
            gradesPanel.SetActive(true);
        }
    }

    private void LoadStudentList()
    {
        ClearStudentButtons();

        db.Collection("users")
            .WhereEqualTo("role", "Student")
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Failed to load students: " + task.Exception);
                    return;
                }

                QuerySnapshot snapshot = task.Result;

                if (snapshot.Count == 0)
                {
                    Debug.LogWarning("No student users found.");
                    return;
                }

                foreach (DocumentSnapshot studentDoc in snapshot.Documents)
                {
                    string studentId = studentDoc.Id;
                    string email = "Unknown Student";

                    if (studentDoc.ContainsField("email"))
                    {
                        email = studentDoc.GetValue<string>("email");
                    }

                    CreateStudentButton(studentId, email);
                }
            });
    }

    private void CreateStudentButton(string studentId, string email)
    {
        GameObject newButtonObj = Instantiate(studentButtonTemplate, studentListContentParent);
        newButtonObj.SetActive(true);

        TMP_Text buttonText = FindChildTMPTextByName(newButtonObj.transform, "StudentName");
        if (buttonText == null)
        {
            buttonText = newButtonObj.GetComponentInChildren<TMP_Text>(true);
        }

        if (buttonText != null)
        {
            buttonText.text = email;
        }

        Button button = newButtonObj.GetComponentInChildren<Button>(true);
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                SelectStudent(studentId, email);
            });
        }
    }

    private void SelectStudent(string studentId, string email)
    {
        selectedStudentId = studentId;
        selectedStudentEmail = email;

        if (selectedStudentLabel != null)
        {
            selectedStudentLabel.text = "Selected: " + selectedStudentEmail;
        }

        if (saveGradesButton != null)
        {
            saveGradesButton.interactable = true;
        }

        if (studentSelectPanel != null)
        {
            studentSelectPanel.SetActive(false);
        }

        if (gradesPanel != null)
        {
            gradesPanel.SetActive(true);
        }

        LoadGradesForSelectedStudent();
    }

    private void LoadGradesForSelectedStudent()
    {
        if (string.IsNullOrEmpty(selectedStudentId))
        {
            Debug.LogWarning("No student selected.");
            return;
        }

        DocumentReference gradesDocRef = db.Collection("grades").Document(selectedStudentId);

        gradesDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to load grades: " + task.Exception);
                return;
            }

            DocumentSnapshot snapshot = task.Result;

            if (!snapshot.Exists)
            {
                Debug.Log("No grade document found. Creating empty grades.");
                CreateEmptyGradesForSelectedStudent();
                ClearGradeInputs();
                return;
            }

            FillGradeInputs(snapshot);
        });
    }

    private void CreateEmptyGradeRows()
    {
        string[] names = {"Measurement & Error",
        "Newton's 1st Law", "Newton's 2nd Law", "Constant Acceleration", "Projectile Motion","Friction",
        "Energy", "Terminal Velocity", "Momentum", "Torque", "Pendulum Motion", "Mass on a Spring"};
        ClearGradeRows();
        gradeInputs.Clear();

        for (int i = 1; i <= TotalLabs; i++)
        {
            GameObject newRow = Instantiate(gradeRowTemplate, gradeContentParent);
            newRow.SetActive(true);

            TMP_Text labNameText = FindChildTMPTextByName(newRow.transform, "LabName");
            TMP_InputField gradeInput = FindChildTMPInputByName(newRow.transform, "GradeInput");

            if (labNameText != null)
            {
                labNameText.text = "Lab " + i + ": " + names[i-1];
            }

            if (gradeInput != null)
            {
                gradeInput.text = "";
                gradeInput.interactable = false;
                gradeInputs[i] = gradeInput;
            }
        }
    }

    private void FillGradeInputs(DocumentSnapshot snapshot)
    {
        for (int i = 1; i <= TotalLabs; i++)
        {
            string labKey = "lab" + i;
            string gradeValue = "";

            if (snapshot.ContainsField(labKey))
            {
                object value = snapshot.GetValue<object>(labKey);
                gradeValue = value?.ToString();
            }

            if (gradeInputs.ContainsKey(i))
            {
                gradeInputs[i].interactable = true;
                gradeInputs[i].text = gradeValue;
            }
        }
    }

    private void ClearGradeInputs()
    {
        for (int i = 1; i <= TotalLabs; i++)
        {
            if (gradeInputs.ContainsKey(i))
            {
                gradeInputs[i].interactable = true;
                gradeInputs[i].text = "";
            }
        }
    }

    private void CreateEmptyGradesForSelectedStudent()
    {
        if (string.IsNullOrEmpty(selectedStudentId))
        {
            return;
        }

        Dictionary<string, object> emptyGrades = new Dictionary<string, object>();

        for (int i = 1; i <= TotalLabs; i++)
        {
            emptyGrades["lab" + i] = "";
        }

        db.Collection("grades").Document(selectedStudentId)
            .SetAsync(emptyGrades)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Failed to create empty grades: " + task.Exception);
                }
                else
                {
                    Debug.Log("Empty grades created for selected student.");
                }
            });
    }

    public void SaveGrades()
    {
        if (string.IsNullOrEmpty(selectedStudentId))
        {
            Debug.LogWarning("Cannot save. No student selected.");
            return;
        }

        Dictionary<string, object> updatedGrades = new Dictionary<string, object>();

        for (int i = 1; i <= TotalLabs; i++)
        {
            string labKey = "lab" + i;
            string gradeValue = "";

            if (gradeInputs.ContainsKey(i) && gradeInputs[i] != null)
            {
                gradeValue = gradeInputs[i].text.Trim();
            }

            updatedGrades[labKey] = gradeValue;
        }

        db.Collection("grades").Document(selectedStudentId)
            .SetAsync(updatedGrades, SetOptions.MergeAll)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Failed to save grades: " + task.Exception);
                }
                else
                {
                    Debug.Log("Grades saved for " + selectedStudentEmail);
                }
            });
    }

    private void ClearGradeRows()
    {
        foreach (Transform child in gradeContentParent)
        {
            if (child.gameObject != gradeRowTemplate)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void ClearStudentButtons()
    {
        foreach (Transform child in studentListContentParent)
        {
            if (child.gameObject != studentButtonTemplate)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private TMP_Text FindChildTMPTextByName(Transform parent, string childName)
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

    private TMP_InputField FindChildTMPInputByName(Transform parent, string childName)
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