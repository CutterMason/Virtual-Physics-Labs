using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControls : MonoBehaviour
{
    public static bool IsPaused { get; private set; } = false;

    private Rigidbody[] allBodies;
    private Dictionary<Rigidbody, Vector3> storedVelocities = new();
    private Dictionary<Rigidbody, Vector3> storedAngularVelocities = new();

    private void Start()
    {
        PauseGame(); // Scene always starts paused
    }

    public void PauseGame()
    {
        if (IsPaused) return;
        IsPaused = true;

        // Freeze "time" for anything using deltaTime / FixedUpdate timing
        Time.timeScale = 0f;

        // If you want, keep this too (not strictly required if timeScale=0)
        Physics.autoSimulation = false;

        allBodies = FindObjectsOfType<Rigidbody>();

        storedVelocities.Clear();
        storedAngularVelocities.Clear();

        foreach (var rb in allBodies)
        {
            if (rb == null) continue;

            storedVelocities[rb] = rb.linearVelocity;
            storedAngularVelocities[rb] = rb.angularVelocity;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void ResumeGame()
    {
        if (!IsPaused) return;
        IsPaused = false;

        foreach (var rb in allBodies)
        {
            if (rb == null) continue;

            if (storedVelocities.TryGetValue(rb, out var v))
                rb.linearVelocity = v;

            if (storedAngularVelocities.TryGetValue(rb, out var av))
                rb.angularVelocity = av;
        }

        Physics.autoSimulation = true;
        Time.timeScale = 1f;
    }

    public void RestartScene()
    {
        Physics.autoSimulation = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}