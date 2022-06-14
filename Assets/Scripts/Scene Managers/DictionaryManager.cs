using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

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

    // public DictionaryManager()
    // {
    //     allProverbs = new List<ProverbsDictionary>(new []
    //     {
    //         new ProverbsDictionary("<b>horse is in house!</b>", "meaning"),
    //         new ProverbsDictionary("<b>horse was here!</b>", "meaning"),
    //         new ProverbsDictionary("<b>Don't look a gift horse in his mouth</b>", "meaning"),
    //         new ProverbsDictionary("slow and steady wins the race", "meaning2"),
    //         new ProverbsDictionary("To be the black sheep of the family", "if you are different, you are oft not accepted in a group."),
    //         new ProverbsDictionary("His bark is worst than his bite", "To make a lot of noise without reason apparently."),
    //         new ProverbsDictionary("When in Rome, do as Romans do", "When visiting a foreign land, follow the customs of those who live in it."),
    //         new ProverbsDictionary("Don’t judge a book by its cover", "You should not judge the worth or value of something by its outward appearance alone."),
    //         new ProverbsDictionary("A bird in hand is worth two in the bush", "Things we already have are more concrete than what we hope to get."),
    //         new ProverbsDictionary("All that glitters is not gold", "said about something that seems to be good on the surface, but might not be when you look at it more closely"),
    //         new ProverbsDictionary("In the land of the blind, the one-eyed man is king", "Even someone without much talent or ability is considered special by those with no talent or ability at all. ")
    //     });
    //     filteredProverbsList = new List<ProverbsDictionary>(allProverbs);
    //     wordsToFilterOn = new List<string>();
    // }
    
    // Start is called before the first frame update
    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        GetPlayerKey();
        StartCoroutine(wait());
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(1);
        UpdateDictionaryContentHolderContents();
    }

    private void GetPlayerKey()
    {
        // Goes to the 'players' database table and searches for the user
        dbReference.Child("players").OrderByChild("email").EqualTo("dantheodor123@gmail.com")
            .ValueChanged += (object sender, ValueChangedEventArgs args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            // Check to see if there is at least one result
            if (args.Snapshot != null && args.Snapshot.ChildrenCount > 0)
            {
                string playerKey = args.Snapshot.Children.First().Key;
                GetProficiencies(playerKey);
            }
        };
    }

    private void GetProficiencies(string playerKey)
    {
        // Goes to the 'proficiencies' database table and searches for {playerKey}
        dbReference.Child("proficiencies").Child(playerKey)
        .GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Task could not be completed.");
                return;
            }
            else if (task.IsCompleted)
            {
                // Take a snapshot of the database entry
                DataSnapshot snapshot = task.Result;
                // Convert the JSON back to a Proficiency object
                string json = snapshot.GetRawJsonValue();
                Proficiency playerProficiency = JsonUtility.FromJson<Proficiency>(json);
                // filter out proverbs where user has stage 1 for from playerProficiency
                List<Bucket> buckets = playerProficiency.apprentice.FindAll(b => b.stage >= 2);
                buckets.AddRange(playerProficiency.journeyman);
                buckets.AddRange(playerProficiency.expert);
                buckets.AddRange(playerProficiency.master);
                
                List<string> bucketKeys = buckets.Select(b => b.key).ToList();
                getProverbsToShow(bucketKeys);
            }
        });
    }

    private void getProverbsToShow(List<string> keysToGetProverbsFor)
    {
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
    
}
