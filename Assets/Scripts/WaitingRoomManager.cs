using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class WaitingRoomManager : MonoBehaviourPun
{
    [SerializeField]
    private GameObject[] splashes;

    [SerializeField]
    private GameObject[] tutorials;

    void Start()
    {
        splashes[PhotonNetwork.LocalPlayer.ActorNumber - 1].SetActive(true);
        tutorials[PhotonNetwork.LocalPlayer.ActorNumber - 1].SetActive(true);
    }
}
