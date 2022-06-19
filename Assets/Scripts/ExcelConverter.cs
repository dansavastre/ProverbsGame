using Firebase;
using Firebase.Database;
using Firebase.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    public TextMeshProUGUI instructionText;
    private string path;

    /// <summary>
    /// Executes when an instance of this class is initialized.
    /// </summary>
    void Start()
    {
        path = Application.persistentDataPath + "/proverbs.csv";
        instructionText.text += path;

        if(File.Exists(path))
        {
            byte[] m_bytes = File.ReadAllBytes(path);
            string s = System.Text.Encoding.UTF8.GetString(m_bytes);
            Debug.Log(s);
        }
    }

    /// <summary>
    /// Method for uploading the converted proverbs to the database.
    /// </summary>
    public void UploadProverbs()
    {
        // Get the root reference location of the database
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Remove existing proverbs from the database
        dbReference.Child("proverbs").SetRawJsonValueAsync(JsonUtility.ToJson(string.Empty));

        // Fetch the .csv file from the Resources folder
        TextAsset proverbsCSV = Resources.Load<TextAsset>("Proverbs");
        
        // check if the file exists
        if(File.Exists(path))
        {
            byte[] m_bytes = File.ReadAllBytes(path);
            string s = System.Text.Encoding.UTF8.GetString(m_bytes);
            Debug.Log(s);
        }
        
        string[] data = proverbsCSV.text.Split(new char[] { '\n' });

        // for each data entry in the list
        for(int i = 1; i < data.Length - 1; i++)
        {
            Regex regx = new Regex(',' + "(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            string[] row = regx.Split(data[i]); // split the entry into attributes
            // parse the attributes from the JSON object
            List<string> keywords = parseList(row[3].Substring(1, row[3].Length - 2));
            List<string> otherKeywords = parseList(row[4].Substring(1, row[4].Length - 2));
            List<string> otherPhrases = parseList(row[2].Substring(1, row[2].Length - 2));
            List<string> otherMeanings = parseList(row[6].Substring(1, row[6].Length - 2));
            List<string> otherExamples = parseList(row[8].Substring(1, row[8].Length - 2));
            string example = row[7];
            if(example[0] == '"')
            {
                example = example.Substring(1, example.Length - 2).Replace("\"\"", "\"");
            }

            // create the Proverb object from the parsed attributes
            Proverb proverb = new Proverb(  row[1].Replace("\"", ""), keywords, row[5].Replace("\"", ""), example, 
                                            row[10].Replace("\r", ""), otherPhrases, otherKeywords, otherMeanings, otherExamples, row[9].Replace("\"", ""));
            Debug.Log(JsonUtility.ToJson(proverb));

            // Add the proverb to the database
            string proverbKey = dbReference.Child("proverbs").Push().Key;
            dbReference.Child("proverbs").Child(proverbKey).SetRawJsonValueAsync(JsonUtility.ToJson(proverb));
        }
    }

    /// <summary>
    /// Method for parsing the list of words in the JSON pbject.
    /// </summary>
    /// <param name="wordsList">string denoting the JSON object to be parsed</param>
    /// <returns>a list of strings denoting the parsed words</returns>
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
