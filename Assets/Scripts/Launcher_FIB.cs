using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class Launcher_FIB : MonoBehaviourPunCallbacks 
{
    // Instance of the launcher
    public static Launcher_FIB Instance; 

    public PhotonView _photon;
    private List<RoomInfo> allRooms = new List<RoomInfo>();

    [SerializeField] TMP_InputField roomNameInputField_FIB;
    [SerializeField] TMP_Text errorText_FIB;
    [SerializeField] TMP_Text roomNameText_FIB;
    [SerializeField] Transform roomListContent_FIB;
    [SerializeField] Transform playerListContent_FIB;
    [SerializeField] GameObject roomListItemPrefab_FIB;
    [SerializeField] GameObject playerListItemPrefab_FIB;
    [SerializeField] GameObject startGameButton_FIB;

    /// <summary>
    /// Disconnects the player and sends them to the multiplayer menu scene.
    /// </summary>
    public void OpenMultiplayerTitleMenuScene() 
    {
        SceneManager.LoadScene("MainMenu");
        PhotonNetwork.Disconnect();
    }

    /// <summary>
    /// Executed when an instance of this class is initialized.
    /// </summary>
    void Awake() 
    {
        Instance = this;
    }

    /// <summary>
    /// Executed when the game is started.
    /// </summary>
    void Start()
    {
        Debug.Log("Connecting to Master.");
        if (PhotonNetwork.IsConnected) OnJoinedRoom();
        else PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// Executed when the player connects to the server.
    /// </summary>
    public override void OnConnectedToMaster() 
    {
        Debug.Log("Connected to Master.");

        // You need to be in a Lobby to join a Room
        PhotonNetwork.JoinLobby();

        // Automatically load the scene for all clients
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    /// <summary>
    /// Executed when the player joins a lobby.
    /// </summary>
    public override void OnJoinedLobby() 
    {
        // Open the title menu on joining lobby
        MenuManager.Instance.OpenMenu("Fill In The Gaps");
        Debug.Log("Joined Lobby.");
        PhotonNetwork.NickName = AccountManager.playerName;
    }

    /// <summary>
    /// Method for creating a multi-player room.
    /// </summary>
    public void CreateRoom() 
    {
        if (string.IsNullOrEmpty(roomNameInputField_FIB.text)) return;

        PhotonNetwork.CreateRoom(roomNameInputField_FIB.text);
        MenuManager.Instance.OpenMenu("Loading");
    }


    /// <summary>
    /// Executes when a player leaves the room.
    /// </summary>
    public override void OnLeftRoom()
    {
        allRooms = new List<RoomInfo>();
    }

    /// <summary>
    /// Executed when the player joins a room.
    /// </summary>
    public override void OnJoinedRoom() 
    {
        MenuManager.Instance.OpenMenu("Room");
        roomNameText_FIB.text = PhotonNetwork.CurrentRoom.Name;

        // Remove all the players in the previous room to start with a clean slate
        foreach (Transform child in playerListContent_FIB)
        {
            Destroy(child.gameObject);
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount >= 4)
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
        else PhotonNetwork.CurrentRoom.IsVisible = true;

        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Count(); ++i)
        {
            Instantiate(playerListItemPrefab_FIB, playerListContent_FIB).GetComponent<PlayerListItem>().SetUp(players[i]);
        }

        // Only the host of the game can start the game
        startGameButton_FIB.SetActive(PhotonNetwork.IsMasterClient);
    }

    /// <summary>
    /// Method that makes sure a new host is chosen for the game if the initial host leaves.
    /// </summary>
    /// <param name="newMasterClient">The new host for the game.</param>
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        startGameButton_FIB.SetActive(PhotonNetwork.IsMasterClient);
    }

    /// <summary>
    /// Method that notifies the player when creating a room fails.
    /// </summary>
    /// <param name="returnCode">The return code of the error shown to the player.</param>
    /// <param name="message">The message accompanying the error.</param>
    public override void OnCreateRoomFailed(short returnCode, string message) 
    {
        errorText_FIB.text = "Room Creation Failed: " + message;
        MenuManager.Instance.OpenMenu("Error");
    }

    /// <summary>
    /// Start the game.
    /// </summary>
    public void StartGame() 
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            PhotonNetwork.LoadLevel("FillBlankMultiplayer");
        }
    }

    /// <summary>
    /// Method for leaving a room and sending the player back to the multi-player menu.
    /// </summary>
    public void LeaveRoom() 
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("Loading");
    }

    /// <summary>
    /// Method for joining a certain room.
    /// </summary>
    /// <param name="info">The information of the room to be joined.</param>
    public void JoinRoom(RoomInfo info) 
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("Loading");
    }

    /// <summary>
    /// Executes when the player leaves the lobby.
    /// </summary>
    public override void OnLeftLobby() 
    {
        MenuManager.Instance.OpenMenu("Fill In The Gaps");
    }

    /// <summary>
    /// Executes when the list of rooms available to the player changes.
    /// </summary>
    /// <param name="roomList">The list of information on the different rooms that are currently available.</param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList) 
    {
        foreach (var roomInfo in roomList)
        {
            if (roomInfo.RemovedFromList) allRooms.Remove(roomInfo);
            else allRooms.Add(roomInfo);
        }

        allRooms = allRooms.ToHashSet().ToList();
        
        foreach (Transform transform in roomListContent_FIB)
        {
            Destroy(transform.gameObject);
        }
        
        foreach (RoomInfo roomInfo in allRooms) 
        {
            Instantiate(roomListItemPrefab_FIB, roomListContent_FIB).GetComponent<RoomListItem>().SetUp(roomInfo);
        }
    }

    /// <summary>
    /// Executes when the player enters a room.
    /// </summary>
    /// <param name="newPlayer">The Player object to be added to the layout of the menu.</param>
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) 
    {
        Instantiate(playerListItemPrefab_FIB, playerListContent_FIB).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }
}