using Firebase;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionManager : MonoBehaviour
{
    // TextMeshPro input fields
    public TMP_InputField PlayerEmail;

    // TextMeshPro buttons
    public Button SessionButton;

    // Stores the reference location of the database
    private DatabaseReference dbReference;

    // Stores the player key
    private string playerKey;
    private Proficiency playerProficiency;

    // Start is called before the first frame update
    void Start()
    {
        // Get the root reference location of the database
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Make the button inactive
        SessionButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerProficiency != null)
        {
            // Make the button active
            SessionButton.gameObject.SetActive(true);
        }
    }

    public void GetPlayerKey()
    {
        dbReference.Child("players").OrderByChild("email").EqualTo(PlayerEmail.text)
        .ValueChanged += (object sender, ValueChangedEventArgs args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            if (args.Snapshot != null && args.Snapshot.ChildrenCount > 0)
            {
                foreach (var childSnapshot in args.Snapshot.Children)
                {
                    playerKey = childSnapshot.Key;
                    Debug.Log(childSnapshot.Key);
                    GetPlayerProficiencies();
                }
            }
        };
    }

    private void GetPlayerProficiencies()
    {
        dbReference.Child("proficiencies").Child(playerKey)
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
                playerProficiency = JsonUtility.FromJson<Proficiency>(json);
                Debug.Log(json);
            }
        });
    }
}
