using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

// TODO: Add more comments to this file

public class DictionaryManager : MonoBehaviour
{
    // UI elements
    [SerializeField] private TextMeshProUGUI filterText;
    [SerializeField] private Transform filterHolderPanel;
    [SerializeField] private TextMeshProUGUI dictionaryContentHolder;

    // UI prefabs
    [SerializeField] private Button wordButtonPrefab;

    // Audio source for button sound
    public static AudioSource WoodButton;
    
    // Proverb information
    private List<ProverbsDictionary> allProverbs;
    private List<ProverbsDictionary> filteredProverbsList;
    private HashSet<string> wordsToFilterOn;
    private DatabaseReference dbReference;
    
    private void Start()
    {
        // Get the root reference location of the database
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Get the GameObject that contains the audio source for button sound
        WoodButton = AccountManager.WoodButton;

        getProverbsToShow();
        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1);
        UpdateDictionaryContentHolderContents();
    }

    private void getProverbsToShow()
    {
        List<Bucket> buckets = SessionManager.playerProficiency.apprentice.FindAll(b => b.stage >= 2);
        buckets.AddRange(SessionManager.playerProficiency.journeyman);
        buckets.AddRange(SessionManager.playerProficiency.expert);
        buckets.AddRange(SessionManager.playerProficiency.master);
        
        List<string> keysToGetProverbsFor = buckets.Select(b => b.key).ToList();
        
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
                List<DataSnapshot> proverbs = task.Result.Children.Where(d => keysToGetProverbsFor.Contains(d.Key)).ToList();
                List<Proverb> proverbsFromDB = proverbs.Select(s => JsonUtility.FromJson<Proverb>(s.GetRawJsonValue())).ToList();
                allProverbs = proverbsFromDB.Select(p => new ProverbsDictionary(
                    "<b>" + p.phrase + "</b>", p.meaning)).ToList();
                filteredProverbsList = new List<ProverbsDictionary>(allProverbs);
                wordsToFilterOn = new HashSet<string>();
                UpdateDictionaryContentHolderContents();
            }
        });
    }

    private void UpdateDictionaryContentHolderContents()
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

    private void WordButtonPressed(string wordOfButton)
    {
        wordsToFilterOn.Remove(wordOfButton);

        foreach (var tmp in filterHolderPanel.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (tmp.text == wordOfButton)
            {
                Destroy(tmp.transform.parent.GameObject());
            }
        }
        filteredProverbsList = new List<ProverbsDictionary>(allProverbs);
        foreach (var wordToFilterOn in wordsToFilterOn)
        {
            filteredProverbsList = filteredProverbsList.Where(s => s.proverb.ToLower().Contains(wordToFilterOn)).ToList();
        }
        UpdateDictionaryContentHolderContents();
    }

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
        UpdateDictionaryContentHolderContents();
    }

    // Plays the button clicked sound once
    // TODO: Share method
    public void PlonkNoise()
    {
        WoodButton.Play();
    }

    // Switches to another scene
    // TODO: Share method
    public void SwitchScene(int sceneIndex)
    {
        SceneManager.LoadScene(SessionManager.scenes[sceneIndex]);
    }
}