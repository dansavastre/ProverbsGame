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
    [SerializeField] private TextMeshProUGUI taskText;
    [SerializeField] private GameObject homeButton;
    [SerializeField] private GameObject barBackground;
    [SerializeField] private GameObject questionBoard;
    [SerializeField] private GameObject imageBoard;
    [SerializeField] private GameObject nextButton;

    // Sprites for UI elements
    [SerializeField] private Sprite otherHomeButton;
    [SerializeField] private Sprite otherBarBackground;
    [SerializeField] private Sprite otherImageBoard;
    [SerializeField] private Sprite otherNextButton;

    // Stores information fetched from the database
    private StorageReference storageRef;
    private string currentImage; 
    private byte[] fileContents;

    // The maximum number of bytes that will be retrieved
    private long maxAllowedSize = 1 * 1024 * 1024;

    // Variables for storing the type of multiple choice question
    public enum Mode { ProverbMeaning, MeaningProverb, ExampleSentence}
    public Mode gamemode;

    protected async override void Start()
    {
        base.Start();

        // Do not initially show the image
        image.enabled = false;

        SetMode();

        // Goes to the 'proverbs' database table and searches for the key
        await dbReference.Child("proverbs").Child(currentBucket.key)
        .GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Task (get next proverb) could not be completed.");
                return;
            }
            else if (task.IsCompleted)
            {
                // Take a snapshot of the database entry
                DataSnapshot snapshot = task.Result;
                // Convert the JSON back to a Proverb object
                string json = snapshot.GetRawJsonValue();
                nextProverb = JsonUtility.FromJson<Proverb>(json);
            }
        });
        
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

        int[] numbers = RandomPositions();

        if (gamemode == Mode.ProverbMeaning)
        {
            SetCurrentQuestion(nextProverb.meaning, nextProverb.otherMeanings);
            taskText.text = "Select the meaning";
            currentQuestion.text = nextProverb.phrase;
        }
        else
        {
            // Get other phrases from other proverbs
            List<string> otherPhrases = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                int randIndex = Random.Range(0, allProficienciesNoFilter.Count);
                string key = allProficienciesNoFilter[randIndex].key;

                // Goes to the 'proverbs' database table and searches for the key
                await dbReference.Child("proverbs").Child(key)
                .GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.LogError("Task could not be completed.");
                        i--;
                    }
                    else if (task.IsCompleted)
                    {
                        // Take a snapshot of the database entry
                        DataSnapshot snapshot = task.Result;
                        // Convert the JSON back to a Proverb object
                        string json = snapshot.GetRawJsonValue();
                        string fetchedPhrase = JsonUtility.FromJson<Proverb>(json).phrase;

                        if (fetchedPhrase.Equals(nextProverb.phrase) || otherPhrases.Contains(fetchedPhrase)) i--;
                        else otherPhrases.Add(fetchedPhrase);
                    }
                    else i--;
                });
            }
            
            SetCurrentQuestion(nextProverb.phrase, otherPhrases);
            
            taskText.text = "Select the proverb";
            if (gamemode == Mode.MeaningProverb) currentQuestion.text = nextProverb.meaning;
            else currentQuestion.text = nextProverb.example;
        }

        // Set the question text
        questionText.text = currentQuestion.text;
    }

    // Set the multiple choice mode depending on proverb stage
    private void SetMode()
    {
        if (currentBucket.stage == 1) gamemode = Mode.ProverbMeaning;
        else if (currentBucket.stage == 3) gamemode = Mode.MeaningProverb;
        else 
        {
            gamemode = Mode.ExampleSentence;
            // Change UI element sprites to different theme
            homeButton.GetComponent<Image>().sprite = otherHomeButton;
            barBackground.GetComponent<Image>().sprite = otherBarBackground;
            questionBoard.GetComponent<Image>().sprite = otherOptionBoard;
            imageBoard.GetComponent<Image>().sprite = otherImageBoard;
            nextButton.GetComponent<Image>().sprite = otherNextButton;
        }
    }

    // Create randomized list of question positions
    private int[] RandomPositions()
    {
        int[] numbers = { -1, -1, -1, -1 };
        for (int i = 0; i < 4; i++)
        {
            int random = Random.Range(0, 4);
            if (numbers.Contains(random)) i--;
            else numbers[i] = random;
        }
        return numbers;
    }

    // Functionality for clicking the hint image:
    // - if the hint image is currently hidden, show it;
    // - it the hint image is currently shown, hide it.
    // TODO: Share method
    public void HintClicked() {
        image.enabled = !image.enabled;
    }
}