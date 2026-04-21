using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHistoryManager : MonoBehaviour
{
    private const string LastSceneKey = "LastScene";

    /// <summary>
    /// Call this when loading a new scene.
    /// It saves the current scene before switching.
    /// </summary>
    public static void LoadScene(string sceneName)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Save current scene
        PlayerPrefs.SetString(LastSceneKey, currentScene);
        PlayerPrefs.Save();

        // Load the new scene
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Call this for your back button.
    /// </summary>
    public static void LoadPreviousScene()
    {
        if (PlayerPrefs.HasKey(LastSceneKey))
        {
            string lastScene = PlayerPrefs.GetString(LastSceneKey);
            SceneManager.LoadScene(lastScene);
        }
        else
        {
            Debug.LogWarning("No previous scene saved!");
        }
    }

    /// <summary>
    /// Optional: Clear saved history
    /// </summary>
    public static void ClearHistory()
    {
        PlayerPrefs.DeleteKey(LastSceneKey);
    }
}