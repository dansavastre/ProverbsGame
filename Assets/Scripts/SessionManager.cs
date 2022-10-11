using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Firebase;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using Random = System.Random;

public class SessionManager : MonoBehaviour 
{
    // UI elements
    [SerializeField] private TextMeshProUGUI ApprenticeCount;
    [SerializeField] private TextMeshProUGUI JourneymanCount;
    [SerializeField] private TextMeshProUGUI ExpertCount;
    [SerializeField] private TextMeshProUGUI MasterCount;

    // Stores the reference location of the database
    public static DatabaseReference dbReference;

    // Stores the current and next player proficiency
    public static Proficiency playerProficiency;
    public static Proficiency playerProficiencyNoFilter;
    public static Proficiency newProficiency;
    public static string playerEmail;
    public static string playerName;
    public static string playerKey;

    // Progress bar
    public static int correctAnswers;

    // The number of questions that should be allowed in a single session
    public static int maxValue = 10;

    // Variables
    public static LinkedList<Bucket> allProficiencies;
    public static List<Bucket> allProficienciesNoFilter;
    public static Dictionary<Bucket, int> dictionary;
    public static Proverb proverb;
    public static Proficiency proficiency;
    public static bool isOnDemandBeforeAnswer;

    private Random random;

    public static string[] scenes =
    {
        "FirstScreen",          // 0 First screen on app launch
        "Register",             // 1 Screen to register
        "Login",                // 2 Screen to login
        "MainMenu",             // 3 Singleplayer menu
        "FillInBlanks",         // 4 Multiplayer menu
        "InfoScreen",           // 5 Information page
        "ProfilePage",          // 6 Profile page
        "Dictionary"            // 7 Proverb dictionary
    };

