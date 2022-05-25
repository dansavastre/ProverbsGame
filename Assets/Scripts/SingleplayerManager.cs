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

public class SingleplayerManager : MonoBehaviour
{
    // UI elements
    [SerializeField] protected TextMeshProUGUI questionText;
    [SerializeField] protected TextMeshProUGUI resultText;
    [SerializeField] protected GameObject nextQuestionButton;

    // Stores information fetched from the database
    public static Proficiency playerProficiency;
    public static Proficiency newProficiency;
    protected DatabaseReference dbReference;
    protected Proverb nextProverb;
    protected string currentType;
    protected string currentKey;

    // Variables
    protected Question currentQuestion;
    
    // Start is called before the first frame update
    protected void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        playerProficiency = SessionManager.playerProficiency;
        newProficiency = SessionManager.newProficiency;
        
        GetNextKey();
        nextQuestionButton.SetActive(false);
    }

    // Display the feedback after the player answers the question
    protected void DisplayFeedback(bool correct)
    {
        if (correct) 
        {
            resultText.text = "Correct!";
            UpdateProficiency();
            SessionManager.RightAnswer();
        }
        else 
        {
            resultText.text = "Incorrect!";
            SessionManager.WrongAnswer();
        }
        nextQuestionButton.SetActive(true);
    }

    // Load the next question
    public void LoadQuestion() 
    {
        Debug.Log("Load next question.");
        GetNextKey();
        switch (currentType)
        {
            case "apprentice":
                SceneManager.LoadScene("RecognizeImage");
                break;
            case "journeyman":
                SceneManager.LoadScene("MultipleChoice");
                break;
            case "expert":
                SceneManager.LoadScene("FillBlanks");
                break;
            case "master":
                SceneManager.LoadScene("MultipleChoice");
                break;
            default:
                string json = JsonUtility.ToJson(newProficiency);
                dbReference.Child("proficiencies").Child(SessionManager.PlayerKey()).SetRawJsonValueAsync(json);
                SceneManager.LoadScene("Menu");
                break;
        }
    }

    // Get the key for the next proverb in the session in chronological order
    protected void GetNextKey()
    {
        if (playerProficiency.apprentice.Count > 0)
        {
            currentKey = playerProficiency.apprentice.First();
            currentType = "apprentice";
        }
        else if (playerProficiency.journeyman.Count > 0)
        {
            currentKey = playerProficiency.journeyman.First();
            currentType = "journeyman";
        }
        else if (playerProficiency.expert.Count > 0)
        {
            currentKey = playerProficiency.expert.First();
            currentType = "expert";
        }
        else if (playerProficiency.master.Count > 0)
        {
            currentKey = playerProficiency.master.First();
            currentType = "master";
        }
        else
        {
            Debug.Log("Session complete.");
            currentKey = null;
            currentType = "none";
        }
    }

    // Update the player proficiency into a new object
    protected void UpdateProficiency()
    {
        switch (currentType)
        {
            case "apprentice":
                playerProficiency.apprentice.Remove(currentKey);
                if (SessionManager.wrongAnswers == 0)
                {
                    newProficiency.journeyman.Add(currentKey);
                    Debug.Log(currentKey + " moved to journeyman!");
                } else 
                {
                    newProficiency.apprentice.Add(currentKey);
                    Debug.Log(currentKey + " stayed in apprentice...");
                }
                break;
            case "journeyman":
                playerProficiency.journeyman.Remove(currentKey);
                if (SessionManager.wrongAnswers == 0)
                {
                    newProficiency.expert.Add(currentKey);
                    Debug.Log(currentKey + " moved to expert!");
                } else 
                {
                    newProficiency.apprentice.Add(currentKey);
                    Debug.Log(currentKey + " moved to apprentice...");
                }
                break;
            case "expert":
                playerProficiency.expert.Remove(currentKey);
                if (SessionManager.wrongAnswers == 0)
                {
                    newProficiency.master.Add(currentKey);
                    Debug.Log(currentKey + " moved to master!");
                } else 
                {
                    newProficiency.journeyman.Add(currentKey);
                    Debug.Log(currentKey + " moved to journeyman...");
                }
                break;
            case "master":
                playerProficiency.master.Remove(currentKey);
                if (SessionManager.wrongAnswers == 0)
                {
                    newProficiency.master.Add(currentKey);
                    Debug.Log(currentKey + " stayed in master!");
                } else 
                {
                    newProficiency.expert.Add(currentKey);
                    Debug.Log(currentKey + " moved to expert...");
                }
                break;
            default:
                Debug.Log("Invalid type.");
                break;
        }
    }
}
