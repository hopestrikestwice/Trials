/// GameManager.cs
/// 
/// Handles this player leaving the room and other players leaving or joining
/// the room. Also instantiates networked assets related to setting up the game
/// or player.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Public Fields

    public static GameManager Instance;

    [Tooltip("The prefab to use for representing the player")]
    public GameObject[] playerPrefabs;

    [Tooltip("The prefab to use for representing the Kraken")]
    public GameObject krakenPrefab;

    #endregion

    #region Photon Callbacks

    /// <summary>
    /// Called when the local player left the room. We need to load back into the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", newPlayer.NickName); // not seen  if you're the new player connecting

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);

            //LoadArena();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", otherPlayer.NickName); // seen when other disconnects

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before player leaves room? - not  sure what this means

            LoadArena();
        }
    }

    #endregion

    #region MonoBehaviour Callbacks

    private void Start()
    {
        Instance = this;

        if (playerPrefabs.Length <= 0)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else if (PlayerManagerCore.LocalPlayerInstance == null)
        {
            Debug.LogFormat("We are Instantiating LocalPlayer from  {0}", Application.loadedLevelName);
            // We're in a room. Spawn a character for the local player. It gets synced by using PhotonNetwork.Instantiate
            PhotonNetwork.Instantiate(this.playerPrefabs[PhotonNetwork.LocalPlayer.ActorNumber - 1].name, new Vector3(0f, 15f, 0f), Quaternion.identity, 0);
        }
        else
        {
            Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
        }

        // Kraken must be instantiated after LocalPlayer, or else player doesn't show on the screen
        // I think this has to do with TentacleAnimationManager's TargetNearestPlayer(), but need to read more to see
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("Instantiating Kraken from {0}", Application.loadedLevelName);

            PhotonNetwork.Instantiate(this.krakenPrefab.name,
                                        new Vector3(0f, 0f, 0f),
                                        Quaternion.Euler(0, -90, 0),
                                        0);
        }
    }

    #endregion

    #region Public Methods

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    #region Private Methods

    void LoadArena()
    {
        //Gilbert - Since we AutomaticallySyncScene = true in launcher, we only need to load scenes on the master client and the other clients will follow.
        //Gilbert - Note: AutomaticallySyncScene may mean we can't use Unity functions to change scene at all - we have to  use PhotonNetwork functions like LoadLevel
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork: Trying to load a level but we are not the master Client");
        }
        Debug.LogFormat("PhotonNetwork: Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LoadLevel("Main Game");
    }

    #endregion
}
