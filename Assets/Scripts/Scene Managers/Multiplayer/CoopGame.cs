using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Storage;
using MiniJSON;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Random = System.Random;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class CoopGame : SingleplayerManager
{
    public static Launcher_FIB Instance;
    public static List<string> allWords;
    public static List<string> buttonIndices;
    private static string correctProverb;

    public PhotonView _photon;
    public string LastClickedWord;

    private StorageReference storageRef;
    private DateTime now;
    private List<string> buttonsToCreateWords;
    private List<Proverb> proverbs;
    private int playerCount;
    private int playersDone = 0;
    private string answerProverb;
    private string placeholderRegex = "<u><alpha=#00>xxxxx</color></u>";

    [SerializeField] private Transform keywordBoard;
    [SerializeField] private Button dragDropButtonPrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI[] otherPlayerNames;
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private GameObject hintButton;

    /// <summary>
    /// Called before the first frame update.
    /// - Creates player buttons;
    /// - If masterClient: gets proverbs from the DB, selects {numberOfProverbsPerPlayer} proverbs per player and randomly distributes them between players.
    /// </summary>
    async void Start()
    {
        // Initialize variables
        List<DataSnapshot> allProverbs = new List<DataSnapshot>();
        Random random = new Random();
        int numberOfProverbsPerPlayer = 2;
        int[] randomProverbIndices = {};

        allWords = new List<string>();
        buttonIndices = new List<string>();
        now = DateTime.UtcNow;
        proverbs = new List<Proverb>();
        playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        nextQuestionButton.SetActive(false);
        popupPanel.SetActive(false);

        // Set player buttons active and add their names
        for (int i = 0; i < playerCount - 1; i++)
        {
            otherPlayerNames[i].text = PhotonNetwork.PlayerListOthers[i].NickName;
            otherPlayerNames[i].transform.parent.GameObject().SetActive(true);
        }

        // Get proverbs from DB if this is master client and distribute them
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
                        // Select {playerCount} random proverb indices
                        randomProverbIndices = new int[playerCount * numberOfProverbsPerPlayer];
                        for (int i = 0; i < randomProverbIndices.Length; i++)
                        {
                            int nextInt = random.Next(0, Convert.ToInt32(snapshot.ChildrenCount));
                            if (!randomProverbIndices.Contains(nextInt)) randomProverbIndices[i] = nextInt;
                            else i--;
                        }
                        allProverbs = snapshot.Children.ToList();
                    }
                });

            // select proverbs from the database with the random indices selected above
            List<Proverb> proverbsSelected = new List<Proverb>();
            foreach (int i in randomProverbIndices)
            {
                Proverb proverbToAdd = JsonUtility.FromJson<Proverb>(allProverbs[i].GetRawJsonValue());
                List<string> keyWordsClone = proverbToAdd.keywords.Select(item => (string)item.Clone()).ToList();

                foreach (string word in keyWordsClone)
                {
                    for (int j = 1; j < Regex.Matches(proverbToAdd.phrase, word, RegexOptions.IgnoreCase).Count; j++)
                    {
                        proverbToAdd.keywords.Add(word);
                        Debug.Log(word);
                    }
                }
                proverbsSelected.Add(proverbToAdd);
            }

            Dictionary<string, List<string>> allKeywordsPerPlayer = new Dictionary<string, List<string>>(); // keywords with user having them
            List<string> allKeywords = new List<string>();  // list to contain all (other)keywords from the proverbs selected
            // Send proverbs to each player
            foreach (var player in PhotonNetwork.PlayerList)
            {
                // Send the first {numberOfProverbsPerPlayer} proverbs to "player" and add keywords to dict
                for (int i = 0; i < numberOfProverbsPerPlayer; i++)
                {
                    Debug.Log(JsonUtility.ToJson(proverbsSelected[i]));
                    _photon.RPC("AddProverb", player, JsonUtility.ToJson(proverbsSelected[i]));

                    if (allKeywordsPerPlayer.ContainsKey(player.NickName)) allKeywordsPerPlayer[player.NickName].AddRange(proverbsSelected[i].keywords);
                    else allKeywordsPerPlayer.Add(player.NickName, proverbsSelected[i].keywords);
                    
                    // Add keywords to list
                    allKeywords.AddRange(proverbsSelected[i].keywords);
                }

                // Remove the proverbs sent
                proverbsSelected.RemoveRange(0, numberOfProverbsPerPlayer);
            }

            // Shuffling list of words
            for (int i = 0; i < allKeywords.Count; i++)
            {
                string temp = allKeywords[i];
                int randomIndex = random.Next(i, allKeywords.Count);
                allKeywords[i] = allKeywords[randomIndex];
                allKeywords[randomIndex] = temp;
            }

            // Distribute keywords between players
            if (PhotonNetwork.CurrentRoom.PlayerCount <= 2) SentMyKeywordsToAllPlayers(allKeywords);
            else SentMyKeywordsToOtherPlayers(allKeywords, allKeywordsPerPlayer);
            
            _photon.RPC("LoadNextProverb", RpcTarget.All);
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
    }

    /// <summary>
    /// Method for adding a proverb to the game.
    /// </summary>
    /// <param name="proverbToAdd">String denoting the JSON object of the proverb to be added.</param>
    [PunRPC]
    private void AddProverb(string proverbToAdd)
    {
        proverbs.Add(JsonUtility.FromJson<Proverb>(proverbToAdd));
    }

    /// <summary>
    /// Loads the next available proverb into the scene with blanks or lets the master know this player is finished.
    /// </summary>
    [PunRPC]
    private void LoadNextProverb()
    {
        // If no proverbs left, then this player is finished and master should be let known
        if (proverbs.Count == 0)
        {
            questionText.text = "You are done with your proverbs! Help your teammates finish theirs!";
            checkButton.gameObject.SetActive(false);
            nextQuestionButton.SetActive(false);
            _photon.RPC("PlayerDone", RpcTarget.MasterClient);
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
            answerProverb = Regex.Replace(answerProverb, v, placeholderRegex, RegexOptions.IgnoreCase);
        }

        questionText.text = answerProverb;
        popupPanel.SetActive(false);
        
        // Get a reference to the storage service, using the default Firebase App
        storageRef = FirebaseStorage.DefaultInstance.GetReferenceFromUrl("gs://sp-proverb-game.appspot.com");

        // Reference for retrieving an image
        StorageReference imageRef = storageRef.Child("proverbs/" + proverb.image);
            
        const long maxAllowedSize = 1 * 1024 * 1024;
        byte[] fileContents;
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
                popupPanel.GetComponentInChildren<RawImage>().texture = tex;
            }
        });
    }
    
    /// <summary>
    /// Executes on each frame update.
    /// Checks whether words from proverb need to be set back as buttons.
    /// </summary>
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var wordIndex = TMP_TextUtilities.FindIntersectingWord(questionText, Input.mousePosition, null);

            if (wordIndex != -1)
            {
                LastClickedWord = questionText.textInfo.wordInfo[wordIndex].GetWord();

                // If a keyword inside of the proverb is clicked, remove that keyword from the proverb and create a button
                string[] splits = questionText.text.Split(" ");
                
                bool isKeyword = false;

                foreach (string word in allWords)
                {
                    if (splits[wordIndex].Contains(word))
                    {
                        isKeyword = true;
                        LastClickedWord = word;
                    }
                }

                // If a keyword inside of the proverb is clicked, remove that keyword from the proverb and create a button
                if ((wordIndex > -1) && (isKeyword))
                {
                    RemoveWord(LastClickedWord, wordIndex);
                    CreateButtonForKeyword(LastClickedWord);
                }
            }
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount < playerCount)
        {
            StartCoroutine(endGame("A player has unfortunately left your game... Moving you back to the lobby..."));
        }
    }
    
    /// <summary>
    /// PunRPC taking care of receiving keywords from other players.
    /// </summary>
    /// <param name="msg"></param>
    [PunRPC]
    void ReceiveChat(string msg)
    {
        CreateButtonForKeyword(msg);
    }

    /// <summary>
    /// Sends missing keywords from this player's proverb to all players including yourself.
    /// </summary>
    /// <param name="myKeywords">List of the keywords from this player's proverb to send to other players.</param>
    private void SentMyKeywordsToAllPlayers(List<string> myKeywords)
    {
        Debug.Log("SentMyKeywordsToAllPlayers");
        int playerCountThisRoom = PhotonNetwork.CurrentRoom.PlayerCount;
        int i = 0;
        foreach (string keyword in myKeywords)
        {
            var playerToSendTo = PhotonNetwork.PlayerList[i % playerCountThisRoom];
            _photon.RPC("ReceiveChat", playerToSendTo, keyword);
            i++;
        }
    }

    /// <summary>
    /// Sends missing keywords from this player's proverb to other players.
    /// </summary>
    /// <param name="myKeywords">List of the keywords from this player's proverb to send to other players.</param>
    /// <param name="allKeywordsPerPlayer">Dictionary having users as key and keywords as value, indicating what keywords each user will need in the game.</param>
    private void SentMyKeywordsToOtherPlayers(List<string> myKeywords, Dictionary<string, List<string>> allKeywordsPerPlayer)
    {
        Debug.Log("SentMyKeywordsToOtherPlayers");
        int playerCountThisRoom = PhotonNetwork.CurrentRoom.PlayerCount;
        int i = 0;
        foreach (string keyword in myKeywords)
        {
            var playerToSendTo = PhotonNetwork.PlayerList[i % playerCountThisRoom];

            while (allKeywordsPerPlayer[playerToSendTo.NickName].Contains(keyword)) // TODO what if all players have the specific keyword?
            {
                i++;
                playerToSendTo = PhotonNetwork.PlayerList[i % playerCountThisRoom];
            }
            _photon.RPC("ReceiveChat", playerToSendTo, keyword);
            i++;
        }
    }
    
    /// <summary>
    /// Check if text is able to be put in the sentence.
    /// </summary>
    /// <param name="text">String denoting the sentence.</param>
    /// <param name="search">Text that is being checked for adding to the sentence.</param>
    /// <returns>Whether or not the text can be addedd to the sentence.</returns>
    public bool CanInput(string text, string search)
    {
        int pos = text.IndexOf(search);
        if (pos < 0) return false;
        else return true;
    }

    /// <summary>
    /// Remove a word from the proverb.
    /// </summary>
    /// <param name="word">The word to be removed.</param>
    /// <param name="wordIndex">The index of the word to be removed.</param>
    private void RemoveWord(string word, int wordIndex)
    {
        answerProverb = questionText.text;
        string[] splits = questionText.text.Split(" ");
        splits[wordIndex] = splits[wordIndex].Replace(word, placeholderRegex);
        
        answerProverb = string.Join(" ", splits);

        // TODO: Fix this crude solution to extra spaces
        answerProverb = answerProverb.Replace("  ", " ");
        answerProverb = answerProverb.Replace("  ", " ");

        questionText.text = answerProverb;
    }

    /// <summary>
    /// Function that replaces the first occurrence of a string "search" inside of a string "text" with a string "replace".
    /// </summary>
    /// <param name="text">Text denoting the sentence.</param>
    /// <param name="search">The string whose first occurrence should be replaced.</param>
    /// <param name="replace">The string to replace the first occurrence with.</param>
    /// <returns>The modified sentence.</returns>
    public string ReplaceFirst(string text, string search, string replace)
    {
        if (!CanInput(answerProverb, search)) return text;
        else return text.Substring(0, text.IndexOf(search)) + replace + text.Substring(text.IndexOf(search) + search.Length);
    }

    /// <summary>
    /// Display the feedback after the player answers the question.
    /// </summary>
    public void CheckAnswer()
    {
        answerProverb = questionText.text;
        string playerProverb = answerProverb.Replace("<u>", "").Replace("</u>", "");
        bool correct = playerProverb.ToLower().Equals(correctProverb.ToLower());

        if (correct)
        {
            resultText.text = "Correct";
            nextQuestionButton.SetActive(true);
            checkButton.gameObject.SetActive(false);
        }
        else resultText.text = "Incorrect";
        // TODO: Disable the ability to click and check new answers
    }

    /// <summary>
    /// Called when next question button is clicked. Loads the next proverb and resets the scene for it.
    /// </summary>
    public void NextQuestionButtonClicked()
    {
        LoadNextProverb();
        resultText.text = "";
        checkButton.gameObject.SetActive(true);
        nextQuestionButton.SetActive(false);
    }

    /// <summary>
    /// Creates button for a keyword at a nice place in the UI.
    /// </summary>
    /// <param name="text">String denoting the keyword that the button is created for.</param>
    public void CreateButtonForKeyword(string text)
    {
        int index = 0;
        while (index < buttonIndices.Count && buttonIndices[index] != "") index++;
        CreateButton(index, text);
    }

    /// <summary>
    /// Create a button at the right position and configures its components.
    /// </summary>
    /// <param name="i">Index of the button to be created.</param>
    /// <param name="text">String denoting the keyword that the button is created for.</param>
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
        
        if (i >= buttonIndices.Count) buttonIndices.Add(text);
        else buttonIndices[i] = text;
        allWords.Add(text);
    }

    /// <summary>
    /// Function to be called when any player is finished. This function is only called in the Master's CoopGame class.
    /// </summary>
    [PunRPC]
    private void PlayerDone()
    {
        int amountOfPlayers = PhotonNetwork.PlayerListOthers.Count() + 1;
        playersDone = playersDone + 1;
        // Check if all players are done
        if (amountOfPlayers <= playersDone)
        {
            Debug.Log("Everyone's done");
            _photon.RPC("LoadRoomAgain", RpcTarget.All);
        }
    }

    /// <summary>
    /// Method for ending the game.
    /// </summary>
    /// <param name="message">String denoting the message that is shown to the players when the game ends.</param>
    /// <returns>A command telling the program to wait 3 seconds.</returns>
    IEnumerator endGame(string message)
    {
        checkButton.gameObject.SetActive(false);
        nextQuestionButton.SetActive(false);
        questionText.text = message;
        PhotonNetwork.CurrentRoom.IsVisible = true;
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("FillInBlanks");
    }

    /// <summary>
    /// Joins room where player is already in a level of it.
    /// </summary>
    [PunRPC]
    private void LoadRoomAgain()
    {
        double seconds = Math.Round((DateTime.UtcNow - now).TotalSeconds, 2);
        StartCoroutine(endGame("Good job! You finished all of the proverbs in: " + seconds + " seconds! Moving you to the lobby..."));
    }

    /// <summary>
    /// Change the state of the pop-up panel.
    /// </summary>
    public void changePopUpState()
    {
        if (hintButton.GetComponentInChildren<TextMeshProUGUI>().text.Contains("Show"))
        {
            hintButton.GetComponentInChildren<TextMeshProUGUI>().text = "Hide Picture";
        }
        else
        {
            hintButton.GetComponentInChildren<TextMeshProUGUI>().text = "Show Picture";
        }
        popupPanel.SetActive(!popupPanel.activeSelf);
    }
}