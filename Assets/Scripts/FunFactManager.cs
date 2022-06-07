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
    [SerializeField] private RawImage image;
    private StorageReference storageRef;
    private string currentImage;
    private byte[] fileContents;

    public async void Start()
    {
        DisplayFunFact();
    }

    private void DisplayFunFact() 
    {
        nextQuestionButton.SetActive(true);
        nextProverb = ProverbStateController.proverb;
        Debug.Log(nextProverb.funFact);
        funFactText.text = nextProverb.funFact;
    }

}
