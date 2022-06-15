using Firebase;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.Networking;
using TMPro;

public class ExcelConverter : MonoBehaviour
{

    // Stores the reference location of the database
    private DatabaseReference dbReference;

    [SerializeField]
    public TextMeshProUGUI resultText;

    // void Start()
    // {
    //     parseList("We must be careful with horses.,When we receive a gift, we must show happiness, even if it is not to our liking.,We should always show satisfaction with a gift, if it is to our liking.");
    // }

    public void UploadProverbs()
    {
        // Get the root reference location of the database
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Fetch the .csv file from the Resources folder
        TextAsset proverbsCSV = Resources.Load<TextAsset>("Proverbs");
        
        // check if the file exists
        if(proverbsCSV == null)
        {
            resultText.text = "The file could not be loaded. Make sure the file name is \"proverbs.csv\" and that the file is in the Resources folder";
        }
        
        string[] data = proverbsCSV.text.Split(new char[] { '\n' });

        for(int i = 1; i < data.Length - 1; i++)
        {
            Regex regx = new Regex(',' + "(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            string[] row = regx.Split(data[i]);
            List<string> keywords = parseList(row[3].Substring(1, row[3].Length - 2));
            List<string> otherKeywords = parseList(row[4].Substring(1, row[4].Length - 2));
            List<string> otherPhrases = parseList(row[2].Substring(1, row[2].Length - 2));
            List<string> otherMeanings = parseList(row[6].Substring(1, row[6].Length - 2));
            List<string> otherExamples = parseList(row[8].Substring(1, row[8].Length - 2));

            Proverb proverb = new Proverb(  row[1].Replace("\"", ""), keywords, row[5].Replace("\"", ""), row[7].Replace("\"", ""), 
                                            row[10].Replace("\r", ""), otherPhrases, otherKeywords, otherMeanings, otherExamples, row[9].Replace("\"", ""));
            Debug.Log(JsonUtility.ToJson(proverb));

            // Add the proverb to the database
            string proverbKey = dbReference.Child("proverbs").Push().Key;
            dbReference.Child("proverbs").Child(proverbKey).SetRawJsonValueAsync(JsonUtility.ToJson(proverb));
        }
    }

    private List<string> parseList(string wordsList)
    {
        List<string> res = new List<string>();

        Regex regx = new Regex(@",(?=\S)");
        string[] words = regx.Split(wordsList);
        foreach(var word in words)
        {
            res.Add(word.Replace("\"\"", "\""));
        }
        return res;
    }
}
