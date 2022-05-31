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
        allProficiencies = new LinkedList<Bucket>();
        alreadyAnsweredQuesitonSet = new HashSet<Bucket>();

        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        playerProficiency = SessionManager.playerProficiency;
        
        random = new Random();
        
        // Have all proficiencies in one list 
        allProficiencies.AddRange(playerProficiency.apprentice);
        allProficiencies.AddRange(playerProficiency.journeyman);
        allProficiencies.AddRange(playerProficiency.expert);
        allProficiencies.AddRange(playerProficiency.master);
        allProficiencies.OrderBy(item => random.Next());
        
        // containing update proficiencies
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
            SessionManager.RightAnswer();   // TODO we need to change this implementation
        }
        else // TODO why is proficiency not updated here? If it should be updated, same if condition as above is needed.
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
                string json = JsonUtility.ToJson(newProficiency);
                dbReference.Child("proficiencies").Child(SessionManager.PlayerKey()).SetRawJsonValueAsync(json);
                SceneManager.LoadScene("Menu");
                break;
        }
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
    // TODO change implementation if necessary
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
