using Firebase;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class FunFactManager : SingleplayerManager
{
    [SerializeField] private TextMeshProUGUI funFactText;
    [SerializeField] private TextMeshProUGUI funFactScrollable;
    [SerializeField] private GameObject scrollBar;

    /// <summary>
    /// Executes when the game is started.
    /// </summary>
    public void Start()
    {
        nextProverb = SessionManager.proverb;
        newProficiency = SessionManager.proficiency;

        // Reset game objects
        funFactText.text = "";
        scrollBar.SetActive(false);

        GetImage();

        questionText.text = nextProverb.phrase;
        progressBar.SetProgress((float)SessionManager.correctAnswers / (float)SessionManager.maxValue);

        DisplayFunFact();
    }

    /// <summary>
    /// Sets the corresponding UI elements and enables scroll bar if necessary.
    /// </summary>
    private void DisplayFunFact() 
    {
        string funFact = nextProverb.funFact;
        nextQuestionButton.SetActive(true);
        
        if (funFact.Length > 210)
        {
            scrollBar.SetActive(true);
            funFactScrollable.text = funFact;
        }
        else funFactText.text = funFact;
    }
}