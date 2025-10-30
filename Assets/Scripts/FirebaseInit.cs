using Firebase;
using UnityEngine;

public class FirebaseInit : MonoBehaviour
{
    void Awake()
    {
        var options = new AppOptions()
        {
            ProjectId = "virtual-physics-labs",
            ApiKey = "AIzaSyAV7PYs8sr9sdzRPhEWNnMtLD99lCE72YA",
            AppId = "1:679124689249:android:83cf6d9e236e503623e520",
            StorageBucket = "virtual-physics-labs.firebasestorage.app"
        };

        FirebaseApp.Create(options);
        Debug.Log("Firebase manually initialized for desktop!");
    }
}