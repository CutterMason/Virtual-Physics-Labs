using System.IO;
using UnityEngine;

public class OpenNotesFolder : MonoBehaviour
{
    public void OpenFolder()
    {
        string folderPath = Application.persistentDataPath;
        Directory.CreateDirectory(folderPath);
        Application.OpenURL("file://" + folderPath);
    }
}
