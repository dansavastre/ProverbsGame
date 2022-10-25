using System.Collections.Generic;
using System.Linq;
using TMPro;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Text.RegularExpressions;

public class FillFormManager : SingleplayerManager
{
    private string correctProverb;
    private string answerProverb;
    private List<string> allWords;
    private string LastClickedWord;
    private string placeholderRegex = "<u><alpha=#00>xxxxx</color></u>";
    private enum Mode { FillBlanks, FormSentence }
    private Mode gamemode;

    [SerializeField] private Button fillInTheBlanksAnswerButtonPrefab;

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    protected async override void Start()
    {
        base.Start();

        if (currentBucket.stage == 4) gamemode = Mode.FillBlanks;
        else if (currentBucket.stage == 6) gamemode = Mode.FormSentence;
        else Debug.Log("ERROR: Mode not FillBlanks or FormSentence! Wrong scene!");

        // Do not initially show the image for fill in the blanks
        if (gamemode == Mode.FillBlanks) image.enabled = false;

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

        correctProverb = nextProverb.phrase;
        if (gamemode == Mode.FillBlanks)
        {
            answerProverb = correctProverb;

            // Add the keywords to allwords, and add some flukes
            allWords = nextProverb.keywords;
            allWords.AddRange(nextProverb.otherKeywords);

            List<string> keyWordsClone = nextProverb.keywords.Select(item => (string)item.Clone()).ToList();

            foreach (string v in keyWordsClone)
            {
                for (int i = 1; i < Regex.Matches(nextProverb.phrase, v, RegexOptions.IgnoreCase).Count; i++)
                {
                    nextProverb.keywords.Add(v);
                }
                answerProverb = Regex.Replace(answerProverb, v, placeholderRegex, RegexOptions.IgnoreCase);
            }
        }
        else if (gamemode == Mode.FormSentence)
        {
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
        answerText.text = correctProverb;
    }

    /// <summary>
    /// Executes on each frame update.
    /// </summary>
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int wordIndex = TMP_TextUtilities.FindIntersectingWord(questionText, Input.mousePosition, null);

            if (wordIndex != -1)
            {
                LastClickedWord = questionText.textInfo.wordInfo[wordIndex].GetWord();

                // If a keyword inside of the proverb is clicked, remove that keyword from the proverb and create a button
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

                if ((wordIndex > -1) && (isKeyword)) removeWord(LastClickedWord, wordIndex);
            }
        }
    }

    /// <summary>
    /// Create a button for each word option.
    /// </summary>
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

    /// <summary>
    /// Check if text is able to be put in the sentence.
    /// </summary>
    /// <param name="text">String denoting the sentence.</param>
    /// <param name="search">String denoting the text to be put into the sentence.</param>
    /// <returns>whether or not the word can be added to the sentence</returns>
    public bool canInput(string text, string search)
    {
        int pos = text.IndexOf(search);
        if (pos < 0) return false;
        return true;
    }

    /// <summary>
    /// Input a word inside of the proverb.
    /// </summary>
    /// <param name="word">The word to be added to the proverb.</param>
    private void inputWord(string word)
    {
        if (gamemode == Mode.FillBlanks)
        {
            word = "<u><b>" + word + "</u></b>";
            answerProverb = ReplaceFirst(answerProverb, placeholderRegex, word);
        }
        else if (gamemode == Mode.FormSentence)
        {
            answerProverb = answerProverb + " " + word;
            answerProverb = answerProverb.Replace("  ", " ");
            answerProverb = answerProverb.Replace("  ", " ");
        }
        questionText.text = answerProverb;
    }

    /// <summary>
    /// Remove a word from the proverb.
    /// </summary>
    /// <param name="word">The word to be removed.</param>
    /// <param name="wordIndex">The index of the word to be removed.</param>
    private void removeWord(string word, int wordIndex)
    {
        Button[] buttons = answerBoard.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++) 
        {
            // TODO: Check if these can be merged
            if (gamemode == Mode.FillBlanks)
            {
                if (buttons[i].GetComponentInChildren<TextMeshProUGUI>().text.Equals(word) && buttons[i].interactable == false) 
                {
                    buttons[i].interactable = true;
                    break;
                }
            }
            else if (gamemode == Mode.FormSentence)
            {
                if (buttons[i].GetComponentInChildren<TextMeshProUGUI>().text.Equals(word))
                {
                    buttons[i].interactable = true;
                }
            }
        }

        string[] splits = questionText.text.Split(" ");
        if (gamemode == Mode.FillBlanks) splits[wordIndex] = splits[wordIndex].Replace(word, placeholderRegex);
        else if (gamemode == Mode.FormSentence) splits[wordIndex] = "";

        answerProverb = questionText.text;
        answerProverb = string.Join(" ", splits);

        if (gamemode == Mode.FormSentence)
        {
            // TODO: Fix this crude solution to extra spaces
            answerProverb = answerProverb.Replace("  ", " ");
            answerProverb = answerProverb.Replace("  ", " ");
        }

        questionText.text = answerProverb;
    }

    /// <summary>
    /// Function that replaces the first occurrence of a string "search" inside of a string "text" with a string "replace".
    /// </summary>
    /// <param name="text">Text denoting the sentence.</param>
    /// <param name="search">The string whose first occurrence should be replaced.</param>
    /// <param name="replace">The string to replace the first occurrence with.</param>
    /// <returns>The modified sentence.</returns>
    public string ReplaceFirst(string text, string search, string replace)
    {
        if (!canInput(answerProverb, search)) return text;
        return text.Substring(0, text.IndexOf(search)) + replace + text.Substring(text.IndexOf(search) + search.Length);
    }

    /// <summary>
    /// Detect the press of a button.
    /// </summary>
    /// <param name="index">The index of the button that is checked for presses.</param>
    public void buttonPressed(int index)
    {
        if ((gamemode == Mode.FillBlanks && canInput(answerProverb, placeholderRegex)) || gamemode == Mode.FormSentence)
        {
            inputWord(answerBoard.GetComponentsInChildren<Button>()[index].GetComponentInChildren<TextMeshProUGUI>().text);
            answerBoard.GetComponentsInChildren<Button>()[index].interactable = false;
        }
    }

    /// <summary>
    /// Display the feedback after the player answers the question.
    /// </summary>
    // TODO: Make answer checking more robust
    public void CheckAnswer()
    {
        // Do string manipulation to verify that the sentences are the same or not
        if (gamemode == Mode.FillBlanks) 
        {
            string playerProverb = answerProverb.Replace("<u><b>", "").Replace("</u></b>", "");
            DisplayFeedback(playerProverb.ToLower().Equals(correctProverb.ToLower()));
            image.enabled = true;
        }
        else if (gamemode == Mode.FormSentence)
        {
            string playerProverb = answerProverb.Replace(" ", "");
            DisplayFeedback(playerProverb.ToLower().Equals(correctProverb.ToLower().Replace(" ", "")));
        }

        if (continueOverlay != null) continueOverlay.SetActive(true);
        // TODO: Disable the ability to click new answers
        checkButton.enabled = false;
    }
}