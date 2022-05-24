using Firebase;
using Firebase.Database;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;

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

public class MCQVariations : MonoBehaviour
{
    public Question[] questions;
    private static List<Question> notAnswered;
    private Question currentQuestion;
    private Answer selectedAnswer;
    private DatabaseReference dbReference;
    public static Proficiency playerProficiency;
    public static Proficiency newProficiency;

    [SerializeField]
    private Text factText;

    [SerializeField]
    private TextMeshProUGUI answerText1, answerText2, answerText3, answerText4;

    [SerializeField]
    private Text correctAnswerText, wrongAnswerText;

    [SerializeField]
    private float delayBetweenQuestions = 1f;

    public enum Modes { ProverbMeaning, MeaningProverb, ExampleSentence}

    public Modes gamemode;

    async void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        playerProficiency = SessionManager.playerProficiency;
        newProficiency = SessionManager.newProficiency;
        currentKey = GetNextKey();

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

        Question qst = new Question();
        Answer ans1 = new Answer();
        ans1.text = nextProverb.meaning;
        ans1.isCorrect = true;

        Answer ans2 = new Answer();
        ans2.text = "b";
        ans2.isCorrect = false;

        Answer ans3 = new Answer();
        ans3.text = "c";
        ans3.isCorrect = false;

        Answer ans4 = new Answer();
        ans4.text = "d";
        ans4.isCorrect = false;

        Answer[] answers = {ans1, ans2, ans3, ans4};

        qst.answers = answers;

        if (gamemode == Modes.ExampleSentence)
        {
            //questions = load questions
        }else if(gamemode == Modes.MeaningProverb)
        {
            //questions = load questions
        }
        else {
            //questions = load questions
        }


        // only initialize the notAnswered list at the beginning, not on every scene load
        if (notAnswered == null || notAnswered.Count == 0)
            notAnswered = questions.ToList<Question>();

        SetCurrentQuestion();
    }

    // Get the key for the next proverb in the session in chronological order
    private string GetNextKey()
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
            return null;
        }
        return currentKey;
    }

    /**
     * Method that returns a random unanswered question.
     */
    private void SetCurrentQuestion()
    {
        int randomQuestionIndex = Random.Range(0, notAnswered.Count - 1);
        currentQuestion = notAnswered[randomQuestionIndex];

        factText.text = currentQuestion.text;
        answerText1.text = currentQuestion.answers[0].text;
        answerText2.text = currentQuestion.answers[1].text;
        answerText3.text = currentQuestion.answers[2].text;
        answerText4.text = currentQuestion.answers[3].text;
    }

    IEnumerator TransitionToNextQuestion()
    {
        notAnswered.Remove(currentQuestion); // remove the question from the list

        yield return new WaitForSeconds(delayBetweenQuestions); // wait for a bit before transitioning to the next question

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // load the scene with the index of our current scene (i.e. restart)
    }

    public void UserSelected(int index)
    {
        if (currentQuestion.answers[index].isCorrect)
            Debug.Log("CORRECT!");
        else
            Debug.Log("WRONG!");

        DisplayFeedback(0);
        StartCoroutine(TransitionToNextQuestion());
    }

    /**
     * Method that displays the feedback after the player answers the question.
     */
    private void DisplayFeedback(int answerIndex)
    {
        if (currentQuestion.answers[answerIndex].isCorrect)
            correctAnswerText.text = "CORRECT!";
        else
            wrongAnswerText.text = "WRONG!";
    }
}
