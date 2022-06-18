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

    // Stores the reference location of the image storage
    private StorageReference storageRef;
    private string currentImage;
    private byte[] fileContents;

    // The maximum number of bytes that will be retrieved
    private long maxAllowedSize = 1 * 1024 * 1024;

    public void Start()
    {
        Debug.Log("is on demand: " + SessionManager.isOnDemandBeforeAnswer);
        nextProverb = SessionManager.proverb;
        newProficiency = SessionManager.proficiency;

        // Reset gameobjects
        funFactText.text = "";
        scrollBar.SetActive(false);

        // Get a reference to the storage service, using the default Firebase App
        storageRef = FirebaseStorage.DefaultInstance.GetReferenceFromUrl("gs://sp-proverb-game.appspot.com");

        // Get the root reference location of the image storage
        StorageReference imageRef = storageRef.Child("proverbs/" + nextProverb.image);

        // TODO: Share this method, has no await
        // Load the proverb image from the storage
        imageRef.GetBytesAsync(maxAllowedSize).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Task (get image byte array) could not be completed.");
                return;
            }
            else if (task.IsCompleted)
            {
                fileContents = task.Result;
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileContents);
                image.GetComponent<RawImage>().texture = tex;
            }
        });

        questionText.text = nextProverb.phrase;

        progressBar.SetProgress((float)SessionManager.correctAnswers / (float)SessionManager.maxValue);

        DisplayFunFact();
    }

    // Sets the corresponding UI elements and enables scroll bar if necessary
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