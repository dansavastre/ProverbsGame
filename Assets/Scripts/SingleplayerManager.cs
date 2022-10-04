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
using SRandom = System.Random;

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

    // UI Manager
    [SerializeField] private UIManager UIManager;

    // Stores the reference location of the database
    public static DatabaseReference dbReference;

    // Stores information fetched from the database
    public StorageReference storageRef;
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

    // Stages corresponding to the proficiency levels
    private const int apprenticeStage = 3;
    private const int journeymanStage = 5;
    private const int expertStage = 6;
    private const int masterStage = 7;

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
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
        progressBar.SetProgress((float)SessionManager.correctAnswers / (float)SessionManager.maxValue);
    }

    /// <summary>
    /// Method for retrieving an image from the databse. This image is then used in the single-player game modes.
    /// </summary>
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

    /// <summary>
    /// Get the key for the next proverb in the session in chronological order.
    /// </summary>
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
        }
    }

    /// <summary>
    /// Display the feedback after the player answers the question, respective of whether or not the answer was correct.
    /// </summary>
    /// <param name="correct">whether or not the question has been answered correctly</param>
    protected void DisplayFeedback(bool correct)
    {
        answered = true;

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
            allProficiencies.Remove(currentBucket);
            // Wrongly answered questions are repeated after other questions are shown
            if (allProficiencies.Count >= 3) allProficiencies.AddAfter(allProficiencies.First.Next.Next, currentBucket);
            else allProficiencies.AddLast(currentBucket);
        }
        progressBar.UpdateProgress((float)SessionManager.correctAnswers / (float)SessionManager.maxValue);
        nextQuestionButton.SetActive(true);
    }

    /// <summary>
    /// Update the player proficiency into a new object.
    /// </summary>
    protected void UpdateProficiency()
    {
        // Bucket currentBucket;
        // update to a new proficiency depending to the current proficiency
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

    /// <summary>
    /// Helper function for updating the player proficiency.
    /// </summary>
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
                Debug.Log("UI Manager is there!");
                switch (currentBucket.stage)
                {
                    case 4:
                        Debug.Log("Enabling congratulations!");
                        UIManager.enableCongratulations(GetTypeOfStage(4));
                        break;
                    case 6: 
                        Debug.Log("Enabling congratulations!");
                        UIManager.enableCongratulations(GetTypeOfStage(6));
                        break;
                    case 7: 
                        Debug.Log("Enabling congratulations!");
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

    /// <summary>
    /// Helper function for checking how many stages the proverb should go down.
    /// </summary>
    /// <param name="stage">the stage number of the current proverb</param>
    /// <param name="mistakes">the number of mistakes done by the player</param>
    /// <returns></returns>
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
    
    // CurrentQuestion gets initialized and written with right values
    // Randomization is used to randomize order of answers
    // In addition, a flexible number of answer buttons is possible
    /**
     * <summary>
     * currentQuestion attribute gets initialized and written with right values.
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

        // Variable number[0] gives the index of correct answer in the "answers" array
        answers[numbers[0]].isCorrect = true;
        // Meaning is the correct answer
        answers[numbers[0]].text = correctAnswer; 
        for (int i = 1; i < numbers.Length; i++)
        {
            answers[numbers[i]].text = wrongAnswers[i-1];
        }

        answerText.text = correctAnswer;
        currentQuestion.answers = answers;

        // Set the question and create the answer buttons
        for (int i = 0; i < numbers.Length; i++)
        {
            CreateButton(i);
        }
    }
    
    /**
     * <summary>
     * Function that creates the buttons containing the possible answers to the multiple choice questions.
     * </summary>
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
        int spaceLength = 20;
        // The spacing that must be added between the buttons
        int spacing = answerIndex * spaceLength;
        
        // Compute the final position of the button
        int yPos = startLocation - answerIndex * buttonHeight - spacing;
        var transform1 = newButton.transform;
        transform1.localPosition = new Vector3(transform1.localPosition.x, yPos);
        
        // Add button properties
        newButton.name = "Answer" + answerIndex;
        newButton.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[answerIndex].text;
        if (MultipleChoiceManager.gamemode == MultipleChoiceManager.Mode.ExampleSentence) newButton.GetComponent<Image>().sprite = otherOptionBoard;
        newButton.onClick.AddListener(() => CheckAnswer(answerIndex));
        answerButtons.Add(newButton);

        StartCoroutine(DelayedAnimation(newButton));
    }
    
    /// <summary>
    /// Display the feedback after the player answers the question
    /// </summary>
    /// <param name="index">the index of the answer that was selected by the player</param>
    public void CheckAnswer(int index)
    {
        DisplayFeedback(currentQuestion.answers[index].isCorrect);
        image.enabled = true;
        if (continueOverlay != null) continueOverlay.SetActive(true);
        DeactivateAnswerButtons();
    }
    
    /// <summary>
    /// Method that deactivates all answer buttons.
    /// </summary>
    private void DeactivateAnswerButtons()
    {
        answerButtons.ForEach(delegate(Button button) { button.interactable = false; });
    }
    
    /**
     * <summary>
     * Get the proficiency type of the stage.
     * </summary>
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

    /// <summary>
    /// Method for quitting the session and returning to the main menu.
    /// </summary>
    public void QuitSession()
    {
        Debug.Log("Quitting session.");
        string json = JsonUtility.ToJson(newProficiency);
        dbReference.Child("proficiencies").Child(SessionManager.playerKey).SetRawJsonValueAsync(json);
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Method that loads the next question scene.
    /// </summary>
    public void LoadNextScene()
    {
        if (firstTimeAnswering && answeredCorrect)
        {
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

    /// <summary>
    /// Load the FunFact scene by pressing the "i" button.
    /// </summary>
    public void LoadFunFactOnDemand()
    {
        if (!answered) SessionManager.isOnDemandBeforeAnswer = true;
        else LoadFunFact();
    }

    /// <summary>
    /// Load the FunFact scene.
    /// </summary>
    public void LoadFunFact() 
    {
        SessionManager.proverb = nextProverb;
        SessionManager.proficiency = newProficiency;
        SceneManager.LoadScene("FunFact");
    }

    /// <summary>
    /// Gets the name of the next scene depending on the stage.
    /// </summary>
    /// <param name="stage">the number of the stage that the proverb is currently in</param>
    /// <returns>a string denoting the name of the scene that must be loaded next</returns>
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

    /// <summary>
    /// Plays an animation on the given button with a random delay.
    /// </summary>
    /// <param name="newButton">the button that has been pressed</param>
    /// <returns>a command telling the program to wait a random amount of time before starting the animation again</returns>
    // TODO: Share method
    private IEnumerator DelayedAnimation(Button newButton)
    {
        SRandom rnd = new SRandom();
        float randomWait = (float)rnd.Next(1, 7)/20;
        yield return new WaitForSeconds(randomWait);
        newButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Functionality for clicking the hint image:
    /// - if the hint image is currently hidden, show it;
    /// - it the hint image is currently shown, hide it.
    /// </summary>
    public void HintClicked() {
        image.enabled = !image.enabled;
    }

    /// <summary>
    /// Plays the button clicked sound once.
    /// </summary>
    // TODO: Share method
    public void PlonkNoise()
    {
        WoodButton.Play();
    }

    /// <summary>
    /// Switch to the scene corresponding to the sceneIndex.
    /// </summary>
    /// <param name="sceneIndex">the index number of the scene to switch to</param>
    // TODO: Share method
    public void SwitchScene(int sceneIndex) 
    {
        SceneManager.LoadScene(SessionManager.scenes[sceneIndex]);
    }
}