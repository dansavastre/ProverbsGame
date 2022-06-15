using Firebase;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class ExcelConverter : MonoBehaviour
{

    // Stores the reference location of the database
    private DatabaseReference dbReference;

    // Start is called before the first frame update
    void Start()
    {
        // Get the root reference location of the database
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Fetch the .csv file from the Resources folder
        TextAsset proverbsCSV = Resources.Load<TextAsset>("Proverbs");
        string[] data = proverbsCSV.text.Split(new char[] { '\n' });

        for(int i = 1; i < 3/*data.Length - 1*/; i++)
        {
            Regex regx = new Regex(',' + "(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))"); 
            string[] row = regx.Split(data[i]);

            List<string> keywords = parseList(row[3].Substring(1, row[3].Length - 2));
            List<string> otherKeywords = parseList(row[4].Substring(1, row[4].Length - 2));
            List<string> otherPhrases = parseList(row[2].Substring(1, row[2].Length - 2));
            List<string> otherMeanings = parseList(row[6].Substring(1, row[6].Length - 2));
            List<string> otherExamples = parseList(row[8].Substring(1, row[8].Length - 2));

            Proverb proverb = new Proverb(row[1], keywords, row[5], row[7], row[10], otherPhrases, otherMeanings, otherExamples);
            Debug.Log(JsonUtility.ToJson(proverb));

            // Add the proverb to the database
            string proverbKey = dbReference.Child("proverbs").Push().Key;
            dbReference.Child("proverbs").Child(proverbKey).SetRawJsonValueAsync(JsonUtility.ToJson(proverb));
        }
    }

    private List<string> parseList(string wordsList)
    {
        List<string> res = new List<string>();
        string[] words = wordsList.Split(new char[] { ',' });
        foreach(var word in words)
        {
            res.Add(word);
            //Debug.Log(str);
        }
        return res;
    }
}
