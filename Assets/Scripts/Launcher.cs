using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;

public class Launcher : MonoBehaviourPunCallbacks {
    public static Launcher Instance; // instance of the launcher

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
        MenuManager.Instance.OpenMenu("Title"); // open the title menu on joining lobby
        Debug.Log("Joined Lobby.");
        PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
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
    /// Executes when the player left the lobby.
    /// </summary>
    public override void OnLeftLobby() {
        MenuManager.Instance.OpenMenu("Title");
    }
}
