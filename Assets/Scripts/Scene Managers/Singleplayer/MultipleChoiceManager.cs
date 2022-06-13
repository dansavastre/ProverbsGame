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

            if (task.IsCompleted)
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
            
            if (task.IsCompleted)
            {
                fileContents = task.Result;
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileContents);
                image.GetComponent<RawImage>().texture = tex;
                Debug.Log("Finished downloading!");
            }
        });

        // Create randomized list of question positions
        int[] numbers = { -1, -1, -1, -1 };
        for (int i = 0; i < 4; i++)
        {
            int random = Random.Range(0, 4);
            if (numbers.Contains(random)) i--;
            else numbers[i] = random;
        }
        if (gamemode == Mode.ProverbMeaning)
        {
            SetCurrentQuestion(nextProverb.meaning, nextProverb.otherMeanings);
            taskText.text = "Select the meaning";
            currentQuestion.text = nextProverb.phrase;
        }
        else
        {
            SetCurrentQuestion(nextProverb.phrase, nextProverb.otherPhrases);
            if (gamemode == Mode.MeaningProverb)
            {
                taskText.text = "Select the proverb";
                currentQuestion.text = nextProverb.meaning;
            }
            else
            {
                taskText.text = "Select the proverb";
                currentQuestion.text = nextProverb.example;
            }
        }

        // Set the question text
        questionText.text = currentQuestion.text;
    }

    /** 
     * Functionality for clicking the hint image:
     * - if the hint image is currently hidden, show it;
     * - it the hint image is currently shown, hide it.
     */
    public void HintClicked() {
        image.enabled = !image.enabled;
    }
}
