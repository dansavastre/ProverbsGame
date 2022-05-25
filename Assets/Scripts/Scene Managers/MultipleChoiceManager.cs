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

    public enum Modes { ProverbMeaning, MeaningProverb, ExampleSentence}
    public Modes gamemode;

    async void Start()
    {
        base.Start();

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
        } 
        else if (gamemode == Modes.MeaningProverb)
        {
            //questions = load questions
        }
        else 
        {
            //questions = load questions
        }

        SetCurrentQuestion();
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
        DeactivateAnswerButtons();
        base.DisplayFeedback(currentQuestion.answers[index].isCorrect);
    }
}
