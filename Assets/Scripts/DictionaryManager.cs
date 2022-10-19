using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;

public class DictionaryManager : MonoBehaviour
{
    private static DatabaseReference dbReference = AccountManager.dbReference;

    private List<ProverbsDictionary> allProverbs;
    private List<ProverbsDictionary> filteredProverbsList;
    private HashSet<string> wordsToFilterOn;

    [SerializeField] private TextMeshProUGUI filterText;
    [SerializeField] private Transform filterHolderPanel;
    [SerializeField] private TextMeshProUGUI dictionaryContentHolder;

    [SerializeField] private Button wordButtonPrefab;

    /// <summary>
    /// Executed when the game is started.
    /// </summary>
    private void Start()
    {
        ShowProverbs();
        StartCoroutine(Wait());
    }

    /// <summary>
    /// Method for making the program wait a second.
    /// </summary>
    /// <returns>A command telling the program to wait for one second.</returns>
    // TODO: Why does the dictionary not work if we wait a second, can this be fixed?
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1);
        UpdateDictionaryContents();
    }

    /// <summary>
    /// Retrieve the proverbs to be shown in the dictionary from the database.
    /// </summary>
    private void ShowProverbs()
    {
        // Add everything from journeyman and up
        List<Bucket> buckets = SessionManager.playerProficiency.apprentice.FindAll(b => b.stage >= 2);
        buckets.AddRange(SessionManager.playerProficiency.journeyman);
        buckets.AddRange(SessionManager.playerProficiency.expert);
        buckets.AddRange(SessionManager.playerProficiency.master);
        List<string> proverbKeys = buckets.Select(b => b.key).ToList();
        
        // Only add stage 2 and 3 from apprentice
        dbReference.Child("proverbs").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Task could not be completed.");
                return;
            }
            else if (task.IsCompleted)
            {
                // Take a snapshot of the database entry
                List<DataSnapshot> proverbs = task.Result.Children.Where(d => proverbKeys.Contains(d.Key)).ToList();
                List<Proverb> proverbsFromDB = proverbs.Select(s => JsonUtility.FromJson<Proverb>(s.GetRawJsonValue())).ToList();
                allProverbs = proverbsFromDB.Select(p => new ProverbsDictionary(
                    "<b>" + p.phrase + "</b>", p.meaning)).ToList();
                filteredProverbsList = new List<ProverbsDictionary>(allProverbs);
                wordsToFilterOn = new HashSet<string>();
                UpdateDictionaryContents();
            }
        });
    }

    /// <summary>
    /// Update the contents of the dictionary according to the list of filtered proverbs.
    /// </summary>
    private void UpdateDictionaryContents()
    {
        filteredProverbsList = filteredProverbsList.OrderBy(p => p.proverb).ToList();
        dictionaryContentHolder.text = "";

        foreach (var proverbsDictionary in filteredProverbsList)
        {
            dictionaryContentHolder.text += 
                proverbsDictionary.proverb + ":" + Environment.NewLine + "  " +
                proverbsDictionary.meaning + Environment.NewLine + Environment.NewLine;
        }
    }

    /// <summary>
    /// Method that is called whenever a word button is pressed.
    /// </summary>
    /// <param name="wordOfButton">string denotingg the word that is on the button</param>
    private void WordButtonPressed(string wordOfButton)
    {
        wordsToFilterOn.Remove(wordOfButton); // remove the word from the list

        // Remove all the GameObjects that shows the filtered word (removes duplicates as well)
        foreach (var tmp in filterHolderPanel.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (tmp.text == wordOfButton)
            {
                Destroy(tmp.transform.parent.GameObject());
            }
        }
        // Add proverbs back that were initially excluded
        filteredProverbsList = new List<ProverbsDictionary>(allProverbs);
        foreach (var wordToFilterOn in wordsToFilterOn)
        {
            filteredProverbsList = filteredProverbsList.Where(s => s.proverb.ToLower().Contains(wordToFilterOn)).ToList();
        }
        UpdateDictionaryContents();
    }

    /// <summary>
    /// Method that is called once a filter has been set to the search.
    /// </summary>
    public void FilterAdded()
    {
        string filter = filterText.text.Replace("\u200B", "");
        filterText.text = "";
        if (filter.Length < 2) return;
        List<string> currentWordsToFilterOn = filter.ToLower().Trim().Split().ToList();

        foreach (string wordToFilterOn in currentWordsToFilterOn)
        {
            wordsToFilterOn.Add(wordToFilterOn);
            filteredProverbsList = filteredProverbsList.Where(s => s.proverb.ToLower().Contains(wordToFilterOn))
                .ToList();

            // Add button for word filtered on 
            Button wordButton = Instantiate(wordButtonPrefab, filterHolderPanel);
            wordButton.GetComponentInChildren<TextMeshProUGUI>().text = wordToFilterOn;
            wordButton.onClick.AddListener(() => WordButtonPressed(wordToFilterOn));
        }
        filterText.text = "";
        UpdateDictionaryContents();
    }
}