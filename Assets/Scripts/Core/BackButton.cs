using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    // Set this in the Inspector
    public string sceneName = "MainMenuUI";

    public void OnBackPressed()
    {
        Debug.Log("[BackButton] Going to scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}
