using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class DictionaryManager : MonoBehaviour
{
    private readonly List<ProverbsDictionary> allProverbs;
    private List<ProverbsDictionary> filteredProverbsList;
    private List<string> wordsToFilterOn;
    
    [SerializeField] private TextMeshProUGUI filterText;
    [SerializeField] private Transform filterTextHolderPanel;
    [SerializeField] private TextMeshProUGUI dictionaryContentHolder;
    [SerializeField] private Button wordButtonPrefab;

    public DictionaryManager()
    {
        allProverbs = new List<ProverbsDictionary>(new []
        {
            new ProverbsDictionary("<b>Don't look a gift horse in his mouth</b>", "meaning"),
            new ProverbsDictionary("slow and steady wins the race", "meaning2"),
            new ProverbsDictionary("To be the black sheep of the family", "if you are different, you are oft not accepted in a group."),
            new ProverbsDictionary("His bark is worst than his bite", "To make a lot of noise without reason apparently."),
            new ProverbsDictionary("When in Rome, do as Romans do", "When visiting a foreign land, follow the customs of those who live in it."),
            new ProverbsDictionary("Don’t judge a book by its cover", "You should not judge the worth or value of something by its outward appearance alone."),
            new ProverbsDictionary("A bird in hand is worth two in the bush", "Things we already have are more concrete than what we hope to get."),
            new ProverbsDictionary("All that glitters is not gold", "said about something that seems to be good on the surface, but might not be when you look at it more closely"),
            new ProverbsDictionary("In the land of the blind, the one-eyed man is king", "Even someone without much talent or ability is considered special by those with no talent or ability at all. ")
        });
        filteredProverbsList = new List<ProverbsDictionary>(allProverbs);
        wordsToFilterOn = new List<string>();

        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateDictionaryContentHolderContents();
        UpdateDictionaryContentHolderContents();
    }
    
    public void FilterAdded()
    {
        string filter = filterText.text.Replace("\u200B", "");
        if (filter.Length < 2) return;
        List<string> currentWordsToFilterOn = filter.ToLower().Trim().Split().ToList();
        

        foreach (string wordToFilterOn in currentWordsToFilterOn)
        {
            wordsToFilterOn.Add(wordToFilterOn);
            filteredProverbsList = filteredProverbsList.Where(s => s.proverb.ToLower().Split().Contains(wordToFilterOn))
                .ToList();

            // add button for word filtered on 
            Button wordButton = Instantiate(wordButtonPrefab, filterTextHolderPanel);
            wordButton.GetComponentInChildren<TextMeshProUGUI>().text = wordToFilterOn;
            int x = wordsToFilterOn.Count - 1;
            wordButton.onClick.AddListener(() => WordButtonPressed(x));
        }
        UpdateDictionaryContentHolderContents();
        filterText.text = "";
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

    private void WordButtonPressed(int wordIndex)
    {
        wordsToFilterOn.RemoveAt(wordIndex);
        Destroy(filterTextHolderPanel.GetChild(wordIndex).GameObject());
        
        filteredProverbsList = new List<ProverbsDictionary>(allProverbs);
        foreach (var wordToFilterOn in wordsToFilterOn)
        {
            filteredProverbsList = filteredProverbsList.Where(s => s.proverb.ToLower().Contains(wordToFilterOn)).ToList();
        }
        UpdateDictionaryContentHolderContents();
    }
    
}
