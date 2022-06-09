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

public class FormSentenceManager : SingleplayerManager
{
    [SerializeField] private RawImage image;
    [SerializeField] private Transform keywordBoard;
    [SerializeField] private List<Button> Buttons;
    [SerializeField] private Button fillInTheBlanksAnswerButtonPrefab;

    // Stores information fetched from the database
    private StorageReference storageRef;
    private string currentImage;
    private byte[] fileContents;

    // Variables
    private static string correctProverb;
    private string answerProverb;
    List<string> allWords;
    private string LastClickedWord;

    // Start is called before the first frame update
    protected async override void Start()
    {
        base.Start();

        image.enabled = false;

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
            
            if (task.IsCompleted)
            {
                fileContents = task.Result;
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileContents);
                image.GetComponent<RawImage>().texture = tex;
                Debug.Log("Finished downloading!");
            }
        });

        // Set the variables
        correctProverb = nextProverb.phrase;
        answerProverb = "";

        string[] splittedStringArray = correctProverb.Split(' ');
        allWords = new List<string>();
        foreach (string stringInArray in splittedStringArray)
        {
            allWords.Add(stringInArray.ToLower());
        }

        // Add the keywords to allwords, and add some flukes
        allWords.Add("frog");
        allWords.Add("box");
        allWords.Add("loses");
        allWords.Add("mediocre");

        // Shuffling list of words
        for (int i = 0; i < allWords.Count; i++)
        {
            string temp = allWords[i];
            int randomIndex = Random.Range(i, allWords.Count);
            allWords[i] = allWords[randomIndex];
            allWords[randomIndex] = temp;
        }

        for (int i = 0; i < allWords.Count; i++)
        {
            Button newButton = Instantiate(fillInTheBlanksAnswerButtonPrefab, keywordBoard, false);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = allWords[i];
            Debug.Log(allWords[i]);
            int xPos = (i % 3 - 1) * 230;
            int yPos = -(i / 3) * 100;
            newButton.transform.localPosition = new Vector3(xPos, yPos);
            newButton.name = "AnswerButton" + i;
            int x = i;
            newButton.onClick.AddListener(() => buttonPressed(x));
        }

        questionText.text = answerProverb;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var wordIndex = TMP_TextUtilities.FindIntersectingWord(questionText, Input.mousePosition, null);

            if (wordIndex != -1)
            {
                LastClickedWord = questionText.textInfo.wordInfo[wordIndex].GetWord();
                Debug.Log(LastClickedWord);

                if (allWords.Contains(LastClickedWord))
                {
                    removeWord(LastClickedWord);
                }
            }
        }
    }

    public bool canInput(string text, string search)
    {
        int pos = text.IndexOf(search);
        if (pos < 0)
        {
            return false;
        }
        return true;
    }

    private void inputWord(string word)
    {
        answerProverb = answerProverb + " " + word;
        questionText.text = answerProverb;
    }

    private void removeWord(string word)
    {
        Button[] buttons = keywordBoard.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].GetComponentInChildren<TextMeshProUGUI>().text.Equals(word))
            {
                buttons[i].interactable = true;
            }
        }
        answerProverb = ReplaceFirst(answerProverb, word, "");
        questionText.text = answerProverb;
    }

    public string ReplaceFirst(string text, string search, string replace)
    {
        if (!canInput(answerProverb, search))
        {
            return text;
        }
        return text.Substring(0, text.IndexOf(search)) + replace + text.Substring(text.IndexOf(search) + search.Length);
    }

    public void buttonPressed(int index)
    {
          inputWord(keywordBoard.GetComponentsInChildren<Button>()[index].GetComponentInChildren<TextMeshProUGUI>().text);
          keywordBoard.GetComponentsInChildren<Button>()[index].interactable = false;
    }

    // Display the feedback after the player answers the question
    public void CheckAnswer()
    {
        // Do string manipulation to verify that the sentences are the same or not
        string playerProverb = answerProverb.Replace(" ", "");

        DisplayFeedback(playerProverb.ToLower().Equals(correctProverb.ToLower().Replace(" ", "")));
        // TODO: Disable the ability to click new answers
        checkButton.SetActive(false);
    }

    // Load the image when a hint is asked for
    public void GetHint()
    {
        image.enabled = true;
    }
}
