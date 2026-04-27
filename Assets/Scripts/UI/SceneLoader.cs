using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        ClearLoadedSaveIfNeeded();

        Time.timeScale = 1f;
        Physics.autoSimulation = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        ClearLoadedSaveIfNeeded();

        Time.timeScale = 1f;
        Physics.autoSimulation = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(sceneIndex);
    }

    public void ReloadCurrentScene()
    {
        ClearLoadedSaveIfNeeded();

        Time.timeScale = 1f;
        Physics.autoSimulation = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ClearLoadedSaveIfNeeded()
    {
        if (LoadManager.Instance != null)
        {
            LoadManager.Instance.ClearLoadedSaveState();
        }
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Physics.autoSimulation = true;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}