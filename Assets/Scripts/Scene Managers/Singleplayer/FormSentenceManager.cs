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
using Random = UnityEngine.Random;
using SRandom = System.Random;

public class FormSentenceManager : SingleplayerManager
{
    // UI prefabs
    [SerializeField] private Button fillInTheBlanksAnswerButtonPrefab;

    // Variables
    private string correctProverb;
    private string answerProverb;
    private List<string> allWords;
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
        for (int i = 0; allWords.Count < 15 && i < nextProverb.otherKeywords.Count; i++)
        {
            allWords.Add(nextProverb.otherKeywords[i]);
        }

        Debug.Log(allWords.Count);

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
        answerText.text = correctProverb;
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

    // Create a button for each word option
    // TODO: Share method with FillBlanksManager
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
    // TODO: Share method with FillBlanksManager
    public bool canInput(string text, string search)
    {
        int pos = text.IndexOf(search);
        if (pos < 0)
        {
            return false;
        }
        return true;
    }

    // Input a word inside of the proverb
    private void inputWord(string word)
    {
        answerProverb = answerProverb + " " + word;
        answerProverb = answerProverb.Replace("  ", " ");
        // Remove triple spaces
        answerProverb = answerProverb.Replace("  ", " ");
        questionText.text = answerProverb;
    }

    // Remove a word from the proverb
    private void removeWord(string word, int wordIndex)
    {
        Button[] buttons = answerBoard.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].GetComponentInChildren<TextMeshProUGUI>().text.Equals(word))
            {
                buttons[i].interactable = true;
            }
        }

        string[] splits = questionText.text.Split(" ");
        splits[wordIndex] = "";

        answerProverb = questionText.text;
        answerProverb = string.Join(" ", splits);
        answerProverb = answerProverb.Replace("  ", " ");
        // Remove triple spaces;
        answerProverb = answerProverb.Replace("  ", " ");

        questionText.text = answerProverb;
    }

    // Function that replaces the first occurance of a string "search" inside of a string "text" with a string "replace"
    // TODO: Share method with FillBlanksManager
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
          inputWord(answerBoard.GetComponentsInChildren<Button>()[index].GetComponentInChildren<TextMeshProUGUI>().text);
          answerBoard.GetComponentsInChildren<Button>()[index].interactable = false;
    }

    // Display the feedback after the player answers the question
    public void CheckAnswer()
    {
        // Do string manipulation to verify that the sentences are the same or not
        string playerProverb = answerProverb.Replace(" ", "");

        Debug.Log(correctProverb.ToLower().Replace(" ", ""));
        Debug.Log(playerProverb.ToLower());

        DisplayFeedback(playerProverb.ToLower().Equals(correctProverb.ToLower().Replace(" ", "")));
        if (continueOverlay != null) continueOverlay.SetActive(true);
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
}