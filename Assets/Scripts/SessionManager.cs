using Firebase;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SessionManager : MonoBehaviour
{
    // UI elements
    [SerializeField] public TextMeshProUGUI ApprenticeCount;
    [SerializeField] public TextMeshProUGUI JourneymanCount;
    [SerializeField] public TextMeshProUGUI ExpertCount;
    [SerializeField] public TextMeshProUGUI MasterCount;
    [SerializeField] public TMP_InputField PlayerEmail;
    [SerializeField] public Button SessionButton;

    // Stores the reference location of the database
    private DatabaseReference dbReference;

    // Stores the current and next player proficiency
    public static Proficiency playerProficiency;
    public static Proficiency copiedProficiency;
    public static Proficiency newProficiency;
    public static int wrongAnswers;

    private TimeSpan[] waitingPeriod = 
    {
        new TimeSpan(),             // Always
        new TimeSpan(2, 0, 0),      // After 2 hours
        new TimeSpan(4, 0, 0),      // After 4 hours
        new TimeSpan(8, 0, 0),      // After 8 hours
        new TimeSpan(1, 0, 0, 0),   // After 1 day
        new TimeSpan(2, 0, 0, 0)    // After 2 days
    };

    // Stores the player key
    private static string playerKey;

    // Start is called before the first frame update
    void Start()
    {
        // Reset the player proficiency
        playerProficiency = null;
        copiedProficiency = null;
        newProficiency = new Proficiency();
        wrongAnswers = 0;

        // Get the root reference location of the database
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Make the button inactive
        SessionButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerProficiency != null)
        {
            // Make the button active
            SessionButton.gameObject.SetActive(true);
        }
    }

    public static void WrongAnswer()    // TODO what if the same question is answered wrong multiple times
    {
        wrongAnswers++;
    }

    public static void RightAnswer()    // TODO why is this set to zero?
    {
        wrongAnswers = 0;
    }

    public static string PlayerKey()
    {
        return playerKey;
    }

    // Fetches the key of the current player
    public void GetPlayerKey()
    {
        // Goes to the 'players' database table and searches for the user
        dbReference.Child("players").OrderByChild("email").EqualTo(PlayerEmail.text)
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
                // Unity does not know we expect exactly one result, so we must iterate 
                foreach (var childSnapshot in args.Snapshot.Children)
                {
                    // Get the key of the current database entry
                    playerKey = childSnapshot.Key;
                    Debug.Log(childSnapshot.Key);
                    // Use this key to fetch the corresponding player proficiency
                    GetPlayerProficiencies();
                }
            }
        };
    }

    // Fetches the proficiency of a player 
    private void GetPlayerProficiencies()
    {
        // Goes to the 'proficiencies' database table and searches for the key
        dbReference.Child("proficiencies").Child(playerKey)
        .GetValueAsync().ContinueWith(task =>
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
                // Convert the JSON back to a Proficiency object
                string json = snapshot.GetRawJsonValue();
                playerProficiency = JsonUtility.FromJson<Proficiency>(json);
                copiedProficiency = JsonUtility.FromJson<Proficiency>(json);
                Debug.Log(json);
                RemoveTimedProverbs();
            }
        });
    }

    // Remove proverbs from the session list that have been questioned recently
    private void RemoveTimedProverbs()
    {
        playerProficiency.apprentice = LoopProverbs(playerProficiency.apprentice);
        playerProficiency.journeyman = LoopProverbs(playerProficiency.journeyman);
        playerProficiency.expert = LoopProverbs(playerProficiency.expert);
        playerProficiency.master = LoopProverbs(playerProficiency.master);
    }

    // Loops over the given list and adds buckets to the result that have passed the waiting period
    private List<Bucket> LoopProverbs(List<Bucket> list)
    {
        List<Bucket> result = new List<Bucket>{};
        foreach (Bucket b in list)
        {
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            date = date.AddMilliseconds(b.timestamp);
            TimeSpan interval = DateTime.Now - date;
            Debug.Log("Timestamp: " + b.timestamp + ", Date: " + date.ToString());
            if (interval.CompareTo(waitingPeriod[b.stage - 1]) >= 0) 
            {
                result.Add(b);
                Debug.Log("Added: " + b.key);
            }
        }
        return result;
    }

    // Loads the first scene
    public void NextScene()
    {
        if (playerProficiency.apprentice.Count > 0)
        {
            SceneManager.LoadScene("RecognizeImage");
        }
        else if (playerProficiency.journeyman.Count > 0)
        {
            SceneManager.LoadScene("MultipleChoice");
        }
        else if (playerProficiency.expert.Count > 0)
        {
            SceneManager.LoadScene("FillBlanks");
        }
        else if (playerProficiency.master.Count > 0)
        {
            SceneManager.LoadScene("MultipleChoice");
        }
        else
        {
            Debug.Log("No proverbs available.");
        }
    }
}
