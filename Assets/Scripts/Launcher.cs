using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks 
{
    // Instance of the launcher
    public static Launcher Instance; 

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
        PhotonNetwork.ConnectUsingSettings();
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
        MenuManager.Instance.OpenMenu("Title");
        Debug.Log("Joined Lobby.");
        PhotonNetwork.NickName = "Player " + Random.Range(0, 1000).ToString("0000");
    }

    /// <summary>
    /// Start the game.
    /// </summary>
    public void StartGame() 
    {
        // TODO: change the level to the actual fill in the blanks multiplayer game mode
        PhotonNetwork.LoadLevel(1);
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
    /// Executes when the player left the lobby.
    /// </summary>
    public override void OnLeftLobby() 
    {
        MenuManager.Instance.OpenMenu("Title");
    }
}