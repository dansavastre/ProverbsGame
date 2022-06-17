using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class DictionaryManager : MonoBehaviour
{
    private List<ProverbsDictionary> allProverbs;
    private List<ProverbsDictionary> filteredProverbsList;
    private HashSet<string> wordsToFilterOn;
    private DatabaseReference dbReference;

    [SerializeField] private TextMeshProUGUI filterText;
    [SerializeField] private Transform filterTextHolderPanel;
    [SerializeField] private TextMeshProUGUI dictionaryContentHolder;
    [SerializeField] private Button wordButtonPrefab;

    private static AudioSource WoodButton;
    private static string[] scenes;
    
    // Start is called before the first frame update
    void Start()
    {
        scenes = SessionManager.scenes;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        WoodButton = AccountManager.WoodButton;
        getProverbsToShow();
        StartCoroutine(wait());
    }

    IEnumerator wait()
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

            // add button for word filtered on 
            Button wordButton = Instantiate(wordButtonPrefab, filterTextHolderPanel);
            wordButton.GetComponentInChildren<TextMeshProUGUI>().text = wordToFilterOn;
            wordButton.onClick.AddListener(() => WordButtonPressed(wordToFilterOn));
        }
        filterText.text = "";
        UpdateDictionaryContentHolderContents();
    }

    private void UpdateDictionaryContentHolderContents()
    {
        filteredProverbsList = filteredProverbsList.OrderBy(p => p.proverb).ToList();
        dictionaryContentHolder.text = "";
        foreach (var proverbsDictionary in filteredProverbsList)
        {
            dictionaryContentHolder.text += proverbsDictionary.proverb + ":" + Environment.NewLine + "  " +
                                            proverbsDictionary.meaning + Environment.NewLine + Environment.NewLine;
        }
    }

    private void WordButtonPressed(string wordOfButton)
    {
        wordsToFilterOn.Remove(wordOfButton);

        foreach (var tmp in filterTextHolderPanel.GetComponentsInChildren<TextMeshProUGUI>())
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

    public void PlonkNoise()
    {
        WoodButton.Play();
    }

    // Switch to the scene corresponding to the sceneIndex
    public void SwitchScene(int sceneIndex)
    {
        SceneManager.LoadScene(scenes[sceneIndex]);
    }
}