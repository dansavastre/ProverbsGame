using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class Launcher_MM : MonoBehaviourPunCallbacks {
    public static Launcher_MM Instance; // instance of the launcher

    // placeholders for information necessary for the multi-player layout
    [SerializeField] TMP_InputField roomNameInputField_MM;
    [SerializeField] TMP_Text errorText_MM;
    [SerializeField] TMP_Text roomNameText_MM;
    [SerializeField] Transform roomListContent_MM;
    [SerializeField] Transform playerListContent_MM;
    [SerializeField] GameObject roomListItemPrefab_MM;
    [SerializeField] GameObject playerListItemPrefab_MM;
    [SerializeField] GameObject startGameButton_MM;

    /// <summary>
    /// Disconnects the player and sends them to the multiplayer menu scene.
    /// </summary>
    public void OpenMultiplayerTitleMenuScene() {
        SceneManager.LoadScene("TitleMenu");
        PhotonNetwork.Disconnect();
    }

    /// <summary>
    /// Executed when an instance of this class is initialized.
    /// </summary>
    void Awake() {
        Instance = this;
    }

    /// <summary>
    /// Executed when the game is started.
    /// </summary>
    void Start() {
        Debug.Log("Connecting to Master.");
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// Executed when the player connects to the server.
    /// </summary>
    public override void OnConnectedToMaster() {
        Debug.Log("Connected to Master.");
        PhotonNetwork.JoinLobby(); // you need to be in a Lobby to join a Room

        PhotonNetwork.AutomaticallySyncScene = true; // automatically load the scene for all clients
    }

    /// <summary>
    /// Executed when the player joins a lobby.
    /// </summary>
    public override void OnJoinedLobby() {
        MenuManager.Instance.OpenMenu("Meaning Matching"); // open the title menu on joining lobby
        Debug.Log("Joined Lobby.");
        PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
    }

    /// <summary>
    /// Method for creating a multi-player room.
    /// </summary>
    public void CreateRoom() {
        if (string.IsNullOrEmpty(roomNameInputField_MM.text))
            return;

        PhotonNetwork.CreateRoom(roomNameInputField_MM.text);
        MenuManager.Instance.OpenMenu("Loading");
    }

    /// <summary>
    /// Executed when the player joins a room.
    /// </summary>
    public override void OnJoinedRoom() {
        MenuManager.Instance.OpenMenu("Room");
        roomNameText_MM.text = PhotonNetwork.CurrentRoom.Name;

        // Remove all the players in the previous room to start with a clean slate
        foreach (Transform child in playerListContent_MM)
            Destroy(child.gameObject);

        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Count(); ++i)
            Instantiate(playerListItemPrefab_MM, playerListContent_MM).GetComponent<PlayerListItem>().SetUp(players[i]);

        startGameButton_MM.SetActive(PhotonNetwork.IsMasterClient); // only the host of the game can start the game
    }

    /// <summary>
    /// Method that makes sure a new host is chosen for the game if the initial host leaves.
    /// </summary>
    /// <param name="newMasterClient">the new host for the game</param>
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient) {
        startGameButton_MM.SetActive(PhotonNetwork.IsMasterClient);
    }

    /// <summary>
    /// Method that notifies the player when creating a room fails.
    /// </summary>
    /// <param name="returnCode">the return code of the error shown to the player</param>
    /// <param name="message">the message accompanying the error</param>
    public override void OnCreateRoomFailed(short returnCode, string message) {
        errorText_MM.text = "Room Creation Failed: " + message;
        MenuManager.Instance.OpenMenu("Error");
    }

    /// <summary>
    /// Starts the game.
    /// </summary>
    public void StartGame() {
        PhotonNetwork.LoadLevel(1); // TODO: change the level to the actual fill in the blanks multiplayer game mode
    }

    /// <summary>
    /// Method for leaving a room and sending the player back to the multi-player menu.
    /// </summary>
    public void LeaveRoom() {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("Loading");
    }

    /// <summary>
    /// Method for joining a certain room.
    /// </summary>
    /// <param name="info">the information of the room to be joined</param>
    public void JoinRoom(RoomInfo info) {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("Loading");
    }

    /// <summary>
    /// Executes when the player leaves the lobby.
    /// </summary>
    public override void OnLeftLobby() {
        MenuManager.Instance.OpenMenu("Meaning Matching");
    }

    /// <summary>
    /// Executes when the list of rooms available to the player changes.
    /// </summary>
    /// <param name="roomList">the list of information on the different rooms that are currently available</param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        foreach (Transform transform in roomListContent_MM)
            Destroy(transform.gameObject);

        foreach (RoomInfo roomInfo in roomList) {
            if (roomInfo.RemovedFromList) // remove closed rooms from the list
                continue;
            Instantiate(roomListItemPrefab_MM, roomListContent_MM).GetComponent<RoomListItem>().SetUp(roomInfo);
        }
    }

    /// <summary>
    /// Executes when the player enters a room.
    /// </summary>
    /// <param name="newPlayer">the Player object to be added to the layout of the menu</param>
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
        Instantiate(playerListItemPrefab_MM, playerListContent_MM).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }
}
