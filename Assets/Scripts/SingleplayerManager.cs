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
    protected List<List<Bucket>> playerProficiencyList;
    protected List<List<Bucket>> newProficiencyList;
    protected DatabaseReference dbReference;
    protected Proverb nextProverb;
    protected string currentType;
    protected string currentKey;
    protected int currentStage;

    // Variables
    protected Question currentQuestion;
    
    // Start is called before the first frame update
    protected virtual void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        playerProficiency = SessionManager.playerProficiency;
        newProficiency = SessionManager.newProficiency;

        playerProficiencyList = new List<List<Bucket>> {
            playerProficiency.apprentice, 
            playerProficiency.journeyman,
            playerProficiency.expert,
            playerProficiency.master
        };

        newProficiencyList = new List<List<Bucket>> {
            newProficiency.apprentice, 
            newProficiency.journeyman,
            newProficiency.expert,
            newProficiency.master
        };
        
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
        switch (currentStage)
        {
            case 1:
                SceneManager.LoadScene("RecognizeImage");
                break;
            case 2:
                SceneManager.LoadScene("MultipleChoice");
                break;
            case 3:
                SceneManager.LoadScene("MultipleChoice");
                break;
            case 4:
                SceneManager.LoadScene("FillBlanks");
                break;
            case 5:
                SceneManager.LoadScene("MultipleChoice");
                break;
            case 6:
                SceneManager.LoadScene("FillBlanks");
                break;
            case 7:
                SceneManager.LoadScene("MultipleChoice");
                break;
            default:
                string json = JsonUtility.ToJson(newProficiency);
                dbReference.Child("proficiencies").Child(SessionManager.PlayerKey()).SetRawJsonValueAsync(json);
                SceneManager.LoadScene("Menu");
                break;
        }
    }

    // Quit the session and return to the start menu
    public void QuitSession()
    {
        Debug.Log("Quitting session.");
        string json = JsonUtility.ToJson(newProficiency);
        dbReference.Child("proficiencies").Child(SessionManager.PlayerKey()).SetRawJsonValueAsync(json);
        SceneManager.LoadScene("Menu");
    }

    // Get the key for the next proverb in the session in chronological order
    protected void GetNextKey()
    {
        if (playerProficiency.apprentice.Count > 0)
        {
            currentKey = playerProficiency.apprentice.First().key;
            currentStage = playerProficiency.apprentice.First().stage;
            currentType = "apprentice";
        }
        else if (playerProficiency.journeyman.Count > 0)
        {
            currentKey = playerProficiency.journeyman.First().key;
            currentStage = playerProficiency.journeyman.First().stage;
            currentType = "journeyman";
        }
        else if (playerProficiency.expert.Count > 0)
        {
            currentKey = playerProficiency.expert.First().key;
            currentStage = playerProficiency.expert.First().stage;
            currentType = "expert";
        }
        else if (playerProficiency.master.Count > 0)
        {
            currentKey = playerProficiency.master.First().key;
            currentStage = playerProficiency.master.First().stage;
            currentType = "master";
        }
        else
        {
            Debug.Log("Session complete.");
            currentKey = null;
            currentStage = -1;
            currentType = "none";
        }
    }

    // Update the player proficiency into a new object
    protected void UpdateProficiency()
    {
        switch (currentType)
        {
            case "apprentice":
                SharedUpdate(0);
                break;
            case "journeyman":
                SharedUpdate(1);
                break;
            case "expert":
                SharedUpdate(2);
                break;
            case "master":
                SharedUpdate(3);
                break;
            default:
                Debug.Log("Invalid type.");
                break;
        }
    }

    // Helper function for updating the player proficiency
    private void SharedUpdate(int index)
    {
        Bucket currentBucket = playerProficiencyList[index].Find(x => x.key == currentKey);
        playerProficiencyList[index].Remove(currentBucket);
        newProficiencyList[index].Remove(currentBucket);
        // Update the timestamp of the bucket to now
        long time = (long) DateTime.Now.ToUniversalTime()
        .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        Debug.Log(time);
        currentBucket.timestamp = time;
        // Check if we should go up or down a stage
        if (SessionManager.wrongAnswers == 0 && currentBucket.stage < 7)
        {
            currentBucket.stage++;
            Debug.Log(currentKey + " stage upgraded!");
        } else if (SessionManager.wrongAnswers != 0 && currentBucket.stage > 0)
        {
            currentBucket.stage--;
            Debug.Log(currentKey + " stage downgraded...");
        }
        // Add bucket to the proficiency that corresponds to its stage
        if (currentBucket.stage <= 3) newProficiency.apprentice.Add(currentBucket);
        else if (currentBucket.stage <= 5) newProficiency.journeyman.Add(currentBucket);
        else if (currentBucket.stage == 6) newProficiency.expert.Add(currentBucket);
        else newProficiency.master.Add(currentBucket);
    }
}
