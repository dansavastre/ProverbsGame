using Firebase;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class MultipleChoiceManager : SingleplayerManager
{
    // UI elements
    [SerializeField] private RawImage image;
    [SerializeField] private TextMeshProUGUI taskText;
    [SerializeField] private Button answerButton0, answerButton1, answerButton2, answerButton3;

    // Stores information fetched from the database
    private StorageReference storageRef;
    private string currentImage;

    private byte[] fileContents;

    public enum Mode { ProverbMeaning, MeaningProverb, ExampleSentence}
    public Mode gamemode;

    protected async override void Start()
    {
        base.Start();

        image.enabled = false;

        if (currentBucket.stage == 2) gamemode = Mode.ProverbMeaning;
        else if (currentBucket.stage == 3) gamemode = Mode.MeaningProverb;
        else gamemode = Mode.ExampleSentence;

        // Goes to the 'proverbs' database table and searches for the key
        await dbReference.Child("proverbs").Child(currentBucket.key)
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
        
        // Get a reference to the storage service, using the default Firebase App
        storageRef = FirebaseStorage.DefaultInstance.GetReferenceFromUrl("gs://sp-proverb-game.appspot.com");

        // Reference for retrieving an image
        StorageReference imageRef = storageRef.Child("proverbs/" + nextProverb.image);
        Debug.Log("proverbs/" + nextProverb.image);

        const long maxAllowedSize = 1 * 1024 * 1024;
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
                Debug.Log("Finished downloading!");
            }
        });

        //Create randomized list of question positions
        int[] numbers = { -1, -1, -1, -1 };
        for (int i = 0; i < 4; i++)
        {
            int random = Random.Range(0, 4);
            if (numbers.Contains(random)) i--;
            else numbers[i] = random;
        }

        SetCurrentQuestion(numbers[0]);

        if (gamemode == Mode.ProverbMeaning)
        {
            taskText.text = "Choose the meaning belonging to the proverb below.";
            currentQuestion.text = nextProverb.phrase;

            currentQuestion.answers[numbers[0]].text = nextProverb.meaning;
            currentQuestion.answers[numbers[1]].text = nextProverb.otherMeanings[0];
            currentQuestion.answers[numbers[2]].text = nextProverb.otherMeanings[1];
            currentQuestion.answers[numbers[3]].text = nextProverb.otherMeanings[1];
        }
        else
        {
            if (gamemode == Mode.MeaningProverb)
            {
                taskText.text = "Choose the proverb belonging to the meaning below.";
                currentQuestion.text = nextProverb.meaning;
            }
            else
            {
                taskText.text = "Choose the proverb belonging in the example below.";
                currentQuestion.text = nextProverb.example;
            }
            currentQuestion.answers[numbers[0]].text = nextProverb.phrase;
            currentQuestion.answers[numbers[1]].text = nextProverb.otherPhrases[0];
            currentQuestion.answers[numbers[2]].text = nextProverb.otherPhrases[1];
            currentQuestion.answers[numbers[3]].text = nextProverb.otherPhrases[1];
        }

        // Set the question and button texts
        questionText.text = currentQuestion.text;
        answerButton0.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[0].text;
        answerButton1.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[1].text;
        answerButton2.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[2].text;
        answerButton3.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[3].text;
    }

    // Load the image when a hint is asked for
    public void GetHint()
    {
        image.enabled = true;
    }

    // Load the proverb into a question
    private void SetCurrentQuestion(int correct)
    {
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

        Answer[] answers = { answer0, answer1, answer2, answer3 };

        answers[correct].isCorrect = true;
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
