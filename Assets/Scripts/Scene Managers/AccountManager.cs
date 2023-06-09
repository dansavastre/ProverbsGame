using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Firebase;
using Firebase.Database;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AccountManager : MonoBehaviour
{
    // UI elements
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField usernameField;

    // Audio source for button sound
    public static AudioSource WoodButton;

    // Stores the reference location of the database
    private DatabaseReference dbReference;

    // Player information
    public static string playerEmail;
    public static string playerName;
    private Proficiency playerProficiency;
    private string playerKey;

    /// <summary>
    /// Executes when the game is started.
    /// </summary>
    private void Start()
    {
        // Get the root reference location of the database
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Find the GameObject that contains the audio source for button sound
        WoodButton = GameObject.Find("WoodButtonAudio").GetComponent<AudioSource>();
    }

    /// <summary>
    /// Checks if given email is associated to account and logs in if it is.
    /// </summary>
    public void OnClickLogin()
    {
        playerEmail = emailField.text;

        // Check if the email is actually associated with an account by going
        // to the 'players' database table and searching for the user
        dbReference.Child("players").OrderByChild("email").EqualTo(playerEmail)
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
                    // Get the name of the current database entry
                    playerName = childSnapshot.Child("playerName").Value.ToString();
                }
                // Load next scene after succesful login
                SwitchScene(3);
            }
            else
            {
                // TODO: Put warning on screen that email is not in use
                playerEmail = null;
                playerName = null;
            }
        };
    }

    /// <summary>
    /// Checks if given email is associated to account and registers if it is not.
    /// </summary>
    public void OnClickRegister() 
    {
        playerEmail = emailField.text;
        playerName = usernameField.text;

        // Check if the email is already associated with an account by going
        // to the 'players' database table and searching for the user
        dbReference.Child("players").OrderByChild("email").EqualTo(playerEmail)
        .ValueChanged += (object sender, ValueChangedEventArgs args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            // Check to see if there is at least one result
            // TODO: Stop this if statement from running when the email is not actually in use
            if (args.Snapshot != null && args.Snapshot.ChildrenCount > 0)
            {
                // TODO: Put warning on screen that email is in use
                Debug.Log("Register: Email already in use.");
                // playerEmail = null;
            }
            else
            {
                // Add the new user to the database
                playerKey = dbReference.Child("players").Push().Key;
                dbReference.Child("players").Child(playerKey).SetRawJsonValueAsync(JsonUtility.ToJson(new Player(playerName, playerEmail)));
                // Fetches all proverbs in database and puts them in the new users' proficiency
                GetProverbs();
                // Load next scene after succesful registration
                SwitchScene(3);
            }
        };
    }

    /// <summary>
    /// Assigns each proverb to the apprentice proficiency for a new user.
    /// </summary>
    private void GetProverbs()
    {
        dbReference.Child("proverbs").GetValueAsync().ContinueWith(task =>
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
                // Get all the proverbs to be added to the apprentice bucket
                string json = snapshot.GetRawJsonValue();
                Debug.Log(json);
                playerProficiency = new Proficiency();

                foreach(DataSnapshot s in snapshot.Children)
                {
                    // Debug.Log(s.Key);
                    playerProficiency.apprentice.Add(new Bucket(s.Key, 1, 0));
                }
                dbReference.Child("proficiencies").Child(playerKey).SetRawJsonValueAsync(JsonUtility.ToJson(playerProficiency));
        }});
    }

    /// <summary>
    /// Plays the button clicked sound once.
    /// </summary>
    // TODO: Share method
    public void PlonkNoise()
    {
        WoodButton.Play();
    }

    /// <summary>
    /// Switches to another scene.
    /// </summary>
    /// <param name="sceneIndex"></param>
    // TODO: Share method
    public void SwitchScene(int sceneIndex)
    {
        SceneManager.LoadScene(SessionManager.scenes[sceneIndex]);
    }
}