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

public class MultipleChoiceManager : SingleplayerManager
{
    // UI elements
    [SerializeField] private Button answerButton0, answerButton1, answerButton2, answerButton3;

    public enum Mode { ProverbMeaning, MeaningProverb, ExampleSentence}
    public Mode gamemode;

    async void Start()
    {
        base.Start();

        if (currentType == "journeyman") gamemode = Mode.ProverbMeaning;
        else gamemode = Mode.ExampleSentence;

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

        SetCurrentQuestion();

        if (gamemode == Mode.ProverbMeaning)
        {
            currentQuestion.text = nextProverb.phrase;
            currentQuestion.answers[0].text = nextProverb.meaning;
            currentQuestion.answers[1].text = nextProverb.otherMeanings[0];
            currentQuestion.answers[2].text = nextProverb.otherMeanings[1];
            currentQuestion.answers[3].text = nextProverb.otherMeanings[1];
        } 
        else 
        {
            if (gamemode == Mode.MeaningProverb) currentQuestion.text = nextProverb.meaning;
            else currentQuestion.text = nextProverb.example;
            currentQuestion.answers[0].text = nextProverb.phrase;
            currentQuestion.answers[1].text = nextProverb.otherPhrases[0];
            currentQuestion.answers[2].text = nextProverb.otherPhrases[1];
            currentQuestion.answers[3].text = nextProverb.otherPhrases[1];
        }

        // Set the question and button texts
        questionText.text = currentQuestion.text;
        answerButton0.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[0].text;
        answerButton1.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[1].text;
        answerButton2.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[2].text;
        answerButton2.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[3].text;
    }

    // Load the proverb into a question
    private void SetCurrentQuestion()
    {
        // Create question and answer objects from proverb
        currentQuestion = new Question();

        Answer answer0 = new Answer();
        answer0.isCorrect = true;
        Answer answer1 = new Answer();
        answer1.isCorrect = false;
        Answer answer2 = new Answer();
        answer2.isCorrect = false;
        Answer answer3 = new Answer();
        answer3.isCorrect = false;

        Answer[] answers = {answer0, answer1, answer2, answer3};
        currentQuestion.answers = answers;
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
        DeactivateAnswerButtons();
        base.DisplayFeedback(currentQuestion.answers[index].isCorrect);
    }
}