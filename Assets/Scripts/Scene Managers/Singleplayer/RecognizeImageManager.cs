using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;

public class RecognizeImageManager : SingleplayerManager
{
    /// <summary>
    /// Executes when the game is started.
    /// </summary>
    protected async override void Start()
    {
        base.Start();

        // Goes to the 'proverbs' database table and searches for the key
        await dbReference.Child("proverbs").Child(currentBucket.key)
        .GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Task (get next proverb) could not be completed.");
                return;
            }
            else if (task.IsCompleted)
            {
                // Take a snapshot of the database entry
                DataSnapshot snapshot = task.Result;
                // Convert the JSON back to a Proverb object
                string json = snapshot.GetRawJsonValue();
                nextProverb = JsonUtility.FromJson<Proverb>(json);
            }
        });

        GetImage();
        
        SetCurrentQuestion(nextProverb.phrase, nextProverb.otherPhrases);
    }
}