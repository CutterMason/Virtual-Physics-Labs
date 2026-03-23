using System;
using System.IO;
using TMPro;
using UnityEngine;

public class NotesExporter : MonoBehaviour
{
    [Header("Notepad Reference")]
    public TMP_InputField notepadInput;

    [Header("File Settings")]
    public string defaultFileName = "VP1L_Notes";

    public void ExportNotesToTxt()
    {
        if (notepadInput == null)
        {
            Debug.LogError("NotesExporter: notepadInput is not assigned.");
            return;
        }

        string notesText = notepadInput.text;

        if (string.IsNullOrWhiteSpace(notesText))
        {
            Debug.LogWarning("NotesExporter: There are no notes to export.");
            return;
        }

        string safeFileName = MakeSafeFileName(defaultFileName);
        string timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"{safeFileName}_{timeStamp}.txt";

        // Saves to a safe writable folder for the app
        string folderPath = Application.persistentDataPath;
        string fullPath = Path.Combine(folderPath, fileName);

        try
        {
            File.WriteAllText(fullPath, notesText);
            Debug.Log($"Notes exported successfully to: {fullPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to export notes: {e.Message}");
        }
    }

    private string MakeSafeFileName(string fileName)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c.ToString(), "");
        }

        return string.IsNullOrWhiteSpace(fileName) ? "VP1L_Notes" : fileName;
    }

    public void OpenNotesFolder()
    {
        string folderPath = Application.persistentDataPath;

        if (Directory.Exists(folderPath))
        {
            Application.OpenURL("file://" + folderPath);
        }
        else
        {
            Debug.LogWarning("Notes folder does not exist yet.");
        }
    }
}
