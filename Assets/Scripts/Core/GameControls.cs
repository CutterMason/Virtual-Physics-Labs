using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControls : MonoBehaviour
{
    public static bool IsPaused { get; private set; } = false;

    private Rigidbody[] allBodies;
    private readonly Dictionary<Rigidbody, Vector3> storedVelocities = new();
    private readonly Dictionary<Rigidbody, Vector3> storedAngularVelocities = new();

    private bool pauseOnLoad = true; // set true if you want restart to come up paused

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        PauseGame(); // keep if you want scenes to start paused
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // scene just reloaded, optionally start paused again
        if (pauseOnLoad)
            PauseGame();
    }

    public void PauseGame()
    {
        IsPaused = true;

        Time.timeScale = 0f;
        Physics.autoSimulation = false;

        allBodies = FindObjectsOfType<Rigidbody>(includeInactive: true);

        storedVelocities.Clear();
        storedAngularVelocities.Clear();

        foreach (var rb in allBodies)
        {
            if (!rb) continue;

            storedVelocities[rb] = rb.linearVelocity;
            storedAngularVelocities[rb] = rb.angularVelocity;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }
    }

    public void ResumeGame()
    {
        IsPaused = false;

        if (allBodies != null)
        {
            foreach (var rb in allBodies)
            {
                if (!rb) continue;

                if (storedVelocities.TryGetValue(rb, out var v))
                    rb.linearVelocity = v;

                if (storedAngularVelocities.TryGetValue(rb, out var av))
                    rb.angularVelocity = av;
            }
        }

        Physics.autoSimulation = true;
        Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        if (IsPaused) ResumeGame();
        else PauseGame();
    }

    public void RestartScene()
    {
        
        IsPaused = false;
        Time.timeScale = 1f;
        Physics.autoSimulation = true;

        
        storedVelocities.Clear();
        storedAngularVelocities.Clear();
        allBodies = null;

        pauseOnLoad = true; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}