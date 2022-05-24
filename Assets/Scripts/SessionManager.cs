using Firebase;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SessionManager : MonoBehaviour
{
    // TextMeshPro input fields
    public TMP_InputField PlayerEmail;

    // TextMeshPro buttons
    public Button SessionButton;

    // Stores the reference location of the database
    private DatabaseReference dbReference;

    // Stores the current and next player proficiency
    public static Proficiency playerProficiency;
    public static Proficiency newProficiency;
    public static int wrongAnswers;

    // Stores the player key
    private static string playerKey;

    // Start is called before the first frame update
    void Start()
    {
        // Reset the player proficiency
        playerProficiency = null;
        newProficiency = new Proficiency();
        wrongAnswers = 0;

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

    public static void WrongAnswer()
    {
        wrongAnswers++;
    }

    public static void RightAnswer()
    {
        wrongAnswers = 0;
    }

    public static string PlayerKey()
    {
        return playerKey;
    }

    // Fetches the key of the current player
    public void GetPlayerKey()
    {
        // Goes to the 'players' database table and searches for the user
        dbReference.Child("players").OrderByChild("email").EqualTo(PlayerEmail.text)
        .ValueChanged += (object sender, ValueChangedEventArgs args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            // Check to see if there is at least one result
            if (args.Snapshot != null && args.Snapshot.ChildrenCount > 0)
            {
                // Unity does not know we expect exactly one result, so we must iterate 
                foreach (var childSnapshot in args.Snapshot.Children)
                {
                    // Get the key of the current database entry
                    playerKey = childSnapshot.Key;
                    Debug.Log(childSnapshot.Key);
                    // Use this key to fetch the corresponding player proficiency
                    GetPlayerProficiencies();
                }
            }
        };
    }

    // Fetches the proficiency of a player 
    private void GetPlayerProficiencies()
    {
        // Goes to the 'proficiencies' database table and searches for the key
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
                // Take a snapshot of the database entry
                DataSnapshot snapshot = task.Result;
                // Convert the JSON back to a Proficiency object
                string json = snapshot.GetRawJsonValue();
                playerProficiency = JsonUtility.FromJson<Proficiency>(json);
                Debug.Log(json);
            }
        });
    }

    // Loads the next scene
    public void NextScene()
    {
        // SceneManager.LoadScene("FillBlankGame");
        SceneManager.LoadScene("MCQVariation");
        // SceneManager.LoadScene("RecognizeImages");
    }
}
