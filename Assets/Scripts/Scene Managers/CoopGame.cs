using Firebase;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class CoopGame : SingleplayerManager
{
    // UI elements
    [SerializeField] private List<GameObject> buttons;
    [SerializeField] private List<TextMeshProUGUI> buttonTexts;

    // Variables
    private static string correctProverb;
    private string answerProverb;
    List<string> allWords;
    public string LastClickedWord;

    // Start is called before the first frame update
    async void Start()
    {
        base.Start();

        // Goes to the 'proverbs' database table and searches for the key
        await dbReference.Child("proverbs").Child(currentKey)
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

        // Set the variables
        correctProverb = nextProverb.phrase;
        answerProverb = correctProverb;

        // Add the keywords to allwords, and add some flukes
        allWords = nextProverb.keywords;
        allWords.Add("frog");
        allWords.Add("box");
        allWords.Add("loses");
        allWords.Add("mediocre");

        foreach (string v in nextProverb.keywords)
        {
            answerProverb = answerProverb.Replace(v, "...");
        }

        for(int i = 0; i < buttonTexts.Count; i++)
        {
            buttonTexts[i].text = allWords[i];
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
        if(pos < 0) 
        {
            return false;
        }
        return true;
    }

    private void inputWord(string word)
    {
        word = "<u><b>" + word + "</u></b>";
        answerProverb = ReplaceFirst(answerProverb, "...", word);
        questionText.text = answerProverb;
    }

    private void removeWord(string word)
    {
        for(int i = 0 ; i < buttonTexts.Count; i++) {
            if(buttonTexts[i].text.Equals(word)) {
                buttons[i].SetActive(true);
            }
        }
        word = "<u><b>" + word + "</u></b>";
        answerProverb = ReplaceFirst(answerProverb, word, "...");
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
        if(canInput(answerProverb, "...")) 
        {
            inputWord(buttonTexts[index].text);
            buttons[index].SetActive(false);
        }
    }

    // Display the feedback after the player answers the question
    public void CheckAnswer()
    {
        string playerProverb = answerProverb.Replace("<u><b>", "").Replace("</u></b>", "");
        DisplayFeedback(playerProverb.Equals(correctProverb));
        // TODO: Disable the ability to click and check new answers
    }
}
