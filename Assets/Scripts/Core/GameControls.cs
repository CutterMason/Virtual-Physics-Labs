using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameControls : MonoBehaviour
{
    public static bool IsPaused { get; private set; } = false;
    public static bool IsEditMode { get; private set; } = false;

    [Header("Startup")]
    [SerializeField] private bool pauseOnLoad = true;
    [SerializeField] private bool startInEditMode = true;
    [SerializeField] private bool pauseWhenEnteringEditMode = true;

    private Rigidbody[] allBodies;
    private readonly Dictionary<Rigidbody, Vector3> storedVelocities = new();
    private readonly Dictionary<Rigidbody, Vector3> storedAngularVelocities = new();

    // ---- Capability gates (use these elsewhere) ----
    public static bool CanSpawn => IsEditMode;
    public static bool CanSave => IsEditMode;
    public static bool CanDelete => IsEditMode;
    public static bool CanMoveObjects => IsEditMode;

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
        // Decide initial mode
        if (startInEditMode) EnterEditMode();
        else ExitEditMode();

        if (pauseOnLoad)
            PauseGame();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-apply mode on load if desired
        if (startInEditMode) EnterEditMode();
        else ExitEditMode();

        if (pauseOnLoad)
            PauseGame();
    }

    // ------------------- EDIT MODE -------------------
    public void EnterEditMode()
    {
        IsEditMode = true;

        // Optional: freeze the sim while editing
        if (pauseWhenEnteringEditMode && !IsPaused)
            PauseGame();

        // Make UI usable
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // TODO (optional): broadcast to other systems
        // OnEditModeChanged?.Invoke(true);
    }

    public void ExitEditMode()
    {
        IsEditMode = false;

        // Leaving edit mode should resume simulation
        if (IsPaused)
            ResumeGame();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void ToggleEditMode()
    {
        if (IsEditMode) ExitEditMode();
        else EnterEditMode();
    }

    // ------------------- PAUSE -------------------
    public void PauseGame()
    {
        IsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

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

                rb.WakeUp(); // <-- ADD THIS
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

        // Keep whatever you prefer on restart:
        pauseOnLoad = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void StartPlayMode()
    {
        // Exit edit mode first (this resumes physics too)
        ExitEditMode();

        // Force physics on for objects that should simulate
        foreach (var rb in FindObjectsOfType<Rigidbody>())
        {
            if (!rb) continue;

            // Only affect the stuff you WANT to fall:
            // Use a Tag or Layer check here.
            // Example: if (!rb.CompareTag("Movable")) continue;

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.WakeUp();
        }
    }

}