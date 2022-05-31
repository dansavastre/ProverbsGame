using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UI;

public class DictionaryManager : MonoBehaviour
{
    private readonly List<ProverbsDictionary> proverbs;
    private List<ProverbsDictionary> proverbs_editableList;
    private List<string> wordsToFilterOn;
    
    [SerializeField] private TextMeshProUGUI filterText;
    [SerializeField] private Transform filterTextHolderPanel;
    [SerializeField] private TextMeshProUGUI dictionaryContentHolder;
    [SerializeField] private Button wordButtonPrefab;

    public DictionaryManager()
    {
        proverbs = new List<ProverbsDictionary>(new []
        {
            new ProverbsDictionary("Don't look a gift hose in the mouth", "meaning"),
            new ProverbsDictionary("slow and steady wins the race", "meaning2"),
            new ProverbsDictionary("To be the black sheep of the family", "if you are different, you are oft not accepted in a group."),
            new ProverbsDictionary("His bark is worst than his bite", "To make a lot of noise without reason apparently."),
            new ProverbsDictionary("When in Rome, do as Romans do", "When visiting a foreign land, follow the customs of those who live in it."),
            new ProverbsDictionary("Don’t judge a book by its cover", "You should not judge the worth or value of something by its outward appearance alone."),
            new ProverbsDictionary("A bird in hand is worth two in the bush", "Things we already have are more concrete than what we hope to get."),
            new ProverbsDictionary("All that glitters is not gold", "said about something that seems to be good on the surface, but might not be when you look at it more closely"),
            new ProverbsDictionary("In the land of the blind, the one-eyed man is king", "Even someone without much talent or ability is considered special by those with no talent or ability at all. ")
        });
        proverbs_editableList = new List<ProverbsDictionary>(proverbs);
        wordsToFilterOn = new List<string>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateDictionaryContentHolderContents();
    }
    
    public void FilterAdded()
    {
        Debug.Log(filterText.text);
        if (!filterText.text.Contains(Environment.NewLine) && !filterText.text.Contains(" ") || filterText.text.Equals("")) return;

        string wordToFilterOn = filterText.text;
        Debug.Log("word to filter on:" + wordToFilterOn);
        wordsToFilterOn.Add(wordToFilterOn);
        proverbs_editableList = proverbs_editableList.Where(s => s.proverb.SplitWords(' ').Contains(wordToFilterOn)).ToList();
        UpdateDictionaryContentHolderContents();

        // add button for word filtered on 
        Button wordButton = Instantiate(wordButtonPrefab, filterTextHolderPanel);
            wordButton.GetComponentInChildren<TextMeshProUGUI>().text = wordToFilterOn;
        wordButton.onClick.AddListener(() => WordButtonPressed(wordsToFilterOn.Count - 1));
        
        filterText.text = "";
        
    }

    private void UpdateDictionaryContentHolderContents()
    {
        proverbs_editableList = proverbs_editableList.OrderBy(p => p.proverb).ToList();
        dictionaryContentHolder.text = "";
        foreach (var proverbsDictionary in proverbs_editableList)
        {
            dictionaryContentHolder.text += proverbsDictionary.proverb + ":" + Environment.NewLine + "  " +
                                            proverbsDictionary.meaning + Environment.NewLine + Environment.NewLine;
        }
    }

    private void WordButtonPressed(int wordIndex)
    {
        string wordOnWhichToPutProverbsBackOn = wordsToFilterOn[wordIndex];
        wordsToFilterOn.RemoveAt(wordIndex);    
        Destroy(filterTextHolderPanel.GetComponents<Button>()[wordIndex]);
        proverbs_editableList.AddRange(proverbs.Where(s => s.proverb.Contains(wordOnWhichToPutProverbsBackOn)).ToList());
        UpdateDictionaryContentHolderContents();
    }
    
}
