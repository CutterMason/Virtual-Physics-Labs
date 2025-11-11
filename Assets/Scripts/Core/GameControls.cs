using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControls : MonoBehaviour
{
    private bool isPaused = false;
    private Rigidbody[] allBodies;
    private Dictionary<Rigidbody, Vector3> storedVelocities = new();
    private Dictionary<Rigidbody, Vector3> storedAngularVelocities = new();

    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;

        Physics.autoSimulation = false;

        allBodies = FindObjectsOfType<Rigidbody>();

        storedVelocities.Clear();
        storedAngularVelocities.Clear();

        foreach (var rb in allBodies)
        {
            storedVelocities[rb] = rb.linearVelocity;
            storedAngularVelocities[rb] = rb.angularVelocity;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;

        foreach (var rb in allBodies)
        {
            if (rb != null)
            {
                if (storedVelocities.ContainsKey(rb))
                    rb.linearVelocity = storedVelocities[rb];

                if (storedAngularVelocities.ContainsKey(rb))
                    rb.angularVelocity = storedAngularVelocities[rb];
            }
        }

        Physics.autoSimulation = true;
    }

    public void RestartScene()
    {
        Physics.autoSimulation = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}