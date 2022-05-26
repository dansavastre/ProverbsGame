using Firebase;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DatabaseManager : MonoBehaviour
{
    // TextMeshPro input fields
    public TMP_InputField Proverb;
    public TMP_InputField Keywords;
    public TMP_InputField Meaning;
    public TMP_InputField Example;

    // TextMeshPro text boxes
    public TMP_Text ProverbText;
    public TMP_Text KeywordsText;
    public TMP_Text MeaningText;
    public TMP_Text ExampleText;

    // Stores the reference location of the database
    private DatabaseReference dbReference;
    
    // Variable holding the key to the object
    private string proverbID = "-N1sWun7aYI7-TyYZQRz";
    
    // Start is called before the first frame update
    void Start()
    {
        // Get the root reference location of the database
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    // Parse the input fields to create a database entry
    public void CreateProverb()
    {
        // Create a new key
        string proverbKey = dbReference.Child("proverbs").Push().Key;
        // Create a new proverb
        Proverb newProverb = new Proverb(Proverb.text, ParseKeywords(Keywords.text), Meaning.text, Example.text, "", new List<string>{}, new List<string>{}, new List<string>{});
        // Convert the proverb to JSON
        string proverbJson = JsonUtility.ToJson(newProverb);
        // Store the proverb in the database
        dbReference.Child("proverbs").Child(proverbKey).SetRawJsonValueAsync(proverbJson);
        ClearFields();
    }
    
    // Parse the given string into a list of keywords
    private List<string> ParseKeywords(string kws)
    {
        string[] array = kws.Split(',');
        foreach(string s in array)
            s.Trim(' ');
        return array.ToList();
    }

    // Clear the input fields
    private void ClearFields()
    {
        Proverb.Select();
        Proverb.text = "";
        Keywords.Select();
        Keywords.text = "";
        Meaning.Select();
        Meaning.text = "";
        Example.Select();
        Example.text = "";
    }
    
    // Use the proverb ID to fetch the phrase
    public IEnumerator GetPhrase(Action<string> onCallback)
    {
        // Value of the data residing at the reference location
        var proverbData = dbReference.Child("proverbs").Child(proverbID).Child("phrase").GetValueAsync();

        // Wait until the data is completely loaded before continuing
        yield return new WaitUntil(predicate: () => proverbData.IsCompleted);

        // If there was data at location, take a snapshot of the data and invoke the callback
        if (proverbData != null)  
        {
            DataSnapshot snapshot = proverbData.Result;
            onCallback.Invoke(snapshot.Value.ToString());
        }
    }

    // See comments for GetPhrase
    public IEnumerator GetKeywords(Action<string> onCallback)
    {
        var proverbData = dbReference.Child("proverbs").Child(proverbID).Child("keywords").GetValueAsync();

        yield return new WaitUntil(predicate: () => proverbData.IsCompleted);

        if (proverbData != null)  
        {
            DataSnapshot snapshot = proverbData.Result;
            onCallback.Invoke(snapshot.Value.ToString());
        }
    }

    // See comments for GetPhrase
    public IEnumerator GetMeaning(Action<string> onCallback)
    {
        var proverbData = dbReference.Child("proverbs").Child(proverbID).Child("meaning").GetValueAsync();

        yield return new WaitUntil(predicate: () => proverbData.IsCompleted);

        if (proverbData != null)  
        {
            DataSnapshot snapshot = proverbData.Result;
            onCallback.Invoke(snapshot.Value.ToString());
        }
    }

    // See comments for GetPhrase
    public IEnumerator GetExample(Action<string> onCallback)
    {
        var proverbData = dbReference.Child("proverbs").Child(proverbID).Child("example").GetValueAsync();

        yield return new WaitUntil(predicate: () => proverbData.IsCompleted);

        if (proverbData != null)  
        {
            DataSnapshot snapshot = proverbData.Result;
            onCallback.Invoke(snapshot.Value.ToString());
        }
    }

    // Start a coroutine for each bit of data to make sure everything is loaded
    public void GetProverbInfo()
    {
        // Coroutine for retrieving the proverb
        StartCoroutine(GetPhrase((string proverb) => 
        {
            ProverbText.text = "Proverb: " + proverb;
        }));
        // Coroutine for retrieving the keywords
        StartCoroutine(GetKeywords((string keywords) => 
        {
            KeywordsText.text = "Keywords: " + keywords;
        }));
        // Coroutine for retrieving the meaning
        StartCoroutine(GetMeaning((string meaning) => 
        {
            MeaningText.text = "Meaning: " + meaning;
        }));
        // Coroutine for retrieving the example
        StartCoroutine(GetExample((string example) => 
        {
            ExampleText.text = "Example: " + example;
        }));
    }
}