    private TimeSpan[] waitingPeriod =
    {
        new TimeSpan(),             // Always
        new TimeSpan(0, 30, 0),     // After 30 minutes
        new TimeSpan(1, 0, 0),      // After 1 hour
        new TimeSpan(2, 0, 0),      // After 2 hours
        new TimeSpan(4, 0, 0),      // After 4 hours
        new TimeSpan(8, 0, 0)       // After 8 hours
    };

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    void Start() 
    {
        // Get the root reference location of the database
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Reset the player proficiency
        playerProficiency = null;
        playerProficiencyNoFilter = null;
        newProficiency = null;

        // Set player properties
        playerEmail = AccountManager.playerEmail;
        playerName = AccountManager.playerName;
        playerKey = null;

        // Instantiate variables
        random = new Random();
        isOnDemandBeforeAnswer = false;

        // Only continue if player email is given
        if (playerEmail == null) SwitchScene(0);
        else GetPlayerKey();
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update() 
    {
        if (playerProficiency != null) {
            // Display amount of proverbs in each proficiency
            DisplayProverbCount();
            ApprenticeCount.ForceMeshUpdate(true);
            JourneymanCount.ForceMeshUpdate(true);
            ExpertCount.ForceMeshUpdate(true);
            MasterCount.ForceMeshUpdate(true);
        }
    }

    /// <summary>
    /// Displays the number of proverbs in each proficiency bucket.
    /// </summary>
    private void DisplayProverbCount() 
    {
        ApprenticeCount.text = playerProficiencyNoFilter.apprentice.Count.ToString();
        JourneymanCount.text = playerProficiencyNoFilter.journeyman.Count.ToString();
        ExpertCount.text = playerProficiencyNoFilter.expert.Count.ToString();
        MasterCount.text = playerProficiencyNoFilter.master.Count.ToString();
    }

    /// <summary>
    /// Fetches the key of the current player.
    /// </summary>
    public void GetPlayerKey() {
        // Reset the player proficiency
        playerProficiency = null;
        newProficiency = null;
        // Goes to the 'players' database table and searches for the user
        dbReference.Child("players").OrderByChild("email").EqualTo(playerEmail)
        .ValueChanged += (object sender, ValueChangedEventArgs args) => {
            if (args.DatabaseError != null) {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            // Check to see if there is at least one result
            if (args.Snapshot != null && args.Snapshot.ChildrenCount > 0) {
                // Unity does not know we expect exactly one result, so we must iterate 
                foreach (var childSnapshot in args.Snapshot.Children) {
                    // Get the key of the current database entry
                    playerKey = childSnapshot.Key;
                    Debug.Log(childSnapshot.Key);
                    // Use this key to fetch the corresponding player proficiency
                    GetPlayerProficiencies();
                }
            }
        };
    }

    /// <summary>
    /// Fetches the proficiency of a player.
    /// </summary>
    private void GetPlayerProficiencies() {
        // Goes to the 'proficiencies' database table and searches for the key
        dbReference.Child("proficiencies").Child(playerKey)
        .GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.LogError("Task could not be completed.");
                return;
            } else if (task.IsCompleted) {
                // Take a snapshot of the database entry
                DataSnapshot snapshot = task.Result;
                // Convert the JSON back to a Proficiency object
                string json = snapshot.GetRawJsonValue();
                playerProficiency = JsonUtility.FromJson<Proficiency>(json);
                playerProficiencyNoFilter = JsonUtility.FromJson<Proficiency>(json);
                newProficiency = JsonUtility.FromJson<Proficiency>(json);

                Debug.Log(json);
                // RemoveTimedProverbs();
                InitList();
            }
        });
    }

    /// <summary>
    /// Method that creates the list of questions for the current single-player session.
    /// </summary>
    private void InitList() {
        // Add all proficiencies to one list
        allProficiencies = new LinkedList<Bucket>();
        allProficiencies.AddRange(playerProficiency.apprentice);
        allProficiencies.AddRange(playerProficiency.journeyman);
        allProficiencies.AddRange(playerProficiency.expert);

        // Add all proficiences to a list which is not to be filtered
        allProficienciesNoFilter = new List<Bucket>();
        allProficienciesNoFilter.AddRange(playerProficiency.apprentice);
        allProficienciesNoFilter.AddRange(playerProficiency.journeyman);
        allProficienciesNoFilter.AddRange(playerProficiency.expert);
        allProficienciesNoFilter.AddRange(playerProficiency.master);

        // Initiate ProgressBar
        correctAnswers = 0;
        maxValue = Math.Min(maxValue, allProficiencies.Count);

        Debug.Log("Pre-shuffle: " + LinkedString(allProficiencies));
        Debug.Log("List size pre-shuffle:" + allProficiencies.Count);

        allProficiencies = Shuffle(allProficiencies.ToList());
        ResizeList<Bucket>(ref allProficiencies, maxValue); // Resize the list

        Debug.Log("Post-shuffle: " + LinkedString(allProficiencies));
        Debug.Log("List size post-shuffle:" + allProficiencies.Count);

        // Create a dictionary to keep track of wrong answers
        List<int> ints = new List<int>(new int[allProficiencies.Count]);
        dictionary = new Dictionary<Bucket, int>(allProficiencies
        .Zip(ints, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v));
    }

    /// <summary>
    /// Method for resizing a list to the desired size.
    /// </summary>
    /// <typeparam name="T">symbol denoting the parameter type of the list of questions (Bucket by default)</typeparam>
    /// <param name="list">the list to be resized</param>
    /// <param name="size">the number of elements that the list should be resized to</param>
    private void ResizeList<T>(ref LinkedList<T> list, int size) {
        int curr = list.Count;
        maxValue = Math.Min(maxValue, size);
        if (size < curr) {
            T[] arr = list.ToArray();
            Array.Resize(ref arr, size);
            list = new LinkedList<T>(arr);
        }
    }

    /// <summary>
    /// Print for debugging.
    /// </summary>
    /// <param name="list">the list that should be printed</param>
    /// <returns>a formatted string denoting the list</returns>
    private string LinkedString(LinkedList<Bucket> list) {
        string result = "[";
        foreach (Bucket b in list) {
            result += "{Key: " + b.key + ", Stage: " + b.stage + "}, ";
        }
        return result + "]";
    }

    /// <summary>
    /// Randomly shuffle the items in the given list.
    /// </summary>
    /// <typeparam name="T">symbol denoting the parameter type of the list of questions (Bucket by default)</typeparam>
    /// <param name="list">the list to be shuffled</param>
    /// <returns>the shuffled list</returns>
    private LinkedList<T> Shuffle<T>(IList<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
        return new LinkedList<T>(list);
    }

    /// <summary>
    /// Remove proverbs from the session list that have been questioned recently.
    /// </summary>
    private void RemoveTimedProverbs() {
        playerProficiency.apprentice = LoopProverbs(playerProficiency.apprentice);
        playerProficiency.journeyman = LoopProverbs(playerProficiency.journeyman);
        playerProficiency.expert = LoopProverbs(playerProficiency.expert);
        playerProficiency.master = LoopProverbs(playerProficiency.master);
    }

    /// <summary>
    /// Loops over the given list and adds buckets to the result that have passed the waiting period.
    /// </summary>
    /// <param name="list">the list to be looped over</param>
    /// <returns>the list containing the buckets that have passed the waiting period</returns>
    private List<Bucket> LoopProverbs(List<Bucket> list) {
        List<Bucket> result = new List<Bucket> { };
        foreach (Bucket b in list) {
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            date = date.AddMilliseconds(b.timestamp);
            TimeSpan interval = DateTime.Now - date;
            Debug.Log("Timestamp: " + b.timestamp + ", Date: " + date.ToString());
            if (interval.CompareTo(waitingPeriod[b.stage - 1]) >= 0) {
                result.Add(b);
                Debug.Log("Added: " + b.key);
            }
        }
        return result;
    }

    /// <summary>
    /// Load the first question.
    /// </summary>
    // TODO: fix duplicate code with LoadScene() in SingleplayerManager
    public void NextScene() {
        Bucket bucket = allProficiencies.Count > 0 ? allProficiencies.First.Value : null;
        if (bucket != null) SceneManager.LoadScene(NextSceneName(bucket.stage));
        else Debug.Log("Bucket is null, no proverbs available.");
    }

    /// <summary>
    /// Gets the name of the next scene depending on the stage.
    /// </summary>
    /// <param name="stage">the number of the stage that the proverb is currently in</param>
    /// <returns>a string denoting the name of the scene that must be loaded next</returns>
    // TODO: Share method with SingleplayerManager
    public string NextSceneName(int stage)
    {
        return stage switch
        {
            1 => "MultipleChoice",
            3 => "MultipleChoice",
            5 => "MultipleChoice",
            2 => "RecognizeImage",
            4 => "FillBlanks",
            6 => "FormSentence",
            _ => ""
        };
    }

    /// <summary>
    /// Switch to the scene corresponding to the sceneIndex.
    /// </summary>
    /// <param name="sceneIndex">the index of the scene to be switched to</param>
    public static void SwitchScene(int sceneIndex) 
    {
        SceneManager.LoadScene(scenes[sceneIndex]);
    }
}