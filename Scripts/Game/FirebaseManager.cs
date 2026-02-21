// FirebaseManager.cs

using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;

public class FirebaseManager : MonoBehaviour
{
    private DatabaseReference databaseReference;
    private FirebaseAuth auth;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            auth = FirebaseAuth.DefaultInstance;
        });
    }

    public void SignIn(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCompleted)
            {
                Debug.Log("User signed in successfully.");
            }
            else
            {
                Debug.LogError(task.Exception);
            }
        });
    }

    public void WriteData(string key, object value)
    {
        databaseReference.Child(key).SetValueAsync(value).ContinueWith(task => {
            if (task.IsCompleted)
            {
                Debug.Log("Data written successfully.");
            }
            else
            {
                Debug.LogError(task.Exception);
            }
        });
    }

    public void ReadData(string key)
    {
        databaseReference.Child(key).GetValueAsync().ContinueWith(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log("Data read successfully: " + snapshot.GetRawJsonValue());
            }
            else
            {
                Debug.LogError(task.Exception);
            }
        });
    }
}