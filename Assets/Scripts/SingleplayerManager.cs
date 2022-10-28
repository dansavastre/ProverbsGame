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
using Unity.VisualScripting;
using Random = UnityEngine.Random;
using SRandom = System.Random;

public class SingleplayerManager : MonoBehaviour
{
    public static DatabaseReference dbReference = AccountManager.dbReference;
    public static Proficiency playerProficiency;
    public static Proficiency newProficiency;
    public static List<Bucket> allProficienciesNoFilter;

    private static LinkedList<Bucket> allProficiencies;
    private static Dictionary<Bucket, int> dictionary;

    public StorageReference storageRef;
    public byte[] fileContents;
    public long maxAllowedSize = 1 * 1024 * 1024;

    protected Proverb nextProverb;
    protected Bucket currentBucket;
    protected Question currentQuestion;
    protected string currentType;

    private bool answeredCorrect;
    private bool answered;
    private bool firstTimeAnswering;
    private const int apprenticeStage = 3;
    private const int journeymanStage = 5;
    private const int expertStage = 6;
    private const int masterStage = 7;

    [SerializeField] public ProgressBar progressBar;
    [SerializeField] protected TextMeshProUGUI questionText;
    [SerializeField] protected RawImage image;
    [SerializeField] protected RectTransform answerBoard;
    [SerializeField] protected List<Button> answerButtons;
    [SerializeField] protected TextMeshProUGUI resultText;
    [SerializeField] protected TextMeshProUGUI answerText;
    [SerializeField] protected Button checkButton;
    [SerializeField] protected GameObject nextQuestionButton;
    [SerializeField] protected GameObject continueOverlay;
    [SerializeField] protected Sprite otherOptionBoard;
    [SerializeField] private UIManager UIManager;

