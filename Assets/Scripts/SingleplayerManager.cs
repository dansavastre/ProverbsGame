using Firebase;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SingleplayerManager : MonoBehaviour
{
    // UI elements
    [SerializeField] protected TextMeshProUGUI questionText;
    [SerializeField] protected TextMeshProUGUI resultText;
    [SerializeField] protected GameObject nextQuestionButton;

    // Stores information fetched from the database
    public static Proficiency playerProficiency;
    public static Proficiency newProficiency;
    protected DatabaseReference dbReference;
    protected Proverb nextProverb;
    protected string currentType;
    protected string currentKey;

    // Variables
    protected Question currentQuestion;
    
    // Start is called before the first frame update
    protected void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        playerProficiency = SessionManager.playerProficiency;
        newProficiency = SessionManager.newProficiency;
        
        GetNextKey();
        nextQuestionButton.SetActive(false);
    }

    // Display the feedback after the player answers the question
    protected void DisplayFeedback(bool correct)
    {
        if (correct) 
        {
            resultText.text = "Correct!";
            UpdateProficiency();
            SessionManager.RightAnswer();
        }
        else 
        {
            resultText.text = "Incorrect!";
            SessionManager.WrongAnswer();
        }
        nextQuestionButton.SetActive(true);
    }

    // Load the next question
    public void LoadQuestion() 
    {
        Debug.Log("Load next question.");
        GetNextKey();
        switch (currentType)
        {
            case "apprentice":
                SceneManager.LoadScene("RecognizeImage");
                break;
            case "journeyman":
                SceneManager.LoadScene("MultipleChoice");
                break;
            case "expert":
                SceneManager.LoadScene("FillBlanks");
                break;
            case "master":
                SceneManager.LoadScene("MultipleChoice");
                break;
            default:
                string json = JsonUtility.ToJson(newProficiency);
                dbReference.Child("proficiencies").Child(SessionManager.PlayerKey()).SetRawJsonValueAsync(json);
                SceneManager.LoadScene("Menu");
                break;
        }
    }

    // Get the key for the next proverb in the session in chronological order
    protected void GetNextKey()
    {
        if (playerProficiency.apprentice.Count > 0)
        {
            currentKey = playerProficiency.apprentice.First().key;
            currentType = "apprentice";
        }
        else if (playerProficiency.journeyman.Count > 0)
        {
            currentKey = playerProficiency.journeyman.First().key;
            currentType = "journeyman";
        }
        else if (playerProficiency.expert.Count > 0)
        {
            currentKey = playerProficiency.expert.First().key;
            currentType = "expert";
        }
        else if (playerProficiency.master.Count > 0)
        {
            currentKey = playerProficiency.master.First().key;
            currentType = "master";
        }
        else
        {
            Debug.Log("Session complete.");
            currentKey = null;
            currentType = "none";
        }
    }

    // Update the player proficiency into a new object
    protected void UpdateProficiency()
    {
        Bucket currentBucket;
        switch (currentType)
        {
            case "apprentice":
                currentBucket = playerProficiency.apprentice.Find(x => x.key == currentKey);
                playerProficiency.apprentice.Remove(currentBucket);
                if (SessionManager.wrongAnswers == 0)
                {
                    currentBucket.stage++;
                    if (currentBucket.stage >= 4)
                    {
                        newProficiency.journeyman.Add(currentBucket);
                        // Goes to the 'proficiencies' database table and searches for the bucket
                        // dbReference.Child("proficiencies").Child(SessionManager.PlayerKey())
                        // .Child("apprentice").OrderByChild("key").EqualTo(currentBucket.key)
                        // .ValueChanged += (object sender, ValueChangedEventArgs args) =>
                        // {
                        //     if (args.DatabaseError != null)
                        //     {
                        //         Debug.LogError(args.DatabaseError.Message);
                        //         return;
                        //     }

                        //     // Check to see if there is at least one result
                        //     if (args.Snapshot != null && args.Snapshot.ChildrenCount > 0)
                        //     {
                        //         // Unity does not know we expect exactly one result, so we must iterate 
                        //         foreach (var childSnapshot in args.Snapshot.Children)
                        //         {
                        //             // Get the key of the current database entry
                        //             string bucketKey = childSnapshot.Key;
                        //             Debug.Log(childSnapshot.Key);
                        //             string json = JsonUtility.ToJson(currentBucket);
                        //             dbReference.Child("proficiencies").Child(SessionManager.PlayerKey())
                        //             .Child("apprentice").Child(bucketKey).SetRawJsonValueAsync(json);
                        //         }
                        //     }
                        // };
                        Debug.Log(currentKey + " stage upgraded, moved to journeyman!");
                    } else
                    {
                        newProficiency.apprentice.Add(currentBucket);
                        Debug.Log(currentKey + " stage upgraded, stayed in apprentice!");
                    }
                } else 
                {
                    if (currentBucket.stage > 0) currentBucket.stage--;

                    newProficiency.apprentice.Add(currentBucket);
                    Debug.Log(currentKey + " stage downgraded, stayed in apprentice...");
                }
                break;
            case "journeyman":
                currentBucket = playerProficiency.journeyman.Find(x => x.key == currentKey);
                playerProficiency.journeyman.Remove(currentBucket);
                if (SessionManager.wrongAnswers == 0)
                {
                    currentBucket.stage++;
                    if (currentBucket.stage >= 6)
                    {
                        newProficiency.expert.Add(currentBucket);
                        Debug.Log(currentKey + " stage upgraded, moved to expert!");
                    } else
                    {
                        newProficiency.journeyman.Add(currentBucket);
                        Debug.Log(currentKey + " stage upgraded, stayed in journeyman!");
                    }
                } else 
                {
                    currentBucket.stage--;
                    if (currentBucket.stage < 4)
                    {
                        newProficiency.apprentice.Add(currentBucket);
                        Debug.Log(currentKey + " stage downgraded, moved to apprentice...");
                    } else
                    {
                        newProficiency.journeyman.Add(currentBucket);
                        Debug.Log(currentKey + " stage downgraded, stayed in journeyman...");
                    }
                }
                break;
            case "expert":
                currentBucket = playerProficiency.expert.Find(x => x.key == currentKey);
                playerProficiency.expert.Remove(currentBucket);
                if (SessionManager.wrongAnswers == 0)
                {
                    currentBucket.stage++;
                    newProficiency.master.Add(currentBucket);
                    Debug.Log(currentKey + " stage upgraded, moved to master!");
                } else 
                {
                    currentBucket.stage--;
                    newProficiency.journeyman.Add(currentBucket);
                    Debug.Log(currentKey + " stage downgraded, moved to journeyman...");
                }
                break;
            case "master":
                currentBucket = playerProficiency.master.Find(x => x.key == currentKey);
                playerProficiency.master.Remove(currentBucket);
                if (SessionManager.wrongAnswers == 0)
                {
                    newProficiency.master.Add(currentBucket);
                    Debug.Log(currentKey + " stage unchanged, stayed in master!");
                } else 
                {
                    currentBucket.stage--;
                    newProficiency.expert.Add(currentBucket);
                    Debug.Log(currentKey + " moved to expert...");
                }
                break;
            default:
                Debug.Log("Invalid type.");
                break;
        }
    }
}
