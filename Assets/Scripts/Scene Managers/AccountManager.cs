using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Database;
using UnityEngine.SceneManagement;

public class AccountManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField usernameField;

    private DatabaseReference dbReference;

    private Proficiency playerProficiency;
    public static string playerEmail;
    private string playerKey;

    private string[] scenes = SessionManager.scenes;

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void OnClickLogin()
    {
        Debug.Log("Login!");
        playerEmail = emailField.text;
        Debug.Log("Email: " + playerEmail);

<<<<<<< HEAD:Assets/Scripts/Scene Managers/RegisterManager.cs
    public void OnClickRegister() 
    {
        Debug.Log("Register!");
        string email = emailField.text;
        string username = usernameField.text;
        Debug.Log("Email: " + email + ", Username: " + username);

        // Check if the email is already associated with an account
=======
        // Check if the email is actually associated with an account
>>>>>>> develop:Assets/Scripts/Scene Managers/AccountManager.cs
        // Goes to the 'players' database table and searches for the user
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
<<<<<<< HEAD:Assets/Scripts/Scene Managers/RegisterManager.cs
                Debug.Log("Email already in use");
                SceneManager.LoadScene("SingleplayerMenu");
                //Debug.Log("Loaded Menu");
                //this.enabled = false;
=======
                Debug.Log("Login: Email in use.");
                // Load next scene after succesful login
                SwitchScene(3);
            }
            else
            {
                Debug.Log("Login: Email not in use.");
                playerEmail = null;
            }
        };
    }

    public void OnClickRegister() 
    {
        Debug.Log("Register!");
        playerEmail = emailField.text;
        string username = usernameField.text;
        Debug.Log("Email: " + playerEmail + ", Username: " + username);

        // Check if the email is already associated with an account
        // Goes to the 'players' database table and searches for the user
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
                Debug.Log("Register: Email already in use.");
                // playerEmail = null;
>>>>>>> develop:Assets/Scripts/Scene Managers/AccountManager.cs
            }
            else
            {
                // Add the new user to the database
                playerKey = dbReference.Child("players").Push().Key;
<<<<<<< HEAD:Assets/Scripts/Scene Managers/RegisterManager.cs
                dbReference.Child("players").Child(playerKey).SetRawJsonValueAsync(JsonUtility.ToJson(new Player(username, email)));
                //Debug.Log("PlayerKey: " + playerKey);
                
                GetProverbs();
                
                // Load menu after succesful registration
                SceneManager.LoadScene("SingleplayerMenu");
=======
                dbReference.Child("players").Child(playerKey).SetRawJsonValueAsync(JsonUtility.ToJson(new Player(username, playerEmail)));
                GetProverbs();
                // Load next scene after succesful registration
                SwitchScene(3);
>>>>>>> develop:Assets/Scripts/Scene Managers/AccountManager.cs
            }
        };
    }

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

                foreach(DataSnapshot s in snapshot.Children){
                    Debug.Log(s.Key);
                    playerProficiency.apprentice.Add(new Bucket(s.Key, 1, 0));
                }
                dbReference.Child("proficiencies").Child(playerKey).SetRawJsonValueAsync(JsonUtility.ToJson(playerProficiency));
        }});
    }

    public void SwitchScene(int sceneIndex)
    {
        SceneManager.LoadScene(scenes[sceneIndex]);
    }
}