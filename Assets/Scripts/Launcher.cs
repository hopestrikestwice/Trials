/// Launcher.cs
///
/// Script in the game lobby scene that allows connecting to the Photon Cloud
/// server. Joins an existing game room or creates one if necessary.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    #region Private Serailizable Fields

    /// <summary>
    /// The maximum number of players per room. When a room is full, it can't be joined by new players, so a new room will be created.
    /// </summary>
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, so a new room will be created.")]
    [SerializeField]
    private byte maxPlayersPerRoom = 4;

    [Tooltip("The UI Panel to let the user enter name, connect & play.")]
    [SerializeField]
    private GameObject controlPanel;

    [Tooltip("The UI Label to inform the user that the connection is in progress.")]
    [SerializeField]
    private GameObject progressLabel;

    #endregion

    #region Private Fields

    /// <summary>
    /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
    /// </summary>
    string gameVersion = "1";

    /// <summary>
    /// Keep track of current process. Since connection is asynchronous and based on several Photon callbacks, we need to keep track of this to properly adjust behavior
    /// when we receive call back by Photon. Typically, this is used for OnConnetedToMaster() callback.
    /// </summary>
    bool isConnecting;

    #endregion

    #region MonoBehaviour CallBacks

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    private void Awake()
    {
        // #Critical
        // This makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically.
        // Gilbert - So this makes all clients sync to the same scene? And so we only need to LoadLevel() on master client instead of calling on every client.
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
    }

    #endregion

    #region Public Method

    /// <summary>
    /// Start the connection process.
    /// - If already connected, we attempted joining a random room.
    /// - If not yet connected, Connect this application instance to Photon Cloud Network.
    /// </summary>
    public void Connect()
    {
        progressLabel.SetActive(true);
        controlPanel.SetActive(false);
        this.GetComponent<Animator>().SetBool("start", true);
    }

    // Called at end of canvas shift animation to perform actual connection logic.
    public void AnimationEndConnect()
    {
        //Check if connected. Join room if connected, otherwise initiate connection to server.
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Connected currentplayercount is: " + (PhotonNetwork.CurrentRoom.PlayerCount - 1));
            // #Critical: We attempt to join the specified room. If it fails, we get notified in OnJoinRandomFailed() and we'll create a room instead.
            PhotonNetwork.JoinOrCreateRoom("dragon", null, null);
        }
        else
        {
            Debug.Log("not connected?");
            // #Critical: we must first and foremost connect to Photon Online Server.
            isConnecting = PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    #endregion

    #region MonoBehaviourPunCallbacks Callbacks
    
    // OnConnectedToMaster called whenever client is connected and ready for matchmaking (joining a room). 
    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() called by PUN");

        if (isConnecting)
        {
            // #Critical: First thing we want to do once connected to master is to join a potential existing room.
            // If one does not exist, we'll be called back with OnJoinRandomFailed()
            PhotonNetwork.JoinOrCreateRoom("dragon", null, null);
            isConnecting = false;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);

        isConnecting = false;

        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() called by PUN with reason {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinRandomFailed() called by PUN. No random room available, so we create one." +
            "\nCalling: PhotonNetwork.CreateRoom");

        //#Critical: failed to join a random room, so either none exist or all are full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room!");

        // #Critical: We only load if we are the first player, otherwise rely on "PhotonNetwork.AutomaticallySyncScene" to sync our instance  scene with the master client
        // First player is always the master.
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("Loading 'Waiting Room'");

            // #Critical
            // Load the room level.
            PhotonNetwork.LoadLevel("Waiting Room");
        }
    }
    #endregion


}
