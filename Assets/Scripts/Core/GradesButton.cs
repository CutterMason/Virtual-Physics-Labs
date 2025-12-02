using UnityEngine;
using UnityEngine.SceneManagement;

public class GradesButton : MonoBehaviour
{
    public string teacherGradesScene = "TeacherGradesUI";
    public string studentGradesScene = "StudentsUI";

    public void OnGradesPressed()
    {
        string role = "Student";

        if (SessionManager.Instance != null)
            role = SessionManager.Instance.UserRole;

        if (role == "Teacher")
        {
            SceneManager.LoadScene(teacherGradesScene);
        }
        else
        {
            SceneManager.LoadScene(studentGradesScene);
        }
    }
}