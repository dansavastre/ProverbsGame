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
using SRandom = System.Random;
using System.Text.RegularExpressions;

public class FillBlanksManager : SingleplayerManager
{
    // UI elements
    [SerializeField] private List<Button> Buttons;

    // UI prefabs
    [SerializeField] private Button fillInTheBlanksAnswerButtonPrefab;

    // Stores information fetched from the database
    private StorageReference storageRef;
    private string currentImage; 
    private byte[] fileContents;

    // The maximum number of bytes that will be retrieved
    private long maxAllowedSize = 1 * 1024 * 1024;

    // Variables
    private string correctProverb;
    private string answerProverb;
    private List<string> allWords;
    private string LastClickedWord;

    // Start is called before the first frame update
    protected async override void Start()
    {
        base.Start();

        // Do not initially show the image
        image.enabled = false;

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

        // Set the variables
        correctProverb = nextProverb.phrase;
        answerProverb = correctProverb;

        // Add the keywords to allwords, and add some flukes
        allWords = nextProverb.keywords;
        allWords.AddRange(nextProverb.otherKeywords);

        List<string> keyWordsClone = nextProverb.keywords.Select(item => (string)item.Clone()).ToList();

        foreach (string v in keyWordsClone)
        {
            for (int i = 1; i < Regex.Matches(nextProverb.phrase, v).Count; i++)
            {
                nextProverb.keywords.Add(v);
            }
            answerProverb = Regex.Replace(answerProverb, v, "<u><alpha=#00>xxxxx</color></u>", RegexOptions.IgnoreCase);
        }

        // Shuffling list of words
        for (int i = 0; i < allWords.Count; i++)
        {
            string temp = allWords[i];
            int randomIndex = Random.Range(i, allWords.Count);
            allWords[i] = allWords[randomIndex];
            allWords[randomIndex] = temp;
        }
        CreateButtons();
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

                //If a keyword inside of the proverb is clicked, remove that keyword from the proverb and create a button
                string[] splits = questionText.text.Split(" ");

                bool isKeyword = false;

                foreach  (string word in allWords)
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

    // Create a button for each word option
    // TODO: Share method with FormSentenceManager
    private void CreateButtons()
    {
        // Get the board dimensions
        int boardWidth = (int)answerBoard.GetComponent<RectTransform>().rect.width;
        int boardHeight = (int)answerBoard.GetComponent<RectTransform>().rect.height;

        // Get the button dimensions
        int buttonWidth = (int)fillInTheBlanksAnswerButtonPrefab.GetComponent<RectTransform>().rect.width;
        int buttonHeight = (int)fillInTheBlanksAnswerButtonPrefab.GetComponent<RectTransform>().rect.height;

        for (int i = 0; i < allWords.Count; i++)
        {
            // Instantiate new button from prefab
            Button newButton = Instantiate(fillInTheBlanksAnswerButtonPrefab, answerBoard, false);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = allWords[i];
            
            // Get the starting location of the buttons
            int xStart = boardWidth / 2 - buttonWidth / 2, yStart = boardHeight / 2 - buttonHeight / 2;
            // Get the row and the column of the button in the table
            int row = i % 3 - 1, col = i / 3;
            // Determine the spacing between buttons
            int spaceLength = 35;
            // Keep track of the spacing between buttons in the table
            int widthSpacing = row * spaceLength, heightSpacing = col * spaceLength;

            // Compute the final position of the button
            int xPos = row * buttonWidth + widthSpacing;
            int yPos = yStart - col * buttonHeight - heightSpacing;
            newButton.transform.localPosition = new Vector3(xPos, yPos);

            // Add button properties
            newButton.name = "AnswerButton" + i;
            int x = i;
            newButton.onClick.AddListener(() => buttonPressed(x));

            // Use animation to show button with random delay
            StartCoroutine(DelayedAnimation(newButton));
        }
    }

    // Check if text is able to be put in the sentence
    // TODO: Share method with FormSentenceManager
    public bool canInput(string text, string search)
    {
        int pos = text.IndexOf(search);
        if(pos < 0) 
        {
            return false;
        }
        return true;
    }

    // Input a word inside of the proverb
    private void inputWord(string word)
    {
        word = "<u><b>" + word + "</u></b>";
        answerProverb = ReplaceFirst(answerProverb, "<u><alpha=#00>xxxxx</color></u>", word);
        Debug.Log(answerProverb);
        questionText.text = answerProverb;
    }

    // Remove a word from the proverb
    private void removeWord(string word, int wordIndex)
    {
        Button[] buttons = answerBoard.GetComponentsInChildren<Button>();
        for (int i = 0 ; i < buttons.Length; i++) {
            if (buttons[i].GetComponentInChildren<TextMeshProUGUI>().text.Equals(word) && buttons[i].interactable == false) {
                buttons[i].interactable = true;
                break;
            }
        }
        answerProverb = questionText.text;

        string[] splits = questionText.text.Split(" ");
        splits[wordIndex] = splits[wordIndex].Replace(word, "<u><alpha=#00>xxxxx</color></u>");
    
        answerProverb = string.Join(" ", splits);
        questionText.text = answerProverb;
    }

    // Function that replaces the first occurance of a string "search" inside of a string "text" with a string "replace"
    // TODO: Share method with FormSentenceManager
    public string ReplaceFirst(string text, string search, string replace)
    {
        if (!canInput(answerProverb, search))
        {
            return text;
        }
        return text.Substring(0, text.IndexOf(search)) + replace + text.Substring(text.IndexOf(search) + search.Length);
    }

    // Detect the press of a button
    public void buttonPressed(int index)
    {
        if (canInput(answerProverb, "<u><alpha=#00>xxxxx</color></u>")) 
        {
            inputWord(answerBoard.GetComponentsInChildren<Button>()[index].GetComponentInChildren<TextMeshProUGUI>().text);
            answerBoard.GetComponentsInChildren<Button>()[index].interactable = false;
        }
    }

    // Display the feedback after the player answers the question
    public void CheckAnswer()
    {
        string playerProverb = answerProverb.Replace("<u><b>", "").Replace("</u></b>", "");

        Debug.Log(correctProverb.ToLower().Replace(" ", ""));
        Debug.Log(playerProverb.ToLower());

        DisplayFeedback(playerProverb.ToLower().Equals(correctProverb.ToLower()));
        if (continueOverlay != null) continueOverlay.SetActive(true);
        image.enabled = true;
        // TODO: Disable the ability to click new answers
        checkButton.enabled = false;
    }

    // Plays an animation on the given button with a random delay
    // TODO: Share method
    private IEnumerator DelayedAnimation(Button newButton)
    {
        SRandom rnd = new SRandom();
        float randomWait = (float)rnd.Next(1, 9)/20;
        Debug.Log(randomWait);
        yield return new WaitForSeconds(randomWait);
        newButton.gameObject.SetActive(true);
    }

    // Functionality for clicking the hint image:
    // - if the hint image is currently hidden, show it;
    // - it the hint image is currently shown, hide it.
    // TODO: Share method
    public void HintClicked() {
        image.enabled = !image.enabled;
    }
}