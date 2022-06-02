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
using Unity.VisualScripting;

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
    protected Bucket currentBucket;

    // Variables
    protected Question currentQuestion;
    private LinkedList<Bucket> allProficiencies;
    private Dictionary<Bucket, int> dictionary;
    
    // Start is called before the first frame update
    protected virtual void Start()
    {
        // Get information from external sources
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        playerProficiency = SessionManager.playerProficiency;
        newProficiency = SessionManager.newProficiency;

        // Initialize new variables
        allProficiencies = SessionManager.allProficiencies;
        dictionary = SessionManager.dictionary;
        
        GetNextKey();
        nextQuestionButton.SetActive(false);
    }

    // Get the key for the next proverb in the session in chronological order
    protected void GetNextKey()
    {
        // Select first bucket from shuffled allProficiencies list
        currentBucket = allProficiencies.Count > 0 ? allProficiencies.First.Value : null;
        // The session is complete if there are no proverbs left
        if (currentBucket == null) 
        {
            Debug.Log("Session complete.");
            currentType = "none";
        }
        // Otherwise we fetch the next type
        else currentType = GetTypeOfStage(currentBucket.stage);
    }

    // Display the feedback after the player answers the question
    protected void DisplayFeedback(bool correct)
    {
        if (correct)
        {
            resultText.text = "Correct!";
            UpdateProficiency();
        }
        else 
        {
            resultText.text = "Incorrect!";
            dictionary[currentBucket]++;
            allProficiencies.Remove(currentBucket);
            // Wrongly answered questions are repeated after other questions are shown
            if (allProficiencies.Count >= 3) allProficiencies.AddAfter(allProficiencies.First.Next.Next, currentBucket);
            else allProficiencies.AddLast(currentBucket);
        }
        nextQuestionButton.SetActive(true);
    }

    // Update the player proficiency into a new object
    protected void UpdateProficiency()
    {
        // Bucket currentBucket;
        switch (currentType)
        {
            case "apprentice":
                playerProficiency.apprentice.Remove(currentBucket);
                newProficiency.apprentice.Remove(currentBucket);
                SharedUpdate(0);
                break;
            case "journeyman":
                playerProficiency.journeyman.Remove(currentBucket);
                newProficiency.journeyman.Remove(currentBucket);
                SharedUpdate(1);
                break;
            case "expert":
                playerProficiency.expert.Remove(currentBucket);
                newProficiency.expert.Remove(currentBucket);
                SharedUpdate(2);
                break;
            case "master":
                playerProficiency.master.Remove(currentBucket);
                newProficiency.master.Remove(currentBucket);
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
        allProficiencies.Remove(currentBucket);
        // Update the timestamp of the bucket to now
        long time = (long) DateTime.Now.ToUniversalTime()
        .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        currentBucket.timestamp = time;
        // Check if we should go up or down a stage
        if (dictionary[currentBucket] == 0 && currentBucket.stage < 7)
        {
            currentBucket.stage++;
            Debug.Log(currentBucket.key + " stage upgraded!");
        } else if (dictionary[currentBucket] != 0 && currentBucket.stage > 0)
        {
            currentBucket.stage--;
            Debug.Log(currentBucket.key + " stage downgraded...");
        }
        // Add bucket to the proficiency that corresponds to its stage
        // if (currentBucket.stage <= 3) newProficiency.apprentice.Add(currentBucket);
        // else if (currentBucket.stage <= 5) newProficiency.journeyman.Add(currentBucket);
        // else if (currentBucket.stage == 6) newProficiency.expert.Add(currentBucket);
        // else newProficiency.master.Add(currentBucket);
        string newType = GetTypeOfStage(currentBucket.stage);
        switch (newType)
        {
            case "apprentice":
                newProficiency.apprentice.Add(currentBucket);
                break;
            case "journeyman":
                newProficiency.journeyman.Add(currentBucket);
                break;
            case "expert":
                newProficiency.expert.Add(currentBucket);
                break;
            case "master":
                newProficiency.master.Add(currentBucket);
                break;
        }
    }
    
    // Get the proficiency type corresponding to the stage
    private string GetTypeOfStage(int stage)
    {
        switch (stage)
        {
            case <= 3:
                return "apprentice";
            case > 3 and <= 5:
                return "journeyman";
            case 6:
                return "expert";
            case >= 7:
                return "master";
        }
    }

    // Quit the session and return to the start menu
    public void QuitSession()
    {
        Debug.Log("Quitting session.");
        string json = JsonUtility.ToJson(newProficiency);
        dbReference.Child("proficiencies").Child(SessionManager.playerKey).SetRawJsonValueAsync(json);
        SceneManager.LoadScene("Menu");
    }

    // Load the next question
    public void LoadQuestion() 
    {
        Debug.Log("Load next question.");
        GetNextKey();
        if (currentBucket == null) 
        {
            string json = JsonUtility.ToJson(newProficiency);
            dbReference.Child("proficiencies").Child(SessionManager.playerKey).SetRawJsonValueAsync(json);
            SceneManager.LoadScene("Menu");
            return;
        }
        switch (currentBucket.stage)
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
                dbReference.Child("proficiencies").Child(SessionManager.playerKey).SetRawJsonValueAsync(json);
                SceneManager.LoadScene("Menu");
                break;
        }
    }
}
