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

    public void Start()
    {
        nextProverb = SessionManager.proverb;
        newProficiency = SessionManager.proficiency;
        dbReference = SessionManager.dbReferenceStatic;

        // Get a reference to the storage service, using the default Firebase App
        storageRef = FirebaseStorage.DefaultInstance.GetReferenceFromUrl("gs://sp-proverb-game.appspot.com");

        // Reference for retrieving an image
        StorageReference imageRef = storageRef.Child("proverbs/" + nextProverb.image);
        Debug.Log("proverbs/" + nextProverb.image);

        const long maxAllowedSize = 1 * 1024 * 1024;
        imageRef.GetBytesAsync(maxAllowedSize).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Task (get image byte array) could not be completed.");
                return;
            }
            if (task.IsCompleted)
            {
                fileContents = task.Result;
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileContents);
                image.GetComponent<RawImage>().texture = tex;
                Debug.Log("Finished downloading!");
            }
        });

        questionText.text = nextProverb.phrase;
        
        progressBar.SetProgress((float)SessionManager.correctAnswers / (float)SessionManager.maxValue);
        
        DisplayFunFact();
    }

    private void DisplayFunFact() 
    {
        nextQuestionButton.SetActive(true);
        Debug.Log(nextProverb.funFact);
        funFactText.text = nextProverb.funFact;
    }
}
