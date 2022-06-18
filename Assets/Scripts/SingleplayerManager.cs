using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

public class SingleplayerManager : MonoBehaviour
{
    // UI elements
    [SerializeField] public ProgressBar progressBar;            // Bar at top of screen to track progress
    [SerializeField] protected TextMeshProUGUI questionText;    // Displays proverb/meaning/example
    [SerializeField] protected RawImage image;                  // Stores the image for each scene
    [SerializeField] protected RectTransform answerBoard;       // Area that answer buttons are put into
    [SerializeField] protected List<Button> answerButtons;      // Answer buttons for each question
    [SerializeField] protected TextMeshProUGUI resultText;      // Displays whether answered (in)correctly
    [SerializeField] protected TextMeshProUGUI answerText;      // Displays correct answer
    [SerializeField] protected Button checkButton;              // Used in FillBlanks and FormSentence
    [SerializeField] protected GameObject nextQuestionButton;   // Button that goes to next question
    [SerializeField] protected GameObject continueOverlay;      // Popup that shows feedback and button

    // Sprites for MultipleChoice UI elements
    [SerializeField] protected Sprite otherOptionBoard;         // Alternative theme for option board

    // UI prefabs
    [SerializeField] protected Button answerButtonPrefab;       // Prefab for the answer buttons
    [SerializeField] protected Button funFactButtonPrefab;      // Prefab for the fun fact button

    // Stores the reference location of the database
    public static DatabaseReference dbReference;

    // Stores information fetched from the database
    public StorageReference storageRef;
    public string currentImage; 
    public byte[] fileContents;

    // The maximum number of bytes that will be retrieved
    public long maxAllowedSize = 1 * 1024 * 1024;

    // Audio source for button sound
    public static AudioSource WoodButton;

    // Stores information fetched from the database
    public static Proficiency playerProficiency;
    public static Proficiency newProficiency;
    protected Proverb nextProverb;
    protected Bucket currentBucket;
    protected Question currentQuestion;
    protected string currentType;

    // Variables
    private static LinkedList<Bucket> allProficiencies;
    public static List<Bucket> allProficienciesNoFilter;
    private static Dictionary<Bucket, int> dictionary;
    private bool answeredCorrect;
    private bool answered;
    private bool firstTimeAnswering;

    // UI Manager
    private UIManager UIManager;

    // Stages corresponding to the proficiency levels
    private const int apprenticeStage = 3;
    private const int journeymanStage = 5;
    private const int expertStage = 6;
    private const int masterStage = 7;
    
    // Start is called before the first frame update
    protected virtual void Start()
    {
        // Get information from external sources
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        playerProficiency = SessionManager.playerProficiency;
        newProficiency = SessionManager.newProficiency;

        // Get the GameObject that contains the audio source for button sound
        WoodButton = AccountManager.WoodButton;

        // Instantiate variables
        allProficiencies = SessionManager.allProficiencies;
        allProficienciesNoFilter = SessionManager.allProficienciesNoFilter;
        dictionary = SessionManager.dictionary;
        answeredCorrect = false;
        SessionManager.isOnDemandBeforeAnswer = false;
        answered = false;
        
        GetNextKey();
        nextQuestionButton.SetActive(false);

        // Update Progress bar
        Debug.Log("ProgressBar: " + SessionManager.correctAnswers + " / " + SessionManager.maxValue);
        progressBar.SetProgress((float)SessionManager.correctAnswers / (float)SessionManager.maxValue);
    }

