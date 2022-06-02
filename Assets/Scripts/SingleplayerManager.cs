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
using Random = System.Random;

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
    private Random random;
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
        allProficiencies = new LinkedList<Bucket>();
        random = new Random();
        
        // Add all proficiencies to one list 
        allProficiencies.AddRange(playerProficiency.apprentice);
        allProficiencies.AddRange(playerProficiency.journeyman);
        allProficiencies.AddRange(playerProficiency.expert);
        allProficiencies.AddRange(playerProficiency.master);

        Debug.Log("Pre-shuffle: " + LinkedString(allProficiencies));

        allProficiencies = Shuffle(allProficiencies.ToList());

        Debug.Log("Post-shuffle: " + LinkedString(allProficiencies));

        // Create a dictionary to keep track of wrong answers
        List<int> ints = new List<int>(new int[allProficiencies.Count]);
        dictionary = new Dictionary<Bucket, int>(allProficiencies
        .Zip(ints, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v));
        
        GetNextKey();
        nextQuestionButton.SetActive(false);
    }

    // Print for debugging
    private string LinkedString(LinkedList<Bucket> list)
    {
        string result = "[";
        foreach (Bucket b in list)
        {
            result += "{Key: " + b.key + ", Stage: " + b.stage + "}, ";
        }
        return result + "]";
    }

    // Randomly shuffle the items in the given list
    private LinkedList<T> Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }

        return new LinkedList<T>(list);
    }

    // Display the feedback after the player answers the question
    protected void DisplayFeedback(bool correct)
    {
        if (correct)
        {
            resultText.text = "Correct!";
            UpdateProficiency();

            // TODO: we need to change this implementation
            SessionManager.RightAnswer();
        }
        else 
        {
            resultText.text = "Incorrect!";
            allProficiencies.Remove(currentBucket);
            // Wrongly answered question should be repeated after 3 other questions are shown
            if (allProficiencies.Count >= 3)
            {
                allProficiencies.AddAfter(allProficiencies.First.Next.Next, currentBucket);
            }
            // If there aren't many questions left, add the wrongly answered question at the end
            else
            {
                allProficiencies.AddLast(currentBucket);
            }

            // TODO: we need to change this implementation
            SessionManager.WrongAnswer();
        }
        nextQuestionButton.SetActive(true);
    }

    // Load the next question
    public void LoadQuestion() 
    {
        Debug.Log("Load next question.");
        GetNextKey();
        if (currentBucket == null) 
        {
            string json = JsonUtility.ToJson(newProficiency);
            dbReference.Child("proficiencies").Child(SessionManager.PlayerKey()).SetRawJsonValueAsync(json);
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
    // TODO check this. Can we remove one list from the attributes?
    private void SharedUpdate(int index)
    {
        allProficiencies.Remove(currentBucket);
        // Update the timestamp of the bucket to now
        long time = (long) DateTime.Now.ToUniversalTime()
        .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        Debug.Log(time);
        currentBucket.timestamp = time;
        // Check if we should go up or down a stage
        if (SessionManager.wrongAnswers == 0 && currentBucket.stage < 7)
        {
            currentBucket.stage++;
            Debug.Log(currentBucket.key + " stage upgraded!");
        } else if (SessionManager.wrongAnswers != 0 && currentBucket.stage > 0)
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
    
    /**
     * <summary>Get the proficiency type of the stage</summary>
     */
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
}
