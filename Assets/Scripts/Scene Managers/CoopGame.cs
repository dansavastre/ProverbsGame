using Firebase;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class CoopGame : SingleplayerManager
{

    public static Launcher_FIB Instance;

    public PhotonView _photon;
    // UI elements
    // [SerializeField] private List<GameObject> buttons;
    // [SerializeField] private List<TextMeshProUGUI> buttonTexts;
    [SerializeField] private Transform keywordBoard;
    [SerializeField] private Button dragDropButtonPrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI[] otherPlayerNames;

    // Variables
    private static string correctProverb;
    private string answerProverb;
    private List<string> buttonsToCreateWords;
    public string LastClickedWord;
    public static List<string> allWords;
    public static List<string> buttonIndices;
    public List<Proverb> proverbs;

    //Receive a word from another player
    [PunRPC]
    void ReceiveChat(string msg)
    {
        string[] splits = msg.Split(":");
        if (PhotonNetwork.NickName.Equals(splits[0]))
        {
            CreateButtonForReceivedKeyword(splits[1]);
        }
    }

    //Send a word to another player
    public void SendChat(string msg)
    {
        string newMessage = PhotonNetwork.NickName + ":" + msg;
        _photon.RPC("ReceiveChat", RpcTarget.Others, "a");
    }

    // //Send a message to other players signalling that you joined the game
    // public void sendMyNickName()
    // {
    //     _photon.RPC("ReceiveName", RpcTarget.Others, PhotonNetwork.NickName);
    // }
    
    // //Receive a signal that a player has loaded in to the game
    // [PunRPC]
    // public void ReceiveName(string playerName)
    // {
    //     sendMyNickName();
    //     foreach (TextMeshProUGUI name in otherPlayerNames)
    //     {
    //         if (name.text == playerName) return;
    //     }
    //     
    //     int i = 0;
    //     while (otherPlayerNames[i].text != "")
    //     {
    //         i++;
    //     }
    //
    //     otherPlayerNames[i].text = playerName;
    //     otherPlayerNames[i].transform.parent.GameObject().SetActive(true);
    // }

    //Starts the scene but waits first
    // IEnumerator startButWaitFirst()
    // {
    //     yield return new WaitForSeconds(2);
    //     sendMyNickName();
    // }

    // Start is called before the first frame update
    async void Start()
    {
        // StartCoroutine(startButWaitFirst());
        // base.Start();

        // Goes to the 'proverbs' database table and searches for the key
        // await dbReference.Child("proverbs").Child(currentKey)
        // .GetValueAsync().ContinueWith(task =>
        // {
        //     if (task.IsFaulted)
        //     {
        //         Debug.LogError("Task could not be completed.");
        //         return;
        //     }
        //     
        //     else if (task.IsCompleted)
        //     {
        //         // Take a snapshot of the database entry
        //         DataSnapshot snapshot = task.Result;
        //         // Convert the JSON back to a Proverb object
        //         string json = snapshot.GetRawJsonValue();
        //         nextProverb = JsonUtility.FromJson<Proverb>(json);
        //         Debug.Log(json);
        //     }
        // });
        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount - 1; i++)
        {
            otherPlayerNames[i].text = PhotonNetwork.PlayerListOthers[i].NickName;
            otherPlayerNames[i].transform.parent.GameObject().SetActive(true);
        }
        
        questionText.text = "Don't look a gift horse in the mouth";
        buttonsToCreateWords = new List<string>(new[] { "gift", "horse", "mouth" });

        // Set the variables
        correctProverb = "Don't look a gift horse in the mouth";
        answerProverb = correctProverb;

        foreach (string v in buttonsToCreateWords)
        {
            answerProverb = answerProverb.Replace(v, "<u>BLANK</u>");
        }

        // for(int i = 0; i < buttonTexts.Count; i++)
        // {
        //     buttonTexts[i].text = allWords[i];
        // }

        buttonIndices = new List<string>();
        allWords = new List<string>();
        for (int i = 0; i < buttonsToCreateWords.Count; i++)
        {
            //Button newButton = Instantiate(dragDropButtonPrefab, keywordBoard, false);
            //newButton.GetComponentInChildren<TextMeshProUGUI>().text = allWords[i];
            //int xPos = (i % 3 - 1) * 230;
            //int yPos = -(i / 3) * 70;
            //newButton.transform.localPosition = new Vector3(xPos, yPos);
            //newButton.name = "AnswerButton" + i;
            //newButton.GetComponent<DragDrop>().canvas = canvas;
            //newButton.GetComponent<DragDrop>().proverbText = questionText;
            //newButton.onClick.AddListener(delegate { Debug.Log("hi"); });
            CreateButton(i, buttonsToCreateWords[i]);
        }

        questionText.text = answerProverb;
        CreateButton(buttonsToCreateWords.Count, "test");
        sentMyKeywordsToOtherPlayers(new List<string>(new string[] { "ferhan1", "ferhan2", "ferhan3", "ferhan4" }));
    }


    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var wordIndex = TMP_TextUtilities.FindIntersectingWord(questionText, Input.mousePosition, null);

            if (wordIndex != -1)
            {
                LastClickedWord = questionText.textInfo.wordInfo[wordIndex].GetWord();


                //If a keyword inside of the proverb is clicked, remove that keyword from the proverb and create a button
                if (allWords.Contains(LastClickedWord))
                {
                    removeWord(LastClickedWord);
                    CreateButtonForReceivedKeyword(LastClickedWord);
                }
            }
        }
    }

    /**
     * <summary>Sends missing keywords from this player's proverb to other players and does that in an as uniform way as possible.</summary>
     * <param name="myKeywords">List of the keywords from this player's proverb to send to other players.</param>
     */
    private void sentMyKeywordsToOtherPlayers(List<string> myKeywords)
    {
        int playerCountThisRoom = PhotonNetwork.CurrentRoom.PlayerCount-1;
        int i = 0;
        foreach (string keyword in myKeywords)
        {
            var playerToSendTo = PhotonNetwork.PlayerListOthers[i % playerCountThisRoom];
            _photon.RPC("ReceiveChat", PhotonNetwork.PlayerListOthers[i % playerCountThisRoom], playerToSendTo.NickName+":"+keyword);
            i++;
        }
    }
    
    //Check if text is able to be put in the sentence
    public bool canInput(string text, string search)
    {
        int pos = text.IndexOf(search);
        if(pos < 0) 
        {
            return false;
        }
        return true;
    }

    // private void inputWord(string word)
    // {
    //     word = "<u><b>" + word + "</u></b>";
    //     answerProverb = ReplaceFirst(answerProverb, "<u>BLANK</u>", word);
    //     questionText.text = answerProverb;
    // }

    //Remove a word from the proverb
    private void removeWord(string word)
    {
        word = "<u>" + word + "</u>";
        answerProverb = questionText.text;
        answerProverb = ReplaceFirst(answerProverb, word, "<u>BLANK</u>");
        questionText.text = answerProverb;
    }

    //Function that replaces the first occurance of a string "search" inside of a string "text" with a string "replace"
    public string ReplaceFirst(string text, string search, string replace)
    {
        if (!canInput(answerProverb, search))
        {
            return text;
        }
        return text.Substring(0, text.IndexOf(search)) + replace + text.Substring(text.IndexOf(search) + search.Length);
    }

    // public void buttonPressed(Button button)
    // {
    //     Debug.Log("here");
    //     if(canInput(answerProverb, "<u>BLANK</u>"))
    //     {
    //         string buttonText = button.GetComponentInChildren<TextMeshProUGUI>().text;
    //         inputWord(buttonText);
    //         allWords.Remove(buttonText);
    //         buttonIndices[buttonIndices.IndexOf(buttonText)] = "";
    //         Destroy(button);
    //     }
    // }

    // Display the feedback after the player answers the question
    public void CheckAnswer()
    {
        string playerProverb = answerProverb.Replace("<u><b>", "").Replace("</u></b>", "");
        DisplayFeedback(playerProverb.Equals(correctProverb));
        // TODO: Disable the ability to click and check new answers
    }

    //Creates a button for a received keyword
    public void CreateButtonForReceivedKeyword(string text)
    {
        int index = 0;
        while (index < buttonIndices.Count && buttonIndices[index] != "")
        {
            index++;
        }

        CreateButton(index, text);
    }

    //Create a button with the right position and components
    private void CreateButton(int i, string text)
    {
        Button newButton = Instantiate(dragDropButtonPrefab, keywordBoard, false);
        newButton.GetComponentInChildren<TextMeshProUGUI>().text = text;
        int xPos = (i % 3 - 1) * 230;
        int yPos = -(i / 3) * 70;
        newButton.transform.localPosition = new Vector3(xPos, yPos);
        newButton.name = "AnswerButton" + i;
        newButton.GetComponent<DragDrop>().canvas = canvas;
        newButton.GetComponent<DragDrop>().proverbText = questionText;
        newButton.GetComponent<DragDrop>().startingPosition = newButton.transform.localPosition;
        // newButton.onClick.AddListener(() => buttonPressed(newButton));
        if (i >= buttonIndices.Count)
        {
            buttonIndices.Add(text);
        }
        else
        {
            buttonIndices[i] = text;
        }
        allWords.Add(text);
    }

    //Load next proverb, if there are none, send a signal that you're finished
    private void LoadNextProverb()
    {
        proverbs.RemoveAt(0);


        Proverb proverb = proverbs.First();

        // Set the variables
        correctProverb = proverb.phrase;
        answerProverb = correctProverb;

        foreach (string v in proverb.keywords)
        {
            answerProverb = answerProverb.Replace(v, "<u>BLANK</u>");
        }

        questionText.text = answerProverb;
    }
}
