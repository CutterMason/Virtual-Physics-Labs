using System;
using System.Collections.Generic;

[Serializable]
public class SceneSaveData
{
    public string sceneName;                     // Which lab scene was saved
    public List<ObjectSaveData> objects;         // List of all saved game objects
    public ExperimentData experimentData;        // Lab-specific values

    public SceneSaveData()
    {
        objects = new List<ObjectSaveData>();
        experimentData = new ExperimentData();
    }
}

[Serializable]
public class ObjectSaveData
{
    public string id;        // Unique object ID
    public float px, py, pz; // position
    public float rx, ry, rz; // rotation
    public bool active;      // active state (enabled/disabled)
    public string prefabName;
}

[Serializable]
public class ExperimentData
{
    // Add whatever your lab needs (you can expand later)
    public float sliderValue;
    public bool toggleValue;
    public float timerValue;
}