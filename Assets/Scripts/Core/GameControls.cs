using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameControls : MonoBehaviour
{
    public static bool IsPaused { get; private set; } = false;
    public static bool IsEditMode { get; private set; } = false;

    
    public static System.Action<bool> OnEditModeChanged;

    [Header("Startup")]
    [SerializeField] private bool pauseOnLoad = true;
    [SerializeField] private bool startInEditMode = true;

    private Rigidbody[] allBodies;
    private readonly Dictionary<Rigidbody, Vector3> storedVelocities = new();
    private readonly Dictionary<Rigidbody, Vector3> storedAngularVelocities = new();

    
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
        
        if (pauseOnLoad && !IsPaused)
            PauseGame();

        if (startInEditMode) EnterEditMode();
        else ExitEditMode();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        if (pauseOnLoad && !IsPaused)
            PauseGame();

        if (startInEditMode) EnterEditMode();
        else ExitEditMode();
    }

    
    public void EnterEditMode()
    {
        IsEditMode = true;

        
        if (!IsPaused)
            PauseGame();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        OnEditModeChanged?.Invoke(true);
    }

    public void ExitEditMode()
    {
        IsEditMode = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        OnEditModeChanged?.Invoke(false);
    }

    public void ToggleEditMode()
    {
        if (IsEditMode) ExitEditMode();
        else EnterEditMode();
    }

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

                rb.WakeUp();
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
        IsEditMode = false;

        Time.timeScale = 1f;
        Physics.autoSimulation = true;

        storedVelocities.Clear();
        storedAngularVelocities.Clear();
        allBodies = null;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

   
    public void PressPlay()
    {
        if (IsEditMode)
            ExitEditMode();

        LockOnCamera.ApplyEditedPhysics();

        if (IsPaused)
            ResumeGame();
    }
}