using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class Launcher_FIB : MonoBehaviourPunCallbacks {
    public static Launcher_FIB Instance;

    public PhotonView _photon;

    [SerializeField] TMP_InputField roomNameInputField_FIB;
    [SerializeField] TMP_Text errorText_FIB;
    [SerializeField] TMP_Text roomNameText_FIB;
    [SerializeField] Transform roomListContent_FIB;
    [SerializeField] Transform playerListContent_FIB;
    [SerializeField] GameObject roomListItemPrefab_FIB;
    [SerializeField] GameObject playerListItemPrefab_FIB;
    [SerializeField] GameObject startGameButton_FIB;

    public void OpenMultiplayerTitleMenuScene() {
        SceneManager.LoadScene("TitleMenu");
        PhotonNetwork.Disconnect();
    }

    void Awake() {
        Instance = this;
    }

    void Start() {
        Debug.Log("Connecting to Master.");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() {
        Debug.Log("Connected to Master.");
        PhotonNetwork.JoinLobby(); // you need to be in a Lobby to join a Room

        PhotonNetwork.AutomaticallySyncScene = true; // automatically load the scene for all clients
    }

    public override void OnJoinedLobby() {
        MenuManager.Instance.OpenMenu("Fill In The Gaps"); // open the title menu on joining lobby
        Debug.Log("Joined Lobby.");
        PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
    }

    public void CreateRoom() {
        if (string.IsNullOrEmpty(roomNameInputField_FIB.text))
            return;

        PhotonNetwork.CreateRoom(roomNameInputField_FIB.text);
        MenuManager.Instance.OpenMenu("Loading");
    }

    public override void OnJoinedRoom() {
        MenuManager.Instance.OpenMenu("Room");
        roomNameText_FIB.text = PhotonNetwork.CurrentRoom.Name;

        // Remove all the players in the previous room to start with a clean slate
        foreach (Transform child in playerListContent_FIB)
            Destroy(child.gameObject);

        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Count(); ++i)
            Instantiate(playerListItemPrefab_FIB, playerListContent_FIB).GetComponent<PlayerListItem>().SetUp(players[i]);

        startGameButton_FIB.SetActive(PhotonNetwork.IsMasterClient); // only the host of the game can start the game
    }

    /**
     * Method that makes sure a new host is chosen for the game if the initial host leaves.
     */
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient) {
        startGameButton_FIB.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message) {
        errorText_FIB.text = "Room Creation Failed: " + message;
        MenuManager.Instance.OpenMenu("Error");
    }

    public void StartGame() {
        PhotonNetwork.LoadLevel("FillBlankMultiplayer");
    }

    public void LeaveRoom() {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("Loading");
    }

    public void JoinRoom(RoomInfo info) {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("Loading");
    }

    public override void OnLeftLobby() {
        MenuManager.Instance.OpenMenu("Fill In The Gaps");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        foreach (Transform transform in roomListContent_FIB)
            Destroy(transform.gameObject);

        foreach (RoomInfo roomInfo in roomList) {
            if (roomInfo.RemovedFromList) // remove closed rooms from the list
                continue;
            Instantiate(roomListItemPrefab_FIB, roomListContent_FIB).GetComponent<RoomListItem>().SetUp(roomInfo);
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
        Instantiate(playerListItemPrefab_FIB, playerListContent_FIB).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }
}
