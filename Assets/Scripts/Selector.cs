using Firebase;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Selector : MonoBehaviour
{
    private Proficiency playerProficiency;
    private Proverb nextProverb;

    // Stores the reference location of the database
    private DatabaseReference dbReference = FirebaseDatabase.DefaultInstance.RootReference;

    public Selector(Proficiency playerProficiency)
    {
        this.playerProficiency = playerProficiency;
    }

    public Proverb GetNextProverb()
    {
        Proverb proverb = null;

        // Database access finishes after the function returns
        // It does not return a proverb, it just returns null
        dbReference.Child("proverbs").Child(GetNextKey())
        .GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Task could not be completed.");
                return;
            }
            
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string json = snapshot.GetRawJsonValue();
                proverb = JsonUtility.FromJson<Proverb>(json);
                Debug.Log(json);
            }
        });
        Debug.Log(proverb.phrase);
        return proverb;
    }

    private string GetNextKey()
    {
        string key;
        if (playerProficiency.apprentice.Count > 0)
        {
            key = playerProficiency.apprentice.First();
            playerProficiency.apprentice.Remove(key);
        } else if (playerProficiency.journeyman.Count > 0)
        {
            key = playerProficiency.journeyman.First();
            playerProficiency.journeyman.Remove(key);
        } else if (playerProficiency.expert.Count > 0)
        {
            key = playerProficiency.expert.First();
            playerProficiency.expert.Remove(key);
        } else if (playerProficiency.master.Count > 0)
        {
            key = playerProficiency.master.First();
            playerProficiency.master.Remove(key);
        } else 
        {
            Debug.Log("Session complete.");
            return null;
        }
        return key;
    }

    private int GetRandomBucket() 
    {
        System.Random r = new System.Random();
        int rInt = r.Next(0, 4);
        Debug.Log(rInt);
        return rInt;
    }
}
