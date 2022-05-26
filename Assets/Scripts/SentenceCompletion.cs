using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SentenceCompletion : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI sentence;
    [SerializeField]
    private Transform keywordBoard;
    [SerializeField]
    private TextMeshProUGUI ResultText;
    [SerializeField]
    private List<GameObject> buttons;
    [SerializeField]
    private List<TextMeshProUGUI> buttonTexts;
    [SerializeField]
    private GameObject nextQuestionButton;

    [SerializeField] private Button fillInTheBlanksAnswerButtonPrefab; 

    private static string correctProverb = "Don't look a gifted horse in the mouth";

    private string answerProverb = correctProverb;

    List<string> keyWords = new List<string> { "horse", "mouth" };
    List<string> allWords = new List<string> { "horse", "mouth", "turtle", "Never", "Don't"};
    private string LastClickedWord;

    // Start is called before the first frame update
    void Start()
    {
        foreach (string v in keyWords)
        {
            answerProverb = answerProverb.Replace(v, "...");
        }

        for (int i = 0; i < allWords.Count; i++)
        {
            Button newButton = Instantiate(fillInTheBlanksAnswerButtonPrefab, keywordBoard, false);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = allWords[i];
            int xPos = (i % 3 - 1) * 230;
            int yPos = -(i / 3) * 100;
            newButton.transform.localPosition = new Vector3(xPos, yPos);
            newButton.name = "AnswerButton" + i;
            int x = i;
            newButton.onClick.AddListener(() => buttonPressed(x));
        }

        sentence.text = answerProverb;

        nextQuestionButton.SetActive(false);
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
            inputWord(keywordBoard.GetComponentsInChildren<Button>()[index].GetComponentInChildren<TextMeshProUGUI>().text);
            keywordBoard.GetComponentsInChildren<Button>()[index].enabled = false;
        }
    }

    public void CheckAnswer()
    {
        //Debug.Log(answerProverb.Replace("<u><b>", "").Replace("</u></b>", ""));
        string playerProverb = answerProverb.Replace("<u><b>", "").Replace("</u></b>", "");
        if(playerProverb.Equals(correctProverb))
        {
            ResultText.text = "Correct!";
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
