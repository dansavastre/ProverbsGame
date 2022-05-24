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

public class RecognizingImageGameManager : MonoBehaviour
{
    [SerializeField] private RawImage image;
    [SerializeField] private Button answerButton0, answerButton1, answerButton2, answerButton3;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private GameObject nextQuestionButton;

    // Stores information fetched from the database
    public static Proficiency playerProficiency;
    public static Proficiency newProficiency;
    private Proverb nextProverb;
    private DatabaseReference dbReference;
    private StorageReference storageRef;
    private string currentImage;
    private string currentType;
    private string currentKey;

    private byte[] fileContents;
    private Question question;

    // Start is called before the first frame update.
    private async void Start()
    {
        // Get a reference to the storage service, using the default Firebase App
        storageRef = FirebaseStorage.DefaultInstance.GetReferenceFromUrl("gs://sp-proverb-game.appspot.com");

        dbReference  = FirebaseDatabase.DefaultInstance.RootReference;
        playerProficiency = SessionManager.playerProficiency;
        newProficiency = SessionManager.newProficiency;
        currentKey = GetNextKey();

        await dbReference.Child("proverbs").Child(currentKey)
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
                Debug.Log(json);
            }
        });

        // Example reference for retrieving an image
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
            
            else if (task.IsCompleted)
            {
                fileContents = task.Result;
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileContents);
                image.GetComponent<RawImage>().texture = tex;
                Debug.Log("Finished downloading!");
            }
        });
        
        SetCurrentQuestion();
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
            Debug.Log("Session complete.");
            return null;
        }
        return currentKey;
    }

    /**
     * Method randomly loading the next question from the question list.
     */
    private void SetCurrentQuestion()
    {
        Answer answer0 = new Answer();
        answer0.text = nextProverb.phrase;
        answer0.isCorrect = true;
        Answer answer1 = new Answer();
        answer1.text = nextProverb.otherPhrases[0];
        answer1.isCorrect = false;
        Answer answer2 = new Answer();
        answer2.text = nextProverb.otherPhrases[1];
        answer2.isCorrect = false;
        Answer answer3 = answer2;
        Answer[] answers = new[] {answer0, answer1, answer2, answer3};
        question = new Question();
        question.answers = answers;

        answerButton0.GetComponentInChildren<TextMeshProUGUI>().text = question.answers[0].text;
        answerButton1.GetComponentInChildren<TextMeshProUGUI>().text = question.answers[1].text;
        answerButton2.GetComponentInChildren<TextMeshProUGUI>().text = question.answers[2].text;
        answerButton2.GetComponentInChildren<TextMeshProUGUI>().text = question.answers[3].text;
    }

    /**
     * Method that deactivates all answer buttons.
     */
    private void DeactivateAnswerButtons()
    {
        answerButton0.interactable = false;
        answerButton1.interactable = false;
        answerButton2.interactable = false;
        answerButton3.interactable = false;
    }

    /**
     * Method that displays the feedback after the player answers the question.
     */
    public void CheckAnswer(int index)
    {
        if (question.answers[index].isCorrect)
        {
            resultText.text = "Correct!";
            UpdateProficiency();
            SessionManager.RightAnswer();
        } else 
        {
            resultText.text = "Incorrect!";
            SessionManager.WrongAnswer();
        }
        DeactivateAnswerButtons();
        nextQuestionButton.SetActive(true);
    }

    private void UpdateProficiency()
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

    /**
     * Method that loads the scene again and so loads another question.
     */
    public void NextQuestion()
    {
        // Query the db for the next question and display it to the user using the already implemented methods
        // For now we will just show a message in the console
        if (GetNextKey() == null) {
            string json = JsonUtility.ToJson(newProficiency);
            dbReference.Child("proficiencies").Child(SessionManager.PlayerKey()).SetRawJsonValueAsync(json);
            SceneManager.LoadScene("Menu");
            return;
        }
        Debug.Log("Load next question");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
