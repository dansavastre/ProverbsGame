using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Storage;
using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;


/**
 * This class is based on CoopGame.cs and is quite similar to it. The best way to improve
 * this is to have one base class for these two classes.
 */
public class MeaningMatchingGame : SingleplayerManager
{
    public static Launcher_MM Instance;
    public PhotonView _photon;
    
    // UI elements
    [SerializeField] private Transform proverbBoard;
    [SerializeField] private Button proverbDragDropButtonPrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI[] otherPlayerNames;
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private GameObject hintButton;

    // Variables
    private DateTime now;
    private int playersDone = 0;
    public static string correctProverb;
    public static List<string> buttonIndices;
    private List<Proverb> proverbs;
    readonly Random random = new Random();
    private StorageReference storageRef;
    private int playerCount;    
    public static string currentMeaning;
    public GameObject correctAnswerObjectToRemove; // this is the proverb button and is set by DragDropMM if the proverb dragged on meaning is correct 
    
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
        now = DateTime.UtcNow;
        buttonIndices = new List<string>();
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        proverbs = new List<Proverb>();
        nextQuestionButton.SetActive(false);
        popupPanel.SetActive(false);

        // variables needed here
        playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        int numberOfProverbsPerPlayer = 4;
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
            List<Proverb> proverbsToSend = new List<Proverb>();
            foreach (int i in randomProverbIndices)
            {
                Proverb proverbToAdd = JsonUtility.FromJson<Proverb>(allProverbs[i].GetRawJsonValue());

                proverbsToSend.Add(proverbToAdd);
            }

            Dictionary<string, List<Proverb>> allPorverbsPerPlayer = new Dictionary<string, List<Proverb>>(); // proverbs with users having them
            List<Proverb> proverbsSelected = new List<Proverb>(proverbsToSend); 
            // send proverbs to each player
            foreach (var player in PhotonNetwork.PlayerList)
            {
                // send the first {numberOfProverbsPerPlayer} proverbs to "player" and add keywords to dict
                for (int i = 0; i < numberOfProverbsPerPlayer; i++)
                {
                    _photon.RPC("AddProverb", player, JsonUtility.ToJson(proverbsToSend[i]));
                    if (allPorverbsPerPlayer.ContainsKey(player.NickName))
                    {
                        allPorverbsPerPlayer[player.NickName].Add(proverbsToSend[i]);
                    }
                    else
                    {
                        List<Proverb> l = new List<Proverb>();
                        l.Add(proverbsToSend[i]);
                        allPorverbsPerPlayer.Add(player.NickName, l);
                    }
                }

                // remove the proverbs sent
                proverbsToSend.RemoveRange(0, numberOfProverbsPerPlayer);
            }

            // Shuffle the list of proverbs
            for (int i = 0; i < proverbsSelected.Count; i++)
            {
                Proverb temp = proverbsSelected[i];
                int randomIndex = random.Next(i, proverbsSelected.Count);
                proverbsSelected[i] = proverbsSelected[randomIndex];
                proverbsSelected[randomIndex] = temp;
            }
            
            Debug.Log("Number of players: " + PhotonNetwork.CurrentRoom.PlayerCount);
            if (PhotonNetwork.CurrentRoom.PlayerCount <= 2)
            {
                SentProverbsToAllPlayers(proverbsSelected);
            }
            else
            {
                SentProverbsToOtherPlayers(proverbsSelected, allPorverbsPerPlayer);
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
     * <summary>Loads the next available proverb into the scene or lets the master know this player is finished.</summary>
     */
    [PunRPC]
    private void LoadNextProverb()
    {
        // if no proverbs left, then this player is finished and master should be let known
        if (proverbs.Count == 0)
        {
            questionText.text = "You are done with your proverbs! Help your teammates finish theirs!";
            nextQuestionButton.SetActive(false);
            _photon.RPC("PlayerDone", RpcTarget.MasterClient);
            return;
        }
        
        Proverb proverb = proverbs.First();
        proverbs.RemoveAt(0);
        correctProverb = proverb.phrase;
        questionText.text = proverb.meaning;
        currentMeaning = proverb.meaning;
        popupPanel.SetActive(false);
        
        // getting image from database and setting it
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
    
    /**
     * Called after every frame load.
     * <summary>Checks whether words from proverb need to be set back as buttons.</summary>
     */
    private void Update()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < playerCount)
        {
            StartCoroutine(endGame("A player has unfortunately left your game... Moving you back to the lobby..."));
        }
    }
    
    /**
     * <summary>PunRPC taking care of receiving keywords from other players.</summary>
     */
    [PunRPC]
    void ReceiveChat(string msg)
    {
        CreateButtonForProverb(msg);
    }

