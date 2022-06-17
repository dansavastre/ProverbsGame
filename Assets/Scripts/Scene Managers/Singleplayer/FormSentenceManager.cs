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
        allWords.AddRange(nextProverb.otherKeywords);

        // Shuffling list of words
        for (int i = 0; i < allWords.Count; i++)
        {
            string temp = allWords[i];
            int randomIndex = Random.Range(i, allWords.Count);
            allWords[i] = allWords[randomIndex];
            allWords[randomIndex] = temp;
        }

        int boardRightEdge = (int)answerBoard.GetComponent<RectTransform>().rect.width;
        int boardTopEdge = (int)answerBoard.GetComponent<RectTransform>().rect.height;

        for (int i = 0; i < allWords.Count; i++)
        {
            Button newButton = Instantiate(fillInTheBlanksAnswerButtonPrefab, keywordBoard, false);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = allWords[i];

            int buttonHeight = (int)newButton.GetComponent<RectTransform>().rect.height;
            int buttonWidth = (int)newButton.GetComponent<RectTransform>().rect.width;
            int xStart = boardRightEdge / 2 - buttonWidth / 2, yStart = boardTopEdge / 2 - buttonHeight / 2; // Get the starting location of the buttons
            int row = i % 3 - 1, col = i / 3; // Get the row and the column of the button in the table

            int spaceLength = 25;
            int widthSpacing = row * spaceLength, heightSpacing = col * spaceLength; // Keep track of the spacing between buttons in the table

            // Compute the final position of the button
            int xPos = row * buttonWidth + widthSpacing;
            int yPos = yStart - col * buttonHeight - heightSpacing;
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
            int wordIndex = TMP_TextUtilities.FindIntersectingWord(questionText, Input.mousePosition, null);

            if (wordIndex != -1)
            {
                LastClickedWord = questionText.textInfo.wordInfo[wordIndex].GetWord();

                //If a keyword inside of the proverb is clicked, remove that keyword from the proverb and create a button
                string[] splits = questionText.text.Split(" ");

                bool isKeyword = false;

                foreach (string word in allWords)
                {
                    if (splits[wordIndex].Contains(word))
                    {
                        isKeyword = true;
                        LastClickedWord = word;
                    }
                }

                if ((wordIndex > -1) && (isKeyword))
                {
                    removeWord(LastClickedWord, wordIndex);
                }
            }
        }
    }

    //Check if text is able to be put in the sentence
    public bool canInput(string text, string search)
    {
        int pos = text.IndexOf(search);
        if (pos < 0)
        {
            return false;
        }
        return true;
    }

    //Input a word inside of the proverb
    private void inputWord(string word)
    {
        answerProverb = answerProverb + " " + word;
        questionText.text = answerProverb;
    }

    //Remove a word from the proverb
    private void removeWord(string word, int wordIndex)
    {
        Button[] buttons = keywordBoard.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].GetComponentInChildren<TextMeshProUGUI>().text.Equals(word))
            {
                buttons[i].interactable = true;
            }
        }

        string[] splits = questionText.text.Split(" ");

        Debug.Log(splits.Count());

        splits[wordIndex] = "";
        answerProverb = questionText.text;
        answerProverb = string.Join(" ", splits);
        answerProverb = answerProverb.Replace("  ", " ");
        //remove triple spaces;
        answerProverb = answerProverb.Replace("  ", " ");

        questionText.text = answerProverb;
    }

    //Function that replaces the first occurance of a string "search" inside of a string "text" with a string "replace"
    public string ReplaceFirst(string text, string search, string replace)
    {
        if (!canInput(answerProverb, search))
        {
            return text;
        }
        return text.Substring(0, text.IndexOf(search)) + replace + text.Substring(text.IndexOf(search) + search.Length);
    }

    //Detect the press of a button
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

        Debug.Log(correctProverb.ToLower().Replace(" ", ""));
        Debug.Log(playerProverb.ToLower());

        DisplayFeedback(playerProverb.ToLower().Equals(correctProverb.ToLower().Replace(" ", "")));
        // TODO: Disable the ability to click new answers
        checkButton.SetActive(false);
    }
}
