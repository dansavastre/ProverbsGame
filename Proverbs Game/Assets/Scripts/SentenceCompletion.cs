using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private static string correctProverb = "Don't look a gifted horse in the mouth";

    private string answerProverb = correctProverb;

    List<string> keyWords = new List<string> { "horse", "mouth" };
    List<string> allWords = new List<string> { "horse", "mouth", "turtle", "Never", "Don't"};
    public string LastClickedWord;

    // Start is called before the first frame update
    void Start()
    {
        foreach (string v in keyWords)
        {
            answerProverb = answerProverb.Replace(v, "...");
        }

        for(int i = 0; i < buttonTexts.Count; i++)
        {
            buttonTexts[i].text = allWords[i];
        }

        sentence.text = answerProverb;
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
        //Debug.Log(answerProverb.Replace("<u><b>", "").Replace("</u></b>", ""));
        string playerProverb = answerProverb.Replace("<u><b>", "").Replace("</u></b>", "");
        if(playerProverb.Equals(correctProverb))
        {
            Debug.Log("Correct!");
            ResultText.text = "Correct!";
        }
        else 
        {
            Debug.Log("Incorrect!");
            ResultText.text = "Incorrect!";
        }
    }

}