    /**
     * <summary>Sends proverbs from this the list to all players including this player.</summary>
     * <param name="proverbsSelected">List of the proverbs to be divided between players.</param>
     */
    private void SentProverbsToAllPlayers(List<Proverb> proverbsSelected)
    {
        Debug.Log("in sendProverbToAllPLayers. Length of proverbs: " + proverbsSelected.Count);
        int playerCountThisRoom = PhotonNetwork.CurrentRoom.PlayerCount;
        int i = 0;
        foreach (Proverb proverb in proverbsSelected)
        {
            var playerToSendTo = PhotonNetwork.PlayerList[i % playerCountThisRoom];
            _photon.RPC("ReceiveChat", playerToSendTo, proverb.phrase);
            i++;
        }
    }
    
    /**
     * <summary>Sends proverbs from this the list to other players in the room.</summary>
     * <param name="proverbsSelected">List of the proverbs to be divided between players.</param>
     * <param name="allProverbsPerPlayer">Dictionary having users as key and list of proverbs as value, indicating what proverbs each user is assigned.</param>
     */
    private void SentProverbsToOtherPlayers(List<Proverb> proverbsSelected, Dictionary<string, List<Proverb>> allProverbsPerPlayer)
    {
        int playerCountThisRoom = PhotonNetwork.CurrentRoom.PlayerCount;
        int i = 0;
        foreach (Proverb proverb in proverbsSelected)
        {
            var playerToSendTo = PhotonNetwork.PlayerList[i % playerCountThisRoom];

            while (allProverbsPerPlayer[playerToSendTo.NickName].Contains(proverb))
            {
                i++;
                playerToSendTo = PhotonNetwork.PlayerList[i % playerCountThisRoom];
            }
            _photon.RPC("ReceiveChat", playerToSendTo, proverb.phrase);
            i++;
        }
    }
    
    // Display the feedback after the player answers the question
    public void Answer(bool correct)
    {
        if (correct)
        {
            resultText.text = "Correct\nThe correct proverb is highlighted";
            nextQuestionButton.SetActive(true);
        }
        else
        {
            resultText.text = "Incorrect";
        }
    }
    
    /**
     * <summary>Called when next question button is clicked. Loads the next proverb and resets the scene for it.</summary>
     */
    public void NextQuestionButtonClicked()
    {
        buttonIndices[buttonIndices.IndexOf(correctProverb)] = "";
        Destroy(correctAnswerObjectToRemove.GameObject());
        LoadNextProverb();
        resultText.text = "";
        nextQuestionButton.SetActive(false);
    }
    
    /**
     * <summary>Creates button for a keyword at a nice place in the UI</summary> 
     */
    public void CreateButtonForProverb(string text)
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
        int boardHeight = (int)proverbBoard.GetComponent<RectTransform>().rect.height;

        Button newButton = Instantiate(proverbDragDropButtonPrefab, proverbBoard, false);
        newButton.GetComponentInChildren<TextMeshProUGUI>().text = text;

        int buttonHeight = (int)newButton.GetComponent<RectTransform>().rect.height;

        // Compute the final position of the button
        float yPos = (boardHeight / 2) - buttonHeight / 2 - i * 1.1f * buttonHeight - 10;
        newButton.transform.localPosition = new Vector3(0, yPos);
        newButton.name = "Proverb" + i;

        // Configure the DragDrop script component of the button
        newButton.GetComponent<DragDropMM>().canvas = canvas;
        newButton.GetComponent<DragDropMM>().proverbText = questionText;
        newButton.GetComponent<DragDropMM>().startingPosition = newButton.transform.localPosition;
        newButton.GetComponent<DragDropMM>().initialText = text;
        newButton.GetComponent<DragDropMM>().initialSize = newButton.GetComponent<RectTransform>().sizeDelta;
        newButton.GetComponent<DragDropMM>().meaningMatchingGame = this;
        
        if (i >= buttonIndices.Count)
        {
            buttonIndices.Add(text);
        }
        else
        {
            buttonIndices[i] = text;
        }
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
    
    IEnumerator endGame(string message)
    {
        nextQuestionButton.SetActive(false);
        questionText.text = message;
        PhotonNetwork.CurrentRoom.IsVisible = true;
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("MeaningMatching");
    }
    
    /**
     * <summary>Joins room where player is already in a level of it.</summary>
     */
    [PunRPC]
    private void LoadRoomAgain()
    {
        double seconds = Math.Round((DateTime.UtcNow - now).TotalSeconds, 2);
        StartCoroutine(endGame("Good job! You finished all of the proverbs in: " + seconds + " seconds! Moving you to the lobby..."));
    }
    
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