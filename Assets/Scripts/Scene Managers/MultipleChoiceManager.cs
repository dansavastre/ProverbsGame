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

public class MultipleChoiceManager : MonoBehaviour
{
    // UI elements
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button answerButton0, answerButton1, answerButton2, answerButton3;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private GameObject nextQuestionButton;

    // Stores information fetched from the database
    private DatabaseReference dbReference;
    public static Proficiency playerProficiency;
    public static Proficiency newProficiency;
    private Proverb nextProverb;
    private string currentType;
    private string currentKey;

    private Question currentQuestion;

    public enum Modes { ProverbMeaning, MeaningProverb, ExampleSentence}
    public Modes gamemode;

    async void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        playerProficiency = SessionManager.playerProficiency;
        newProficiency = SessionManager.newProficiency;
        currentKey = GetNextKey();

        // TODO: Move this to its own script file in the future
        // This is hard because of the asynchronous calls to the database

        // Goes to the 'proverbs' database table and searches for the key
        await dbReference.Child("proverbs").Child(currentKey)
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
                // Convert the JSON back to a Proverb object
                string json = snapshot.GetRawJsonValue();
                nextProverb = JsonUtility.FromJson<Proverb>(json);
                Debug.Log(json);
            }
        });

        if (gamemode == Modes.ExampleSentence)
        {
            //questions = load questions
        }else if(gamemode == Modes.MeaningProverb)
        {
            //questions = load questions
        }
        else {
            //questions = load questions
        }

        SetCurrentQuestion();
        nextQuestionButton.SetActive(false);
    }

    // Get the key for the next proverb in the session in chronological order
    private string GetNextKey()
    {
        if (playerProficiency.apprentice.Count > 0)
        {
            currentKey = playerProficiency.apprentice.First();
            currentType = "apprentice";
        }
        else if (playerProficiency.journeyman.Count > 0)
        {
            currentKey = playerProficiency.journeyman.First();
            currentType = "journeyman";
        }
        else if (playerProficiency.expert.Count > 0)
        {
            currentKey = playerProficiency.expert.First();
            currentType = "expert";
        }
        else if (playerProficiency.master.Count > 0)
        {
            currentKey = playerProficiency.master.First();
            currentType = "master";
        }
        else
        {
            Debug.Log("Session complete.");
            return null;
        }
        return currentKey;
    }

    // Load the proverb into a question
    private void SetCurrentQuestion()
    {
        // Create question and answer objects from proverb
        currentQuestion = new Question();

        Answer answer0 = new Answer();
        answer0.text = nextProverb.meaning;
        answer0.isCorrect = true;

        Answer answer1 = new Answer();
        answer1.text = nextProverb.otherMeanings[0];
        answer1.isCorrect = false;

        Answer answer2 = new Answer();
        answer2.text = nextProverb.otherMeanings[1];
        answer2.isCorrect = false;

        Answer answer3 = new Answer();
        answer3.text = nextProverb.otherMeanings[1];
        answer3.isCorrect = false;

        Answer[] answers = {answer0, answer1, answer2, answer3};
        currentQuestion.answers = answers;

        // Set the question and button texts
        questionText.text = currentQuestion.text;
        answerButton0.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[0].text;
        answerButton1.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[1].text;
        answerButton2.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[2].text;
        answerButton2.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[3].text;
    }

    // Deactivate all answer buttons
    private void DeactivateAnswerButtons()
    {
        answerButton0.interactable = false;
        answerButton1.interactable = false;
        answerButton2.interactable = false;
        answerButton3.interactable = false;
    }

    // Display the feedback after the player answers the question
    public void CheckAnswer(int index)
    {
        if (currentQuestion.answers[index].isCorrect) 
        {
            resultText.text = "Correct!";
            UpdateProficiency();
            SessionManager.RightAnswer();
        }
        else 
        {
            resultText.text = "Incorrect!";
            SessionManager.WrongAnswer();
        }
        DeactivateAnswerButtons();
        nextQuestionButton.SetActive(true);
    }

    // Update the player proficiency into a new object
    private void UpdateProficiency()
    {
        switch (currentType)
        {
            case "apprentice":
                playerProficiency.apprentice.Remove(currentKey);
                if (SessionManager.wrongAnswers == 0)
                {
                    newProficiency.journeyman.Add(currentKey);
                    Debug.Log(currentKey + " moved to journeyman!");
                } else 
                {
                    newProficiency.apprentice.Add(currentKey);
                    Debug.Log(currentKey + " stayed in apprentice...");
                }
                break;
            case "journeyman":
                playerProficiency.journeyman.Remove(currentKey);
                if (SessionManager.wrongAnswers == 0)
                {
                    newProficiency.expert.Add(currentKey);
                    Debug.Log(currentKey + " moved to expert!");
                } else 
                {
                    newProficiency.apprentice.Add(currentKey);
                    Debug.Log(currentKey + " moved to apprentice...");
                }
                break;
            case "expert":
                playerProficiency.expert.Remove(currentKey);
                if (SessionManager.wrongAnswers == 0)
                {
                    newProficiency.master.Add(currentKey);
                    Debug.Log(currentKey + " moved to master!");
                } else 
                {
                    newProficiency.journeyman.Add(currentKey);
                    Debug.Log(currentKey + " moved to journeyman...");
                }
                break;
            case "master":
                playerProficiency.master.Remove(currentKey);
                if (SessionManager.wrongAnswers == 0)
                {
                    newProficiency.master.Add(currentKey);
                    Debug.Log(currentKey + " stayed in master!");
                } else 
                {
                    newProficiency.expert.Add(currentKey);
                    Debug.Log(currentKey + " moved to expert...");
                }
                break;
            default:
                Debug.Log("Invalid type.");
                break;
        }
    }

    // Load the next scene and thus question
    public void NextQuestion()
    {
        // Query the db for the next question and display it to the user using the already 
        // implemented methods, for now we will just show a message in the console
        if (GetNextKey() == null) {
            string json = JsonUtility.ToJson(newProficiency);
            dbReference.Child("proficiencies").Child(SessionManager.PlayerKey()).SetRawJsonValueAsync(json);
            SceneManager.LoadScene("Menu");
            return;
        }
        Debug.Log("Load next question");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
