using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;

public class MCQVariations : MonoBehaviour
{
    public Question[] questions;
    private static List<Question> notAnswered;
    private Question currentQuestion;
    private Answer selectedAnswer;

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

    private void Start()
    {
        if(gamemode == Modes.ExampleSentence)
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
