using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniJSON;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Random = System.Random;
using UnityEngine.SceneManagement;

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
    readonly Random random = new Random();

    /**
     * Called before the first frame update
     * <summary>
     * - Creates player buttons;
     * - If masterClient: gets proverbs from the DB, selects {numberOfProverbsPerPlayer} proverbs per player and randomly
     * distributes them between players.
     * </summary>
     */
    async void Start()
    {
        // initializations
        buttonIndices = new List<string>();
        allWords = new List<string>();
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        proverbs = new List<Proverb>();
        
        // variables needed here
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        int numberOfProverbsPerPlayer = 2;
        int[] randomProverbIndices = {};
        List<DataSnapshot> allProverbs = new List<DataSnapshot>();

        // set player buttons active and add their names
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
                        return;
                    }
                
                    else if (task.IsCompleted)
                    {
                        // Take a snapshot of the database entry
                        DataSnapshot snapshot = task.Result;
                        // select {playerCount} random proverb indices
                        randomProverbIndices = new int[playerCount * numberOfProverbsPerPlayer];
                        for (int i = 0; i < randomProverbIndices.Length; i++)
                        {
                            int nextInt = random.Next(0, Convert.ToInt32(snapshot.ChildrenCount));
                            if (!randomProverbIndices.Contains(nextInt))
                            {
                                randomProverbIndices[i] = nextInt;
                            }
                            else
                            {
                                i--;
                            }
                        }
                        
                        allProverbs = snapshot.Children.ToList();
                    }
                });
            // select proverbs from the database with the random indices selected above
            List<Proverb> proverbsSelected = new List<Proverb>();
            foreach (int i in randomProverbIndices)
            {
                proverbsSelected.Add(JsonUtility.FromJson<Proverb>(allProverbs[i].GetRawJsonValue()));
            }

            Dictionary<string, List<string>> allKeywordsPerPlayer = new Dictionary<string, List<string>>(); // keywords with user having them
            List<string> allKeywords = new List<string>();  // list to contain all (other)keywords from the proverbs selected
            // send proverbs to each player
            foreach (var player in PhotonNetwork.PlayerList)
            {
                // send the first {numberOfProverbsPerPlayer} proverbs to "player" and add keywords to dict
                for (int i = 0; i < numberOfProverbsPerPlayer; i++)
                {
                    Debug.Log(JsonUtility.ToJson(proverbsSelected[i]));
                    _photon.RPC("AddProverb", player, JsonUtility.ToJson(proverbsSelected[i]));

                    if (allKeywordsPerPlayer.ContainsKey(player.NickName))
                    {
                        allKeywordsPerPlayer[player.NickName].AddRange(proverbsSelected[i].keywords);
                        allKeywordsPerPlayer[player.NickName].AddRange(proverbsSelected[i].otherKeywords);   //TODO this is also needed
                    }
                    else
                    {
                        allKeywordsPerPlayer.Add(player.NickName, proverbsSelected[i].keywords);
                        allKeywordsPerPlayer[player.NickName].AddRange(proverbsSelected[i].otherKeywords);   //TODO this is also needed
                    }
                    
                    allKeywords.AddRange(proverbsSelected[i].keywords); // add keywords to list
                    allKeywords.AddRange(proverbsSelected[i].otherKeywords);    // add otherKeywords to list  //TODO this is also needed, otherKeywords are repeated twice
                }

                // remove the proverbs sent
                proverbsSelected.RemoveRange(0, numberOfProverbsPerPlayer);
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
            if (PhotonNetwork.CountOfPlayers <= 2)
            {
                SentMyKeywordsToAllPlayers(allKeywords);
            }
            else
            {
                SentMyKeywordsToOtherPlayers(allKeywords, allKeywordsPerPlayer);
            }
            
            
            _photon.RPC("LoadNextProverb", RpcTarget.All);
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }

    }


    [PunRPC]
    private void AddProverb(string p)
    {
        proverbs.Add(JsonUtility.FromJson<Proverb>(p));
    }

    /**
     * <summary>Loads the next available proverb into the scene with blanks or lets the master know this player is finished.</summary>
     */
    [PunRPC]
    private void LoadNextProverb()
    {
        // if no proverbs left, then this player is finished and master should be let known
        if (proverbs.Count == 0)
        {
            _photon.RPC("PlayerDone", RpcTarget.MasterClient);
            questionText.text = "You are done with your proverbs! Help your teammates finish theirs!";
            return;
        }
        Proverb proverb = proverbs.First();
        proverbs.RemoveAt(0);
        buttonsToCreateWords = proverb.keywords;
        correctProverb = proverb.phrase;
        answerProverb = correctProverb;
        
        // Set blank spaces in the proverb based on the keywords
        foreach (string v in buttonsToCreateWords)
        {
            answerProverb = answerProverb.Replace(v, "<u><alpha=#00>xxxxx</color></u>");
        }

        questionText.text = answerProverb;
    }
    
    /**
     * Called after every frame load.
     * <summary>Checks whether words from proverb need to be set back as buttons.</summary>
     */
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
                    CreateButtonForKeyword(LastClickedWord);
                }
            }
        }
    }
    
    /**
     * <summary>PunRPC taking care of receiving keywords from other players.</summary>
     */
    [PunRPC]
    void ReceiveChat(string msg)
    {
        CreateButtonForKeyword(msg);
    }

    /**
     * <summary>Sends missing keywords from this player's proverb to all players including yourself.</summary>
     * <param name="myKeywords">List of the keywords from this player's proverb to send to other players.</param>
     */
    private void SentMyKeywordsToAllPlayers(List<string> myKeywords)
    {
        int playerCountThisRoom = PhotonNetwork.CurrentRoom.PlayerCount;
        int i = 0;
        foreach (string keyword in myKeywords)
        {
            var playerToSendTo = PhotonNetwork.PlayerList[i % playerCountThisRoom];
            _photon.RPC("ReceiveChat", playerToSendTo, keyword);
            i++;
        }
    }
    
    /**
     * <summary>Sends missing keywords from this player's proverb to other players.</summary>
     * <param name="myKeywords">List of the keywords from this player's proverb to send to other players.</param>
     * <param name="allKeywordsPerPlayer">Dictionary having users as key and keywords as value, indicating what keywords each user will need in the game.</param>
     */
    private void SentMyKeywordsToOtherPlayers(List<string> myKeywords, Dictionary<string, List<string>> allKeywordsPerPlayer)
    {
        int playerCountThisRoom = PhotonNetwork.CurrentRoom.PlayerCount;
        int i = 0;
        foreach (string keyword in myKeywords)
        {
            var playerToSendTo = PhotonNetwork.PlayerList[i % playerCountThisRoom];
            while (allKeywordsPerPlayer[playerToSendTo.NickName].Contains(keyword))
            {
                i++;
                playerToSendTo = PhotonNetwork.PlayerList[i % playerCountThisRoom];
            }
            _photon.RPC("ReceiveChat", playerToSendTo, keyword);
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
    //     answerProverb = ReplaceFirst(answerProverb, "<u><alpha=#00>xxxxx</color></u>", word);
    //     questionText.text = answerProverb;
    // }

    //Remove a word from the proverb
    private void RemoveWord(string word)
    {
        word = "<u>" + word + "</u>";
        answerProverb = questionText.text;
        answerProverb = ReplaceFirst(answerProverb, word, "<u><alpha=#00>xxxxx</color></u>");
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
    //     if(canInput(answerProverb, "<u><alpha=#00>xxxxx</color></u>"))
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
        string playerProverb = answerProverb.Replace("<u>", "").Replace("<alpha=#00>", "").Replace("</color>", "").Replace("</u>", "");
        Debug.Log(playerProverb);
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

    /**
     * <summary>Creates button for a keyword at a nice place in the UI</summary> 
     */
    public void CreateButtonForKeyword(string text)
    {
        int index = 0;
        while (index < buttonIndices.Count && buttonIndices[index] != "")
        {
            index++;
        }

        CreateButton(index, text);
    }

    /**
     * <summary>Create a button at the right position and configures its components.</summary>
     */
    private void CreateButton(int i, string text)
    {
        int boardRightEdge = (int)keywordBoard.GetComponent<RectTransform>().rect.width;
        int boardTopEdge = (int)keywordBoard.GetComponent<RectTransform>().rect.height;

        Button newButton = Instantiate(dragDropButtonPrefab, keywordBoard, false);
        newButton.GetComponentInChildren<TextMeshProUGUI>().text = text;

        int buttonHeight = (int)newButton.GetComponent<RectTransform>().rect.height;
        int buttonWidth = (int)newButton.GetComponent<RectTransform>().rect.width;
        int xStart = boardRightEdge / 2 - buttonWidth / 2, yStart = boardTopEdge / 2 - buttonHeight / 2; // Get the starting location of the buttons
        int row = i % 3 - 1, col = i / 3; // Get the row and the column of the button in the table

        int spaceLength = 25;
        int widthSpacing = row * spaceLength, heightSpacing = col * spaceLength; // Keep track of the spacing between buttons in the table

        // Compute the final position of the button
        int xPos = row * buttonWidth + widthSpacing;
        int yPos = yStart - col * buttonHeight - heightSpacing;
        newButton.transform.localPosition = new Vector3(xPos, yPos);
        newButton.name = "AnswerButton" + i;

        // Configure the DragDrop script component of the button
        newButton.GetComponent<DragDrop>().canvas = canvas;
        newButton.GetComponent<DragDrop>().proverbText = questionText;
        newButton.GetComponent<DragDrop>().startingPosition = newButton.transform.localPosition;
        
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

    /**
     * <summary>Function to be called when any player is finished. This function is only called in the Master's CoopGame class.</summary>
     */
    [PunRPC]
    private void PlayerDone()
    {
        int amountOfPlayers = PhotonNetwork.PlayerListOthers.Count() + 1;
        playersDone = playersDone + 1;
        if (amountOfPlayers <= playersDone) //Everyone's done
        {
            Debug.Log("Everyone's done");
            _photon.RPC("LoadRoomAgain", RpcTarget.All);
        }
    }

    IEnumerator DisplayFeedbackMulti()
    {
        resultText.text = "Incorrect!";
        yield return new WaitForSeconds(3);
        resultText.text = "";
    }

    /**
     * <summary>Joins room where player is already in a level of it.</summary>
     */
    [PunRPC]
    private void LoadRoomAgain()
    {
        PhotonNetwork.LoadLevel("FillInBlanks");
    }
}