    [SerializeField] protected Button answerButtonPrefab;

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    protected virtual void Start()
    {
        // Get information from external sources
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        playerProficiency = SessionManager.playerProficiency;
        newProficiency = SessionManager.newProficiency;

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
    /// Method for retrieving an image from the database. This image is then used in the singleplayer game modes.
    /// </summary>
    protected void GetImage()
    {
        // Get a reference to the storage service, using the default Firebase App
        storageRef = FirebaseStorage.DefaultInstance.GetReferenceFromUrl("gs://sp-proverb-game.appspot.com");
        StorageReference imageRef = storageRef.Child("proverbs/" + nextProverb.image);

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
        if (currentBucket == null) currentType = "none";
        else 
        {
            currentType = GetTypeOfStage(currentBucket.stage);
            firstTimeAnswering = currentBucket.timestamp == 0 ? true : false;
        }
    }

    /// <summary>
    /// Display feedback after the player answers the question, respective of whether or not the answer was correct.
    /// </summary>
    /// <param name="correct">Whether or not the question has been answered correctly.</param>
    // TODO: Move to UIManager (?)
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
        // Update to a new proficiency depending to the current proficiency
        switch (currentType)
        {
            case "apprentice":
                playerProficiency.apprentice.Remove(currentBucket);
                newProficiency.apprentice.Remove(currentBucket);
                break;
            case "journeyman":
                playerProficiency.journeyman.Remove(currentBucket);
                newProficiency.journeyman.Remove(currentBucket);
                break;
            case "expert":
                playerProficiency.expert.Remove(currentBucket);
                newProficiency.expert.Remove(currentBucket);
                break;
            case "master":
                playerProficiency.master.Remove(currentBucket);
                newProficiency.master.Remove(currentBucket);
                break;
            default:
                Debug.Log("Invalid type.");
                return;
        }
        SharedUpdate();
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
            if (UIManager != null) 
            {
                if (currentBucket.stage == 4 || currentBucket.stage == 6 || currentBucket.stage == 7)
                {
                    UIManager.enableCongratulations(GetTypeOfStage(currentBucket.stage));
                }
            }
        } 
        else if (dictionary[currentBucket] > 0 && currentBucket.stage > 1)
        {
            currentBucket.stage = ChangeStage(currentBucket.stage, dictionary[currentBucket]);
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
    /// <param name="stage">The stage number of the current proverb.</param>
    /// <param name="mistakes">The number of mistakes done by the player.</param>
    /// <returns>The new stage of the proverb.</returns>
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
            default:
                return -1;
        }
    }
    
    /// <summary>
    /// The currentQuestion attribute gets initialized and written with right values. 
    /// Randomization is used to randomize the order of the answers.
    /// In addition, a flexible number of answer buttons is possible.
    /// </summary>
    /// <param name="correctAnswer">The correct answer.</param>
    /// <param name="wrongAnswers">The wrong answers.</param>
    public void SetCurrentQuestion(string correctAnswer, List<string> wrongAnswers)
    {
        answerButtons = new List<Button>();

        // randomize order of the answers with help of numbers
        int[] numbers = new int[wrongAnswers.Count + 1]; // there are 1 + len(other phrases) answers
        for (var i = 0; i < numbers.Length; i++) numbers[i] = -1;
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
        for (int i = 1; i < numbers.Length; i++) answers[numbers[i]].text = wrongAnswers[i-1];

        answerText.text = correctAnswer;
        currentQuestion.answers = answers;

        // Set the question and create the answer buttons
        for (int i = 0; i < numbers.Length; i++) CreateButton(i);
    }
    
    /// <summary>
    /// Function that creates the buttons containing the possible answers to the multiple choice questions.
    /// </summary>
    /// <param name="answerIndex">The answer the button should contain is at this index in the answers of currentQuestion.</param>
    // TODO: Move to UIManager
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
    /// Display the feedback after the player answers the question.
    /// </summary>
    /// <param name="index">The index of the answer that was selected by the player.</param>
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
    
    /// <summary>
    /// Get the proficiency type of the stage.
    /// </summary>
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
        UIManager.SwitchScene(3);
    }

    /// <summary>
    /// Method that loads the next question scene.
    /// </summary>
    // TODO: Move to UIManager (?)
    public void LoadNextScene()
    {
        if (firstTimeAnswering && answeredCorrect)
        {
            firstTimeAnswering = false;
            LoadFunFact();
        }
        else LoadQuestion();
    }

    /// <summary>
    /// Load the next question.
    /// </summary>
    // TODO: Move to UIManager (?)
    public void LoadQuestion() 
    {
        Debug.Log("Load next question.");
        GetNextKey();
        if (currentBucket == null) 
        {
            Debug.Log("Saving progress.");
            string json = JsonUtility.ToJson(newProficiency);
            dbReference.Child("proficiencies").Child(SessionManager.playerKey).SetRawJsonValueAsync(json);
            UIManager.SwitchScene(3);
            return;
        }

        int nextScene = UIManager.NextSceneName(currentBucket.stage);
        if (nextScene != -1) UIManager.SwitchMode(nextScene);
        else 
        {
            string json = JsonUtility.ToJson(newProficiency);
            dbReference.Child("proficiencies").Child(SessionManager.playerKey).SetRawJsonValueAsync(json);
            UIManager.SwitchScene(3);
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
        UIManager.SwitchMode(0);
    }

    /// <summary>
    /// Plays an animation on the given button with a random delay.
    /// </summary>
    /// <param name="newButton">The button that has been pressed.</param>
    /// <returns>A command telling the program to wait a random amount of time.</returns>
    // TODO: Move to UIManager
    public IEnumerator DelayedAnimation(Button newButton)
    {
        SRandom rnd = new SRandom();
        float randomWait = (float)rnd.Next(1, 9)/20;
        yield return new WaitForSeconds(randomWait);
        newButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Shows and hides the hint image upon clicking it.
    /// </summary>
    // TODO: Move to UIManager
    public void HintClicked() {
        image.enabled = !image.enabled;
    }
}