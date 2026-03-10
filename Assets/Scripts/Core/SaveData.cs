using System;
using System.Collections.Generic;

[Serializable]
public class SceneSaveData
{
    public string sceneName;                    
    public List<ObjectSaveData> objects;        
    public ExperimentData experimentData;        

    public SceneSaveData()
    {
        objects = new List<ObjectSaveData>();
        experimentData = new ExperimentData();
    }
}

[Serializable]
public class ObjectSaveData
{
    public string id;        
    public float px, py, pz;
    public float rx, ry, rz; 
    public bool active;      
    public string prefabName;
    public bool isPresetObject;
}

[Serializable]
public class ExperimentData
{
    public float sliderValue;
    public bool toggleValue;
    public float timerValue;
    public string notepadText;
}