    protected void GetImage()
    {
        // Get a reference to the storage service, using the default Firebase App
        storageRef = FirebaseStorage.DefaultInstance.GetReferenceFromUrl("gs://sp-proverb-game.appspot.com");

        // Get the root reference location of the image storage
        StorageReference imageRef = storageRef.Child("proverbs/" + nextProverb.image);

        // TODO: Share this method, has no await
        // Load the proverb image from the storage
        imageRef.GetBytesAsync(maxAllowedSize).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Task (get image byte array) could not be completed.");
                return;
            }
            else if (task.IsCompleted)
            {
                fileContents = task.Result;
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileContents);
                image.GetComponent<RawImage>().texture = tex;
            }
        });
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
        else 
        {
            currentType = GetTypeOfStage(currentBucket.stage);
            firstTimeAnswering = currentBucket.timestamp == 0 ? true : false;
            Debug.Log("Timestamp: " + currentBucket.timestamp);
            Debug.Log("First time answering: " + firstTimeAnswering);
        }
    }

    // Display the feedback after the player answers the question
    protected void DisplayFeedback(bool correct)
    {
        answered = true;
        
        if(!firstTimeAnswering && funFactButtonPrefab != null)
        {
                Debug.Log("Instantiate");
                Button newButton = Instantiate(funFactButtonPrefab, this.transform);
                newButton.onClick.AddListener(() => LoadFunFactOnDemand());
        }

        if (correct)
        {
            answeredCorrect = true;
            resultText.text = "Correct!";
            SessionManager.correctAnswers++;
            UpdateProficiency();
        }
        else 
        {
            resultText.text = "Incorrect!";
            dictionary[currentBucket]++;
            Debug.Log("Mistakes: " + dictionary[currentBucket].ToString());
            allProficiencies.Remove(currentBucket);
            // Wrongly answered questions are repeated after other questions are shown
            if (allProficiencies.Count >= 3) allProficiencies.AddAfter(allProficiencies.First.Next.Next, currentBucket);
            else allProficiencies.AddLast(currentBucket);
        }
        progressBar.UpdateProgress((float)SessionManager.correctAnswers / (float)SessionManager.maxValue);
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
                SharedUpdate();
                break;
            case "journeyman":
                playerProficiency.journeyman.Remove(currentBucket);
                newProficiency.journeyman.Remove(currentBucket);
                SharedUpdate();
                break;
            case "expert":
                playerProficiency.expert.Remove(currentBucket);
                newProficiency.expert.Remove(currentBucket);
                SharedUpdate();
                break;
            case "master":
                playerProficiency.master.Remove(currentBucket);
                newProficiency.master.Remove(currentBucket);
                SharedUpdate();
                break;
            default:
                Debug.Log("Invalid type.");
                break;
        }
    }

    // Helper function for updating the player proficiency
    private void SharedUpdate()
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
            Debug.Log(currentBucket.key + " stage upgraded to " + currentBucket.stage.ToString());
            if (UIManager != null) 
            {
                switch (currentBucket.stage)
                {
                    case 4:
                        UIManager.enableCongratulations(GetTypeOfStage(4));
                        break;
                    case 6: 
                        UIManager.enableCongratulations(GetTypeOfStage(6));
                        break;
                    case 7: 
                        UIManager.enableCongratulations(GetTypeOfStage(7));
                        break;
                }
            }
        } else if (dictionary[currentBucket] > 0 && currentBucket.stage > 1)
        {
            currentBucket.stage = ChangeStage(currentBucket.stage, dictionary[currentBucket]);
            Debug.Log(currentBucket.key + " stage downgraded to " + currentBucket.stage.ToString());
        }

        // Add bucket to the proficiency that corresponds to its stage
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

    // Helper function for checking how many stages the proverb should go down
    private int ChangeStage(int stage, int mistakes)
    {
        switch (stage)
        {
            case <= journeymanStage:
                return Math.Max(1, stage - mistakes);
            case expertStage:
                return Math.Max(apprenticeStage + 1, stage - mistakes);
            case >= masterStage:
                return Math.Max(journeymanStage + 1, stage - mistakes);
        }
    }
    
    /**
     * <summary>currentQuestion attribute gets initialized and written with right values.
     * Randomization is used to randomize order of answers. In addition, a flexible number of answer buttons is possible.
     * </summary>
     * <param name="correctAnswer">The correct answer</param>
     * <param name="wrongAnswers">The wrong answers</param>
     */
    public void SetCurrentQuestion(string correctAnswer, List<string> wrongAnswers)
    {
        Debug.Log(wrongAnswers.Count());

        answerButtons = new List<Button>();
        // randomize order of the answers with help of numbers
        int[] numbers = new int[wrongAnswers.Count + 1]; // there are 1 + len(other phrases) answers
        for (var i = 0; i < numbers.Length; i++)
        {
            numbers[i] = -1;
        }
        for (int i = 0; i < numbers.Length; i++)
        {
            int random = Random.Range(0, numbers.Length);
            if (numbers.Contains(random)) i--;
            else numbers[i] = random;
        }
        // Create question and answer objects from proverb
        currentQuestion = new Question();
        Answer answer0 = new Answer();
        answer0.isCorrect = false;

        Answer answer1 = new Answer();
        answer1.isCorrect = false;

        Answer answer2 = new Answer();
        answer2.isCorrect = false;

        Answer answer3 = new Answer();
        answer3.isCorrect = false;

        Answer[] answers = {answer0, answer1, answer2, answer3};

        answers[numbers[0]].isCorrect = true;   // number[0] gives the index of correct answer in the "answers" array
        answers[numbers[0]].text = correctAnswer; // meaning is the correct answer
        for (int i = 1; i < numbers.Length; i++)
        {
            answers[numbers[i]].text = wrongAnswers[i-1];
        }

        currentQuestion.answers = answers;

        // Set the question and create the answer buttons
        for (int i = 0; i < numbers.Length; i++)
        {
            CreateButton(i);
        }
    }
    
    /**
     * <summary>Function that creates the buttons containing the possible answers to the multiple choice questions.</summary>
     * <param name="answerIndex">The answer the button should contain is at answerIndex in currentQuestion.answers.</param>
     */
    private void CreateButton(int answerIndex)
    {
        // Get board and button dimensions
        int buttonHeight = (int)answerButtonPrefab.GetComponent<RectTransform>().rect.height; 
        int boardHeight = (int)answerBoard.GetComponent<RectTransform>().rect.height;

        // Instantiate new button from prefab
        Button newButton = Instantiate(answerButtonPrefab, answerBoard, false);

        // Get the starting location of the buttons
        int startLocation = boardHeight / 2 - buttonHeight / 2; 
        // Determine the spacing between buttons
        int spaceLength = 25;
        // The spacing that must be added between the buttons
        int spacing = answerIndex * spaceLength;
        
        // Compute the final position of the button
        int yPos = startLocation - answerIndex * buttonHeight - spacing;
        var transform1 = newButton.transform;
        transform1.localPosition = new Vector3(transform1.localPosition.x, yPos);
        
        // Add button properties
        newButton.name = "Answer" + answerIndex;
        newButton.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[answerIndex].text;
        newButton.GetComponent<Image>().sprite = otherOptionBoard;
        newButton.onClick.AddListener(() => CheckAnswer(answerIndex));
        answerButtons.Add(newButton);
    }
    
    // Display the feedback after the player answers the question
    public void CheckAnswer(int index)
    {
        DisplayFeedback(currentQuestion.answers[index].isCorrect);
        image.enabled = true;
        if (continueOverlay != null) continueOverlay.SetActive(true);
        DeactivateAnswerButtons();
    }
    
    // Deactivate all answer buttons
    private void DeactivateAnswerButtons()
    {
        answerButtons.ForEach(delegate(Button button) { button.interactable = false; });
    }
    
    /**
     * <summary>Get the proficiency type of the stage</summary>
     */
    private string GetTypeOfStage(int stage)
    {
        switch (stage)
        {
            case <= apprenticeStage:
                return "apprentice";
            case > apprenticeStage and <= journeymanStage:
                return "journeyman";
            case expertStage:
                return "expert";
            case >= masterStage:
                return "master";
        }
    }

    // Quit the session and return to the main menu
    public void QuitSession()
    {
        Debug.Log("Quitting session.");
        string json = JsonUtility.ToJson(newProficiency);
        dbReference.Child("proficiencies").Child(SessionManager.playerKey).SetRawJsonValueAsync(json);
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadNextScene()
    {
        if (firstTimeAnswering && answeredCorrect)
        {
            Debug.Log("First time answered correct!");
            firstTimeAnswering = false;
            LoadFunFact();
        }
        else LoadQuestion();
    }

    // Load the next question
    public void LoadQuestion() 
    {
        Debug.Log("Load next question.");
        GetNextKey();
        if (currentBucket == null) 
        {
            Debug.Log("Saving progress.");
            string json = JsonUtility.ToJson(newProficiency);
            dbReference.Child("proficiencies").Child(SessionManager.playerKey).SetRawJsonValueAsync(json);
            SceneManager.LoadScene("MainMenu");
            return;
        }

        string nextScene = NextSceneName(currentBucket.stage);
        if (nextScene != "") SceneManager.LoadScene(nextScene);
        else 
        {
            string json = JsonUtility.ToJson(newProficiency);
            dbReference.Child("proficiencies").Child(SessionManager.playerKey).SetRawJsonValueAsync(json);
            SceneManager.LoadScene("MainMenu");
        }
    }

    // Load the FunFact scene by pressing the "i" button
    public void LoadFunFactOnDemand()
    {
        if (!answered) SessionManager.isOnDemandBeforeAnswer = true;
        else LoadFunFact();
    }

    // Load the FunFact scene
    public void LoadFunFact() 
    {
        SessionManager.proverb = nextProverb;
        SessionManager.proficiency = newProficiency;
        SceneManager.LoadScene("FunFact");
    }

    // Gets the name of the next scene depending on the stage
    // TODO: Share method with SessionManager
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

    // Plays the button clicked sound once
    // TODO: Share method
    public void PlonkNoise()
    {
        WoodButton.Play();
    }

    // Switch to the scene corresponding to the sceneIndex
    // TODO: Share method
    public void SwitchScene(int sceneIndex) 
    {
        SceneManager.LoadScene(SessionManager.scenes[sceneIndex]);
    }
}