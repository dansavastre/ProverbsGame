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

public class SentenceCompletion : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI sentence;
    [SerializeField]
    private TextMeshProUGUI ResultText;
    [SerializeField]
    private List<GameObject> buttons;
    [SerializeField]
    private List<TextMeshProUGUI> buttonTexts;
    [SerializeField]
    private GameObject nextQuestionButton;

    // Stores information fetched from the database
    public static Proficiency playerProficiency;
    private Proverb nextProverb;
    private DatabaseReference dbReference;
    private string currentType;
    private string currentKey;

    private static string correctProverb;
    private string answerProverb;

    List<string> allWords;
    public string LastClickedWord;

    // Start is called before the first frame update
    async void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        playerProficiency = SessionManager.playerProficiency;
        currentKey = GetNextKey();

        // TODO: Move this to its own script folder in the future
        // This is hard because of the asynchronous calls to the database

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

        sentence.text = answerProverb;

        nextQuestionButton.SetActive(false);
    }

    // Get the key for the next proverb in the session in chronological order
    private string GetNextKey()
    {
        if (playerProficiency.apprentice.Count > 0)
        {
            currentKey = playerProficiency.apprentice.First();
            currentType = "apprentice";
        } else if (playerProficiency.journeyman.Count > 0)
        {
            currentKey = playerProficiency.journeyman.First();
            currentType = "journeyman";
        } else if (playerProficiency.expert.Count > 0)
        {
            currentKey = playerProficiency.expert.First();
            currentType = "expert";
        } else if (playerProficiency.master.Count > 0)
        {
            currentKey = playerProficiency.master.First();
            currentType = "master";
        } else 
        {
            // TODO: This causes an error, switch back to the menu instead
            Debug.Log("Session complete.");
            return null;
        }
        return currentKey;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var wordIndex = TMP_TextUtilities.FindIntersectingWord(sentence, Input.mousePosition, null);

            if (wordIndex != -1)
            {
                LastClickedWord = sentence.textInfo.wordInfo[wordIndex].GetWord();

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
        sentence.text = answerProverb;
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
        sentence.text = answerProverb;
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

    public void CheckAnswer()
    {
        string playerProverb = answerProverb.Replace("<u><b>", "").Replace("</u></b>", "");
        if(playerProverb.Equals(correctProverb))
        {
            ResultText.text = "Correct!";
            switch (currentType)
            {
                case "apprentice":
                    playerProficiency.apprentice.Remove(currentKey);
                    break;
                case "journeyman":
                    playerProficiency.journeyman.Remove(currentKey);
                    break;
                case "expert":
                    playerProficiency.expert.Remove(currentKey);
                    break;
                case "master":
                    playerProficiency.master.Remove(currentKey);
                    break;
                default:
                    Debug.Log("Invalid type.");
                    break;
            }
        }
        else 
        {
            ResultText.text = "Incorrect!";
        }
        nextQuestionButton.SetActive(true);
    }

    public void LoadQuestion() 
    {
        // Query the db for the next question and display it to the user using the already implemented methods
        // For now we will just show a message in the console
        Debug.Log("Load next question");
        SceneManager.LoadScene("FillBlankGame");
    }
}
