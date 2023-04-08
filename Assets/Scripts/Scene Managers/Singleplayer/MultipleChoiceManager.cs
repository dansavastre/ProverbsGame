using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;
using Random = UnityEngine.Random;

public class MultipleChoiceManager : SingleplayerManager
{
    public static Mode gamemode;
    public enum Mode { ProverbMeaning, MeaningProverb, ExampleSentence }

    [SerializeField] private TextMeshProUGUI taskText;
    [SerializeField] private GameObject homeButton;
    [SerializeField] private GameObject barBackground;
    [SerializeField] private GameObject questionBoard;
    [SerializeField] private GameObject imageBoard;
    [SerializeField] private GameObject funFactButton;
    [SerializeField] private GameObject nextButton;

    [SerializeField] private Sprite otherHomeButton;
    [SerializeField] private Sprite otherBarBackground;
    [SerializeField] private Sprite otherImageBoard;
    [SerializeField] private Sprite otherFunFactButton;
    [SerializeField] private Sprite otherNextButton;

    [SerializeField] private LocalizedStringTable _localizedStringTable;
    private StringTable _currentStringTable;

    /// <summary>
    /// Executes when the game is started.
    /// </summary>
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
        
        GetImage();

        int[] numbers = RandomPositions();

        if (gamemode == Mode.ProverbMeaning)
        {
            SetCurrentQuestion(nextProverb.meaning, nextProverb.otherMeanings);
            string localTask = LocalizationSettings.StringDatabase.GetLocalizedString("UITable", "MULTI_CHOICE_1");
            taskText.text = localTask; 
            // TODO: Check if works
            Debug.Log(localTask);
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
            
            // Set the current question with phrase and other phrases
            SetCurrentQuestion(nextProverb.phrase, otherPhrases);

            string localTask = LocalizationSettings.StringDatabase.GetLocalizedString("UITable", "MULTI_CHOICE_2");
            taskText.text = localTask;
            // TODO: Check if works
            Debug.Log(localTask);
            if (gamemode == Mode.MeaningProverb) currentQuestion.text = nextProverb.meaning;
            else currentQuestion.text = nextProverb.example;
        }

        // Set the question text
        questionText.text = currentQuestion.text;
    }

    /// <summary>
    /// Set the multiple choice mode depending on proverb stage.
    /// </summary>
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
            funFactButton.GetComponent<Image>().sprite = otherFunFactButton;
            nextButton.GetComponent<Image>().sprite = otherNextButton;
        }
    }

    /// <summary>
    /// Create randomized list of question positions.
    /// </summary>
    /// <returns> Numbers denoting the randomized positions of the answers in the list</returns>
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
}