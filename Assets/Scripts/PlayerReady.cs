using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;

public class PlayerReady : MonoBehaviourPun
{
    [SerializeField]
    public GameObject readyPanel;
    public GameObject waitingPanel;

    private int numReady;
    private int maxPlayers = 1;

    private void Start()
    {
        numReady = 0;
    }

    public void PressReadyButton()
    {
        photonView.RPC("ReadyPlayer", RpcTarget.All);

        readyPanel.SetActive(false);
        waitingPanel.SetActive(true);
    }

    #region RPC
    [PunRPC]
    void ReadyPlayer()
    {
        this.numReady += 1;

        /* Only the master can start the game when all players ready. */
        if (PhotonNetwork.IsMasterClient && this.numReady == this.maxPlayers)
        {
            PhotonNetwork.LoadLevel("Main Game");
        }
    }
    #endregion
}
