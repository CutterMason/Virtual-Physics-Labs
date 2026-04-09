using UnityEngine;

public class ForceTableTopBarBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameControls gameControls;
    [SerializeField] private ForceTableLabController labController;
    [SerializeField] private ForceTableAdjustmentPanel adjustmentPanel;

    public void PressPlay()
    {
        if (gameControls != null && GameControls.IsEditMode)
            gameControls.ExitEditMode();

        if (gameControls != null && !GameControls.IsPaused)
            gameControls.PauseGame();

        if (labController != null)
            labController.BeginSimulation();

        if (adjustmentPanel != null)
            adjustmentPanel.RefreshUIState();
    }

    public void EnterEditMode()
    {
        if (gameControls != null && !GameControls.IsPaused)
            gameControls.PauseGame();

        if (gameControls != null)
            gameControls.EnterEditMode();

        if (labController != null)
            labController.ReturnToEditMode();

        if (adjustmentPanel != null)
            adjustmentPanel.RefreshUIState();
    }

    public void ExitEditMode()
    {
        if (gameControls != null)
            gameControls.ExitEditMode();

        if (adjustmentPanel != null)
            adjustmentPanel.RefreshUIState();
    }

    public void SetEditMode(bool isOn)
    {
        if (isOn)
            EnterEditMode();
        else
            ExitEditMode();
    }

    public void ResetLab()
    {
        if (gameControls != null && !GameControls.IsPaused)
            gameControls.PauseGame();

        if (labController != null)
            labController.ResetToDefaults();

        if (gameControls != null)
            gameControls.EnterEditMode();

        if (adjustmentPanel != null)
            adjustmentPanel.RefreshUIState();
    }
}