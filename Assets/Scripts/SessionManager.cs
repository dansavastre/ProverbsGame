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

public class SessionManager : MonoBehaviour {
    // UI elements
    [SerializeField] public TextMeshProUGUI ApprenticeCount;
    [SerializeField] public TextMeshProUGUI JourneymanCount;
    [SerializeField] public TextMeshProUGUI ExpertCount;
    [SerializeField] public TextMeshProUGUI MasterCount;
    [SerializeField] public Button SessionButton;

    private static AudioSource WoodButton;

    // Stores the reference location of the database
    private DatabaseReference dbReference;
    public static DatabaseReference dbReferenceStatic;

    // Stores the current and next player proficiency
    public static Proficiency playerProficiency;
    public static Proficiency playerProficiencyNoFilter;
    public static Proficiency newProficiency;
    public static string playerEmail;
    public static string playerName;
    public static string playerKey;

    // Progress bar
    public static int maxValue = 10; // The number of questions that should be allowed in a single session
    public static int correctAnswers;

    private Random random;
    public static LinkedList<Bucket> allProficiencies;
    public static List<Bucket> allProficienciesNoFilter;
    public static Dictionary<Bucket, int> dictionary;

    public static Proverb proverb;
    public static Proficiency proficiency;
    public static bool isOnDemandBeforeAnswer;

    public static string[] scenes =
    {
        "FirstScreen",          // First screen on app launch
        "Register",             // Screen to register
        "Login",                // Screen to login
        "SelectionMenu",        // Select singleplayer or multiplayer
        "SingleplayerMenu",     // Singleplayer menu
        "TitleMenu",            // Multiplayer menu
        "InfoScreen",           // Information page
        "ProfilePage"          // Profile page
    };

    private TimeSpan[] waitingPeriod =
    {
        new TimeSpan(),             // Always
        new TimeSpan(2, 0, 0),      // After 2 hours
        new TimeSpan(4, 0, 0),      // After 4 hours
        new TimeSpan(8, 0, 0),      // After 8 hours
        new TimeSpan(1, 0, 0, 0),   // After 1 day
        new TimeSpan(2, 0, 0, 0)    // After 2 days
    };

    // Start is called before the first frame update
    void Start() {
        // Reset the player proficiency
        playerProficiency = null;
        playerProficiencyNoFilter = null;
        newProficiency = null;
        playerEmail = AccountManager.playerEmail;
        playerName = AccountManager.playerName;
        playerKey = null;
        random = new Random();
        isOnDemandBeforeAnswer = false;

        WoodButton = AccountManager.WoodButton;

        // Get the root reference location of the database
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        dbReferenceStatic = dbReference;

        // Make the button inactive
        if (playerEmail == null) {
            Debug.Log("No email was given, returning to first screen.");
            SwitchScene(0);
        } else {
            GetPlayerKey();
        }
    }

    public void PlonkNoise() {
        WoodButton.Play();
    }

    // Update is called once per frame
    void Update() {
        if (playerProficiency != null) {
            // Make the button active
            SessionButton.gameObject.SetActive(true);

            // Display amount of proverbs in each proficiency
            DisplayProverbCount();
            ApprenticeCount.ForceMeshUpdate(true);
            JourneymanCount.ForceMeshUpdate(true);
            ExpertCount.ForceMeshUpdate(true);
            MasterCount.ForceMeshUpdate(true);

            dbReferenceStatic = dbReference;
        }
    }

    // Displays the number of proverbs in each proficiency bucket
    private void DisplayProverbCount() 
    {
        ApprenticeCount.text = playerProficiencyNoFilter.apprentice.Count.ToString();
        JourneymanCount.text = playerProficiencyNoFilter.journeyman.Count.ToString();
        ExpertCount.text = playerProficiencyNoFilter.expert.Count.ToString();
        MasterCount.text = playerProficiencyNoFilter.master.Count.ToString();
    }

    // Fetches the key of the current player
    public void GetPlayerKey() {
        // Reset the player proficiency
        playerProficiency = null;
        newProficiency = null;
        //email = playerEmail.text;
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

    // Fetches the proficiency of a player 
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
                //RemoveTimedProverbs();
                InitList();
            }
        });
    }

    private void InitList() {
        // Add all proficiencies to one list
        allProficiencies = new LinkedList<Bucket>();
        allProficiencies.AddRange(playerProficiency.apprentice);
        allProficiencies.AddRange(playerProficiency.journeyman);
        allProficiencies.AddRange(playerProficiency.expert);
        allProficiencies.AddRange(playerProficiency.master);

        // Add all proficiences to a list which is not to be filtered
        allProficienciesNoFilter = new List<Bucket>();
        allProficienciesNoFilter.AddRange(playerProficiency.apprentice);
        allProficienciesNoFilter.AddRange(playerProficiency.journeyman);
        allProficienciesNoFilter.AddRange(playerProficiency.expert);
        allProficienciesNoFilter.AddRange(playerProficiency.master);

        // Initiate ProgressBar
        correctAnswers = 0;

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

    /**
     * Method for resizing a list to the desired size.
     * 
     * list - the list to be resized
     * size - the number of elements that the list should be resized to
     * c - placeholder element for padding the end of the list
     */
    private void ResizeList<T>(ref LinkedList<T> list, int size) {
        int curr = list.Count;
        if (size < curr) {
            T[] arr = list.ToArray();
            Array.Resize(ref arr, size);
            list = new LinkedList<T>(arr);
        }
    }

    // Print for debugging
    private string LinkedString(LinkedList<Bucket> list) {
        string result = "[";
        foreach (Bucket b in list) {
            result += "{Key: " + b.key + ", Stage: " + b.stage + "}, ";
        }
        return result + "]";
    }

    // Randomly shuffle the items in the given list
    private LinkedList<T> Shuffle<T>(IList<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
        return new LinkedList<T>(list);
    }

    // Remove proverbs from the session list that have been questioned recently
    private void RemoveTimedProverbs() {
        playerProficiency.apprentice = LoopProverbs(playerProficiency.apprentice);
        playerProficiency.journeyman = LoopProverbs(playerProficiency.journeyman);
        playerProficiency.expert = LoopProverbs(playerProficiency.expert);
        playerProficiency.master = LoopProverbs(playerProficiency.master);
    }

    // Loops over the given list and adds buckets to the result that have passed the waiting period
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

    // Load the first question 
    // TODO: fix duplicate code with LoadScene() in SingleplayerManager
    public void NextScene() {
        Bucket bucket = allProficiencies.Count > 0 ? allProficiencies.First.Value : null;
        switch (bucket.stage) {
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
                SceneManager.LoadScene("FormSentence");
                break;
            case 7:
                SceneManager.LoadScene("MultipleChoice");
                break;
            default:
                Debug.Log("No proverbs available.");
                break;
        }
    }

    // Switch to the scene corresponding to the sceneIndex
    public void SwitchScene(int sceneIndex) {
        SceneManager.LoadScene(scenes[sceneIndex]);
    }
}
