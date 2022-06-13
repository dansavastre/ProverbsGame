using Firebase;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class CoopGame : SingleplayerManager
{
    // UI elements
    // [SerializeField] private List<GameObject> buttons;
    // [SerializeField] private List<TextMeshProUGUI> buttonTexts;
    [SerializeField] private Transform keywordBoard;
    [SerializeField] private Button dragDropButtonPrefab;
    [SerializeField] private Canvas canvas;

    // Variables
    private static string correctProverb;
    private string answerProverb;
    private List<string> buttonsToCreateWords;
    public string LastClickedWord;
    public static List<string> allWords;
    public static List<string> buttonIndices;

    // Start is called before the first frame update
    async void Start()
    {
        // base.Start();

        // Goes to the 'proverbs' database table and searches for the key
        // await dbReference.Child("proverbs").Child(currentKey)
        // .GetValueAsync().ContinueWith(task =>
        // {
        //     if (task.IsFaulted)
        //     {
        //         Debug.LogError("Task could not be completed.");
        //         return;
        //     }
        //     
        //     else if (task.IsCompleted)
        //     {
        //         // Take a snapshot of the database entry
        //         DataSnapshot snapshot = task.Result;
        //         // Convert the JSON back to a Proverb object
        //         string json = snapshot.GetRawJsonValue();
        //         nextProverb = JsonUtility.FromJson<Proverb>(json);
        //         Debug.Log(json);
        //     }
        // });
        questionText.text = "Don't look a gift horse in the mouth";
        buttonsToCreateWords = new List<string>(new[] { "gift", "horse", "mouth" });

        // Set the variables
        correctProverb = "Don't look a gift horse in the mouth";
        answerProverb = correctProverb;

        // Add the keywords to allwords, and add some flukes
        // allWords = nextProverb.keywords;
        buttonsToCreateWords.Add("frog");
        buttonsToCreateWords.Add("box");
        buttonsToCreateWords.Add("loses");
        buttonsToCreateWords.Add("mediocre");

        foreach (string v in buttonsToCreateWords)
        {
            answerProverb = answerProverb.Replace(v, "<u>BLANK</u>");
        }

        // for(int i = 0; i < buttonTexts.Count; i++)
        // {
        //     buttonTexts[i].text = allWords[i];
        // }

        buttonIndices = new List<string>();
        allWords = new List<string>();
        for (int i = 0; i < buttonsToCreateWords.Count; i++)
        {
            //Button newButton = Instantiate(dragDropButtonPrefab, keywordBoard, false);
            //newButton.GetComponentInChildren<TextMeshProUGUI>().text = allWords[i];
            //int xPos = (i % 3 - 1) * 230;
            //int yPos = -(i / 3) * 70;
            //newButton.transform.localPosition = new Vector3(xPos, yPos);
            //newButton.name = "AnswerButton" + i;
            //newButton.GetComponent<DragDrop>().canvas = canvas;
            //newButton.GetComponent<DragDrop>().proverbText = questionText;
            //newButton.onClick.AddListener(delegate { Debug.Log("hi"); });
            CreateButton(i, buttonsToCreateWords[i]);
        }

        questionText.text = answerProverb;
        CreateButton(buttonsToCreateWords.Count, "test");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var wordIndex = TMP_TextUtilities.FindIntersectingWord(questionText, Input.mousePosition, null);

            if (wordIndex != -1)
            {
                LastClickedWord = questionText.textInfo.wordInfo[wordIndex].GetWord();

                if (allWords.Contains(LastClickedWord))
                {
                    removeWord(LastClickedWord);
                    CreateButtonForReceivedKeyword(LastClickedWord);
                }
            }
        }
    }

    public bool canInput(string text, string search)
    {
        int pos = text.IndexOf(search);
        if(pos < 0) 
        {
            return false;
        }
        return true;
    }

    // private void inputWord(string word)
    // {
    //     word = "<u><b>" + word + "</u></b>";
    //     answerProverb = ReplaceFirst(answerProverb, "<u>BLANK</u>", word);
    //     questionText.text = answerProverb;
    // }

    private void removeWord(string word)
    {
        word = "<u>" + word + "</u>";
        answerProverb = questionText.text;
        answerProverb = ReplaceFirst(answerProverb, word, "<u>BLANK</u>");
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

    // public void buttonPressed(Button button)
    // {
    //     Debug.Log("here");
    //     if(canInput(answerProverb, "<u>BLANK</u>"))
    //     {
    //         string buttonText = button.GetComponentInChildren<TextMeshProUGUI>().text;
    //         inputWord(buttonText);
    //         allWords.Remove(buttonText);
    //         buttonIndices[buttonIndices.IndexOf(buttonText)] = "";
    //         Destroy(button);
    //     }
    // }

    // Display the feedback after the player answers the question
    public void CheckAnswer()
    {
        string playerProverb = answerProverb.Replace("<u><b>", "").Replace("</u></b>", "");
        DisplayFeedback(playerProverb.Equals(correctProverb));
        // TODO: Disable the ability to click and check new answers
    }

    public void CreateButtonForReceivedKeyword(string text)
    {
        int index = 0;
        while (index < buttonIndices.Count && buttonIndices[index] != "")
        {
            index++;
        }

        CreateButton(index, text);
    }

    private void CreateButton(int i, string text)
    {
        Button newButton = Instantiate(dragDropButtonPrefab, keywordBoard, false);
        newButton.GetComponentInChildren<TextMeshProUGUI>().text = text;
        int xPos = (i % 3 - 1) * 230;
        int yPos = -(i / 3) * 70;
        newButton.transform.localPosition = new Vector3(xPos, yPos);
        newButton.name = "AnswerButton" + i;
        newButton.GetComponent<DragDrop>().canvas = canvas;
        newButton.GetComponent<DragDrop>().proverbText = questionText;
        newButton.GetComponent<DragDrop>().startingPosition = newButton.transform.localPosition;
        // newButton.onClick.AddListener(() => buttonPressed(newButton));
        if (i >= buttonIndices.Count)
        {
            buttonIndices.Add(text);
        }
        else
        {
            buttonIndices[i] = text;
        }
        allWords.Add(text);
    }
}
