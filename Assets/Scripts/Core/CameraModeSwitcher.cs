using UnityEngine;

public class CameraModeSwitcher : MonoBehaviour
{
    public LockOnCamera editModeLockOn;
    public PlayModeLockOnCamera playModeLockOn;

    void Start()
    {
        RefreshMode();
        GameControls.OnEditModeChanged += HandleEditModeChanged;
    }

    void OnDestroy()
    {
        GameControls.OnEditModeChanged -= HandleEditModeChanged;
    }

    void HandleEditModeChanged(bool isEditMode)
    {
        RefreshMode();
    }

    void RefreshMode()
    {
        bool isEditMode = GameControls.IsEditMode;

        if (editModeLockOn != null)
            editModeLockOn.enabled = isEditMode;

        if (playModeLockOn != null)
            playModeLockOn.enabled = !isEditMode;
    }
}