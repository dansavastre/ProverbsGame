using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SentenceCompletion : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI sentence;

    private static string correctProverb = "Don't look a gifted horse in the mouth";

    private string answerProverb = correctProverb;

    List<string> keyWords = new List<string> { "horse", "mouth" };

// Start is called before the first frame update
void Start()
    {
        foreach (string v in keyWords)
        {
            answerProverb = answerProverb.Replace(v, "...");
        }

        sentence.text = answerProverb;
    }


}
