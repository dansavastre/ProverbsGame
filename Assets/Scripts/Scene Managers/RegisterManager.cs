using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Database;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class RegisterManager : MonoBehaviour
{

    [SerializeField]
    private TMP_InputField emailField;
    [SerializeField]
    private TMP_InputField usernameField;

    private DatabaseReference dbReference;
    private string playerKey;
    private Proficiency playerProficiency;

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void OnClickRegister() 
    {
        Debug.Log("Register!");
        string email = emailField.text;
        string username = usernameField.text;
        Debug.Log("Email: " + email + ", Username: " + username);        

        // Add the new user to the database
        playerKey = dbReference.Child("players").Push().Key;
        dbReference.Child("players").Child(playerKey).SetRawJsonValueAsync(JsonUtility.ToJson(new Player(username, email)));
        Debug.Log("PlayerKey: " + playerKey);
        
        GetProverbs();
        
        // Load menu after succesful registration
        SceneManager.LoadScene("Menu");
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

    public void OnClickSkip() 
    {
        Debug.Log("Skip!");
        SceneManager.LoadScene("Menu");
    }
}
