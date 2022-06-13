using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Random = System.Random;

public class CoopGame : SingleplayerManager
{

    public static Launcher_FIB Instance;

    public PhotonView _photon;
    // UI elements
    [SerializeField] private Transform keywordBoard;
    [SerializeField] private Button dragDropButtonPrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI[] otherPlayerNames;

    // Variables
    private int playersDone = 0;
    private static string correctProverb;
    private string answerProverb;
    private List<string> buttonsToCreateWords;
    public string LastClickedWord;
    public static List<string> allWords;
    public static List<string> buttonIndices;
    private List<Proverb> proverbs;
    Random random = new Random();

    // Start is called before the first frame update
    async void Start()
    {
        // base.Start();
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        int numberOfProverbsPerPlayer = 2;

        for (int i = 0; i < playerCount - 1; i++)
        {
            otherPlayerNames[i].text = PhotonNetwork.PlayerListOthers[i].NickName;
            otherPlayerNames[i].transform.parent.GameObject().SetActive(true);
        }

        // get proverbs from DB if this is master client and distribute them
        if (PhotonNetwork.IsMasterClient)
        {
            // Goes to the 'proverbs' database table and select {playerCount} random proverbs
            await dbReference.Child("proverbs")
                .GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.LogError("Task could not be completed.");
                    }
                
                    else if (task.IsCompleted)
                    {
                        // Take a snapshot of the database entry
                        DataSnapshot snapshot = task.Result;
                        // convert it to JSON
                        string json = snapshot.GetRawJsonValue();
                        // select {playerCount} random proverb indices
                        int[] randomProverbIndices = new int[playerCount * numberOfProverbsPerPlayer];
                        for (int i = 0; i < randomProverbIndices.Length; i++)
                        {
                            int nextInt = random.Next(0, Convert.ToInt32(snapshot.ChildrenCount));
                            if (!randomProverbIndices.Contains(nextInt))
                            {
                                randomProverbIndices[i] = nextInt;
                            }
                        }

                        // select proverbs from the database with the random indices selected above
                        List<DataSnapshot> allProverbs = snapshot.Children.ToList();
                        List<Proverb> proverbsSelected = new List<Proverb>();
                        foreach (int i in randomProverbIndices)
                        {
                            proverbsSelected.Add(JsonUtility.FromJson<Proverb>(allProverbs[i].GetRawJsonValue()));
                        }
                        
                        // send proverbs to each player
                        foreach (var player in PhotonNetwork.PlayerList)
                        {
                            _photon.RPC("SetProverbs", player, 
                                proverbsSelected.GetRange(0, numberOfProverbsPerPlayer));
                            proverbsSelected.RemoveRange(0, numberOfProverbsPerPlayer);
                        }
                        Debug.Log(proverbsSelected[0].phrase);
                    }
                });
        }
    }


    /**
     * <summary>Gets list of proverbs, saves this in corresponding attribute, merges all keywords, and distributes them</summary>
     */
    [PunRPC]
    private void SetProverbs(List<Proverb> proverbs)
    {
        this.proverbs = proverbs;
        // merge all keywords
        List<string> allKeywords = new List<string>();
        foreach (Proverb proverb in this.proverbs)
        {
            allKeywords.AddRange(proverb.keywords);
            // allKeywords.AddRange(proverbs.otherKeyWords);    //TODO this is also needed
        }
        
        //Shuffling list of words
        for (int i = 0; i < allKeywords.Count; i++)
        {
            string temp = allKeywords[i];
            int randomIndex = random.Next(i, allKeywords.Count);
            allKeywords[i] = allKeywords[randomIndex];
            allKeywords[randomIndex] = temp;
        }

        // Distribute keywords between players
        SentMyKeywordsToAllPlayers(allKeywords);

        // show next proverb
        for (int i = 0; i < allKeywords.Count; i++)
        {
            CreateButton(i, allKeywords[i]);
        }
        LoadNextProverb();
    }

    private void LoadNextProverb()
    {
        if (proverbs.Count == 0)
        {
            _photon.RPC("PlayerDone", RpcTarget.MasterClient);
        }
        Proverb proverb = proverbs.First();
        proverbs.RemoveAt(0);
        buttonsToCreateWords = proverb.keywords;
        correctProverb = proverb.phrase;
        answerProverb = correctProverb;
        
        foreach (string v in buttonsToCreateWords)
        {
            answerProverb = answerProverb.Replace(v, "<u>BLANK</u>");
        }
        
        buttonIndices = new List<string>();
        allWords = new List<string>();

        questionText.text = answerProverb;
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
                    RemoveWord(LastClickedWord);
                    CreateButtonForReceivedKeyword(LastClickedWord);
                }
            }
        }
    }
    
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
        _photon.RPC("ReceiveChat", RpcTarget.Others, newMessage);
    }

    /**
     * <summary>Sends missing keywords from this player's proverb to other players and does that in an as uniform way as possible.</summary>
     * <param name="myKeywords">List of the keywords from this player's proverb to send to other players.</param>
     */
    private void SentMyKeywordsToAllPlayers(List<string> myKeywords)
    {
        int playerCountThisRoom = PhotonNetwork.CurrentRoom.PlayerCount;
        int i = 0;
        foreach (string keyword in myKeywords)
        {
            var playerToSendTo = PhotonNetwork.PlayerList[i % playerCountThisRoom];
            _photon.RPC("ReceiveChat", playerToSendTo, playerToSendTo.NickName+":"+keyword);
            i++;
        }
    }
    
    //Check if text is able to be put in the sentence
    public bool CanInput(string text, string search)
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
    private void RemoveWord(string word)
    {
        word = "<u>" + word + "</u>";
        answerProverb = questionText.text;
        answerProverb = ReplaceFirst(answerProverb, word, "<u>BLANK</u>");
        questionText.text = answerProverb;
    }

    //Function that replaces the first occurance of a string "search" inside of a string "text" with a string "replace"
    public string ReplaceFirst(string text, string search, string replace)
    {
        if (!CanInput(answerProverb, search))
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
        answerProverb = questionText.text;
        string playerProverb = answerProverb.Replace("<u>", "").Replace("</u>", "");
        bool correct = playerProverb.Equals(correctProverb);

        if (correct)
        {
            LoadNextProverb();
        }
        else
        {
            StartCoroutine(DisplayFeedbackMulti());
        }
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

    [PunRPC]
    void PlayerDone()
    {
        int amountOfPlayers = PhotonNetwork.PlayerListOthers.Count() + 1;

        if (amountOfPlayers == playersDone)
        {
            //Everyone's done
            Debug.Log("Everyone's done");
        }
    }

    IEnumerator DisplayFeedbackMulti()
    {
        resultText.text = "Incorrect!";
        yield return new WaitForSeconds(3);
        resultText.text = "";
    }
}
