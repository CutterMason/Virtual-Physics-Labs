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
        PauseGame(); // keep if you want scenes to start paused
    }

    public void PauseGame()
    {
        // ALWAYS force paused state (no early return)
        IsPaused = true;

        Time.timeScale = 0f;
        Physics.autoSimulation = false;

        allBodies = FindObjectsOfType<Rigidbody>(includeInactive: true);

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
        // ALWAYS force unpaused state (no early return)
        IsPaused = false;

        // Restore if we captured anything; if not, still unpause time/physics
        if (allBodies != null)
        {
            foreach (var rb in allBodies)
            {
                if (rb == null) continue;

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
        Physics.autoSimulation = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}