using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {
    public Question[] questions;
    private static List<Question> notAnswered;
    private Question currentQuestion;
    private Answer selectedAnswer;

    [SerializeField]
    private Text factText;

    [SerializeField]
    private Text answerText1, answerText2, answerText3, answerText4;

    private void Start() {
        // only initialize the notAnswered list at the beginning, not on every scene load
        if (notAnswered == null || notAnswered.Count == 0)
            notAnswered = questions.ToList<Question>();

        SetCurrentQuestion();
    }

    /**
     * Method that returns a random unanswered question.
     */
    private void SetCurrentQuestion() {
        int randomQuestionIndex = Random.Range(0, notAnswered.Count - 1);
        currentQuestion = notAnswered[randomQuestionIndex];

        factText.text = currentQuestion.text;
        answerText1.text = currentQuestion.answers[0].text;
        answerText2.text = currentQuestion.answers[1].text;
        answerText3.text = currentQuestion.answers[2].text;
        answerText4.text = currentQuestion.answers[3].text;

        notAnswered.RemoveAt(randomQuestionIndex); // remove the question from the list
    }

    public void UserSelectFirst() {
        if (currentQuestion.answers[0].isCorrect)
            Debug.Log("CORRECT!");
        else
            Debug.Log("WRONG!");
    }

    public void UserSelectSecond() {
        if (currentQuestion.answers[1].isCorrect)
            Debug.Log("CORRECT!");
        else
            Debug.Log("WRONG!");
    }

    public void UserSelectThird() {
        if (currentQuestion.answers[2].isCorrect)
            Debug.Log("CORRECT!");
        else
            Debug.Log("WRONG!");
    }

    public void UserSelectFourth() {
        if (currentQuestion.answers[3].isCorrect)
            Debug.Log("CORRECT!");
        else
            Debug.Log("WRONG!");
    }
}
