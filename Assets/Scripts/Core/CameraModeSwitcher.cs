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

        Transform targetToCarryOver = null;

        // If we are leaving edit mode, grab the current edit-mode target first.
        if (!isEditMode && editModeLockOn != null)
        {
            targetToCarryOver = editModeLockOn.CurrentTarget;
        }

        if (editModeLockOn != null)
            editModeLockOn.enabled = isEditMode;

        if (playModeLockOn != null)
        {
            playModeLockOn.enabled = !isEditMode;

            if (!isEditMode && targetToCarryOver != null)
            {
                playModeLockOn.LockOntoTarget(targetToCarryOver);
            }
        }
    }
}