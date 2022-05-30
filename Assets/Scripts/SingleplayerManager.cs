using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    protected string currentKey;
    protected Bucket currentBucket;

    // Variables
    protected Question currentQuestion;
    private Random random;
    private LinkedList<Bucket> allProficiencies;
    private HashSet<Bucket> alreadyAnsweredQuesitonSet;
    
    // Start is called before the first frame update
    protected void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        playerProficiency = SessionManager.playerProficiency;
        random = new Random();
        allProficiencies.AddRange(playerProficiency.apprentice);
        allProficiencies.AddRange(playerProficiency.journeyman);
        allProficiencies.AddRange(playerProficiency.expert);
        allProficiencies.AddRange(playerProficiency.master);
        allProficiencies.OrderBy(item => random.Next());
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
            // SessionManager.RightAnswer(); // TODO I don't think needed anymore
        }
        else // TODO why is proficiency not updated here? If it should be updated, same if condition as above is needed.
        {
            resultText.text = "Incorrect!";
            if (allProficiencies.Count >= 3)
            {
                allProficiencies.AddAfter(allProficiencies.First.Next.Next, currentBucket);
            }
            else
            {
                allProficiencies.AddLast(currentBucket);
            }
            // SessionManager.WrongAnswer();   // TODO I don't think needed anymore
        }
        UpdateProficiency(correct);
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
        currentBucket = allProficiencies.Count > 0 ? allProficiencies.First.Value : null;
        if (currentBucket == null)
        {
            Debug.Log("Session complete.");
            currentKey = null;
            currentType = "none";
        }
        else
        {
            currentKey = currentBucket.key;
            currentType = GetTypeOfStage(currentBucket.stage);
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
            }
        }
    }

    // Update the player proficiency into a new object
    // TODO check this more thoroughly
    protected void UpdateProficiency(bool questionAnsweredCorrectly)
    {
        if (alreadyAnsweredQuesitonSet.Contains(currentBucket)) { return;}  // If this bucket was already moved during this session, don't move it again
        
        switch (currentType)    // remove currentBucket from the proficiency list that contained it
            {
                case "apprentice":
                    playerProficiency.apprentice.Remove(currentBucket);
                    break;
                case "journeyman":
                    playerProficiency.journeyman.Remove(currentBucket);
                    break;
                case "expert":
                    playerProficiency.expert.Remove(currentBucket);
                    break;
                case "master":
                    playerProficiency.master.Remove(currentBucket);
                    break;
                default:
                    Debug.Log("Invalid type.");
                    break;
            }

        currentBucket.stage += questionAnsweredCorrectly ? 1 : -1;
        switch (currentBucket.stage)
        {
            case < 1:
                currentBucket.stage = 1;
                break;
            case > 7:
                currentBucket.stage = 7;
                break;
        }

        string newType = GetTypeOfStage(currentBucket.stage);

        switch (newType)    // Add currentBucket to its new proficiency list
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
                default:
                    Debug.Log("Invalid type.");
                    break;
            }
    }

    private string GetTypeOfStage(int stage)
    {
        switch (stage)    // Add currentBucket to its new proficiency list
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
