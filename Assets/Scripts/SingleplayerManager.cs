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
    public static Proficiency copiedProficiency;
    // public static Proficiency newProficiency;
    protected List<List<Bucket>> playerProficiencyList;
    protected List<List<Bucket>> copiedProficiencyList;
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
        copiedProficiency = SessionManager.copiedProficiency;
        // newProficiency = SessionManager.newProficiency;

        playerProficiencyList = new List<List<Bucket>> {
            playerProficiency.apprentice, 
            playerProficiency.journeyman,
            playerProficiency.expert,
            playerProficiency.master
        };

        copiedProficiencyList = new List<List<Bucket>> {
            copiedProficiency.apprentice, 
            copiedProficiency.journeyman,
            copiedProficiency.expert,
            copiedProficiency.master
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
                string json = JsonUtility.ToJson(copiedProficiency);
                dbReference.Child("proficiencies").Child(SessionManager.PlayerKey()).SetRawJsonValueAsync(json);
                SceneManager.LoadScene("Menu");
                break;
        }
    }

    // Quit the session and return to the start menu
    public void QuitSession()
    {
        Debug.Log("Quitting session.");
        string json = JsonUtility.ToJson(copiedProficiency);
        dbReference.Child("proficiencies").Child(SessionManager.PlayerKey()).SetRawJsonValueAsync(json);
        SceneManager.LoadScene("Menu");
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
        copiedProficiencyList[index].Remove(currentBucket);
        long time = (long) DateTime.Now.ToUniversalTime()
        .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        Debug.Log(time);
        currentBucket.timestamp = time;
        if (SessionManager.wrongAnswers == 0 && currentBucket.stage < 7)
        {
            currentBucket.stage++;
            Debug.Log(currentKey + " stage upgraded!");
        } else if (SessionManager.wrongAnswers != 0 && currentBucket.stage > 0)
        {
            currentBucket.stage--;
            Debug.Log(currentKey + " stage downgraded...");
        }
        if (currentBucket.stage <= 3) copiedProficiency.apprentice.Add(currentBucket);
        else if (currentBucket.stage <= 5) copiedProficiency.journeyman.Add(currentBucket);
        else if (currentBucket.stage == 6) copiedProficiency.expert.Add(currentBucket);
        else copiedProficiency.master.Add(currentBucket);
    }
}
