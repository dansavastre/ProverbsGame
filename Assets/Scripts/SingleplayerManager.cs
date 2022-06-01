using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
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
    public static Proficiency copiedProficiency;
    // public static Proficiency newProficiency;
    protected List<List<Bucket>> playerProficiencyList;
    protected List<List<Bucket>> copiedProficiencyList;
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
        allProficiencies = new LinkedList<Bucket>();
        alreadyAnsweredQuesitonSet = new HashSet<Bucket>();

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
        
        random = new Random();
        
        // Have all proficiencies in one list 
        allProficiencies.AddRange(playerProficiency.apprentice);
        allProficiencies.AddRange(playerProficiency.journeyman);
        allProficiencies.AddRange(playerProficiency.expert);
        allProficiencies.AddRange(playerProficiency.master);
        allProficiencies.OrderBy(item => random.Next());
        
        // containing update proficiencies
        // newProficiency = SessionManager.newProficiency;
        
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
            // wrong question should be repeated after 3 other questions are repeated
            if (allProficiencies.Count >= 3)
            {
                allProficiencies.AddAfter(allProficiencies.First.Next.Next, currentBucket);
            }
            else    // if there aren't many questions left, add the wrong answered question at the end
            {
                allProficiencies.AddLast(currentBucket);
            }
            SessionManager.WrongAnswer();   // TODO we need to change this implementation
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
        // select first bucket as allProficiencies list is already randomized
        currentBucket = allProficiencies.Count > 0 ? allProficiencies.First.Value : null;
        if (currentBucket == null)  // session is completed in this case
        {
            Debug.Log("Session complete.");
            currentKey = null;
            currentType = "none";
        }
        else
        {
            currentKey = currentBucket.key;
            currentType = GetTypeOfStage(currentBucket.stage);
        }
    }

    // Update the player proficiency into a new object
    protected void UpdateProficiency()
    {
        Bucket currentBucket;
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
    // TODO check this. Can we remove one list from the attributes?
    private void SharedUpdate(int index)
    {
        // Bucket currentBucket = playerProficiencyList[index].Find(x => x.key == currentKey);
        playerProficiencyList[index].Remove(currentBucket);
        copiedProficiencyList[index].Remove(currentBucket);
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
        // if (currentBucket.stage <= 3) copiedProficiency.apprentice.Add(currentBucket);
        // else if (currentBucket.stage <= 5) copiedProficiency.journeyman.Add(currentBucket);
        // else if (currentBucket.stage == 6) copiedProficiency.expert.Add(currentBucket);
        // else copiedProficiency.master.Add(currentBucket);
        string newType = GetTypeOfStage(currentBucket.stage);
        switch (newType)
        {
            case "apprentice":
                copiedProficiency.apprentice.Add(currentBucket);
                break;
            case "journeyman":
                copiedProficiency.journeyman.Add(currentBucket);
                break;
            case "expert":
                copiedProficiency.expert.Add(currentBucket);
                break;
            case "master":
                copiedProficiency.master.Add(currentBucket);
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
    
    
    
    // // TODO check this more thoroughly
    // protected void UpdateProficiency(bool questionAnsweredCorrectly)
    // {
    //     if (alreadyAnsweredQuesitonSet.Contains(currentBucket)) { return;}  // If this bucket was already moved during this session, don't move it again
    //     
    //     switch (currentType)    // remove currentBucket from the proficiency list that contained it
    //         {
    //             case "apprentice":
    //                 playerProficiency.apprentice.Remove(currentBucket);
    //                 break;
    //             case "journeyman":
    //                 playerProficiency.journeyman.Remove(currentBucket);
    //                 break;
    //             case "expert":
    //                 playerProficiency.expert.Remove(currentBucket);
    //                 break;
    //             case "master":
    //                 playerProficiency.master.Remove(currentBucket);
    //                 break;
    //             default:
    //                 Debug.Log("Invalid type.");
    //                 break;
    //         }
    //
    //     currentBucket.stage += questionAnsweredCorrectly ? 1 : -1;
    //     switch (currentBucket.stage)
    //     {
    //         case < 1:
    //             currentBucket.stage = 1;
    //             break;
    //         case > 7:
    //             currentBucket.stage = 7;
    //             break;
    //     }
    //
    //     string newType = GetTypeOfStage(currentBucket.stage);
    //
    //     switch (newType)    // Add currentBucket to its new proficiency list
    //         {
    //             case "apprentice":
    //                 newProficiency.apprentice.Add(currentBucket);
    //                 break;
    //             case "journeyman":
    //                 newProficiency.journeyman.Add(currentBucket);
    //                 break;
    //             case "expert":
    //                 newProficiency.expert.Add(currentBucket);
    //                 break;
    //             case "master":
    //                 newProficiency.master.Add(currentBucket);
    //                 break;
    //             default:
    //                 Debug.Log("Invalid type.");
    //                 break;
    //         }
    // }
}
