using UnityEngine;
using System;

public class SavableObject : MonoBehaviour
{
    public string uniqueId;

    private void Awake()
    {
        // If no ID exists, generate one
        if (string.IsNullOrEmpty(uniqueId))
            uniqueId = Guid.NewGuid().ToString();
    }
}
