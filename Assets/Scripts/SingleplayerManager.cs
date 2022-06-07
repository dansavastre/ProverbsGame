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
using Random = UnityEngine.Random;

public class SingleplayerManager : MonoBehaviour
{
    // UI elements
    [SerializeField] protected TextMeshProUGUI questionText;
    [SerializeField] protected TextMeshProUGUI resultText;
    [SerializeField] protected GameObject nextQuestionButton;
    [SerializeField] protected Button answerButtonPrefab;
    [SerializeField] protected RectTransform answerBoard;
    [SerializeField] protected List<Button> answerButtons;

    // Stores information fetched from the database
    public static Proficiency playerProficiency;
    public static Proficiency newProficiency;
    protected DatabaseReference dbReference;
    protected Proverb nextProverb;
    protected string currentType;
    protected Bucket currentBucket;

    // Variables
    protected Question currentQuestion;
    private static LinkedList<Bucket> allProficiencies;
    private static Dictionary<Bucket, int> dictionary;

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

        // Initialize new variables
        allProficiencies = SessionManager.allProficiencies;
        dictionary = SessionManager.dictionary;
        
        GetNextKey();
        nextQuestionButton.SetActive(false);
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

    // Display the feedback after the player answers the question
    protected void DisplayFeedback(bool correct)
    {
        if (correct)
        {
            resultText.text = "Correct!";
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
            if (numbers.Contains(random))
            {
                i--;
            }
            else
            {
                numbers[i] = random;
            }
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
        Button newButton = Instantiate(answerButtonPrefab, answerBoard, false);
        // Set position
        int yPos = -answerIndex * 75 + 20;
        var transform1 = newButton.transform;
        transform1.localPosition = new Vector3(transform1.localPosition.x, yPos);
        // set name, text, and callback
        newButton.name = "Answer" + answerIndex;
        newButton.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[answerIndex].text;
        newButton.onClick.AddListener(() => CheckAnswer(answerIndex));
        answerButtons.Add(newButton);
    }
    
    // Display the feedback after the player answers the question
    public void CheckAnswer(int index)
    {
        DisplayFeedback(currentQuestion.answers[index].isCorrect);
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

    // Quit the session and return to the start menu
    public void QuitSession()
    {
        Debug.Log("Quitting session.");
        string json = JsonUtility.ToJson(newProficiency);
        dbReference.Child("proficiencies").Child(SessionManager.playerKey).SetRawJsonValueAsync(json);
        SceneManager.LoadScene("Menu");
    }

    // Load the next question
    public void LoadQuestion() 
    {
        Debug.Log("Load next question.");
        GetNextKey();
        if (currentBucket == null) 
        {
            string json = JsonUtility.ToJson(newProficiency);
            dbReference.Child("proficiencies").Child(SessionManager.playerKey).SetRawJsonValueAsync(json);
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
                SceneManager.LoadScene("FormSentence");
                break;
            case 7:
                SceneManager.LoadScene("MultipleChoice");
                break;
            default:
                string json = JsonUtility.ToJson(newProficiency);
                dbReference.Child("proficiencies").Child(SessionManager.playerKey).SetRawJsonValueAsync(json);
                SceneManager.LoadScene("Menu");
                break;
        }
    }
}
