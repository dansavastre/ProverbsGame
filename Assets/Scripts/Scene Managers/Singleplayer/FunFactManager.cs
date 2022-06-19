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
    // UI elements
    [SerializeField] private TextMeshProUGUI funFactText;
    [SerializeField] private TextMeshProUGUI funFactScrollable;
    [SerializeField] private GameObject scrollBar;

    // The maximum number of bytes that will be retrieved
    private long maxAllowedSize = 1 * 1024 * 1024;

    /// <summary>
    /// Executes when the game is started.
    /// </summary>
    public void Start()
    {
        Debug.Log("is on demand: " + SessionManager.isOnDemandBeforeAnswer);
        nextProverb = SessionManager.proverb;
        newProficiency = SessionManager.proficiency;

        // Reset gameobjects
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
        Debug.Log(nextProverb.funFact);
        
        if (funFact.Length > 210)
        {
            scrollBar.SetActive(true);
            funFactScrollable.text = funFact;
        }
        else
        {
            funFactText.text = funFact;
        }
    }
}