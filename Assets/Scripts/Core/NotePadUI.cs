using UnityEngine;
using TMPro;

public class NotepadUI : MonoBehaviour
{
    public GameObject notepadPanel;
    public TMP_InputField notepadInput;

    private bool isOpen = false;

    void Start()
    {
        notepadPanel.SetActive(false);
    }

    public void ToggleNotepad()
    {
        isOpen = !isOpen;
        notepadPanel.SetActive(isOpen);

        if (isOpen)
        {
            notepadInput.Select();
            notepadInput.ActivateInputField();
        }
    }
}