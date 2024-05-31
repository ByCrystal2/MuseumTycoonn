using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase;

public class FirestoreManager : MonoBehaviour
{
    public FirebaseFirestore db;
    public static FirestoreManager instance { get; private set;}
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);        
    }

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(innerTask =>
                {
                    Firebase.DependencyStatus dependencyStatus = innerTask.Result;
                    if (dependencyStatus == Firebase.DependencyStatus.Available)
                    {
                        // Firebase is ready to use.
                        db = FirebaseFirestore.DefaultInstance;
                        Debug.Log("Firebase Firestore is ready to use.");
                    }
                    else
                    {
                        Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                    }
                });
            }
            else
            {
                Debug.LogError("Failed to check and fix Firebase dependencies.");
            }
        });
    }
}
