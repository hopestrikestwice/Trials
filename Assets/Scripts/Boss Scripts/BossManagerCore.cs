/// BossManagerCore.cs
/// 
/// Manages Boss's stats (health, etc.) and syncs them across the network.
/// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class BossManagerCore : MonoBehaviourPun, IPunObservable
{
    [Tooltip("The Kraken's UI GameObject Prefab")]
    [SerializeField]
    public GameObject KrakenUiPrefab;

    private GameObject krakenUI;

    [Tooltip("The current Health of the boss")]
    private float Health = 1f;


    private void Start()
    {
        krakenUI = Instantiate(KrakenUiPrefab);
        krakenUI.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
    }

    public void Hit()
    {
        Debug.Log("Boss Hit!");
        Health -= 0.25f;
    }

    public void SignatureHit(float scale)
    {
        Debug.Log("Boss Hit (by signature)!");
        Health -= scale;
    }

    public float GetHealth()
    {
        return this.Health;
    }

    #region IPunObservable Implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send others our data
            stream.SendNext(this.Health);
        }
        else
        {
            //Network player, receive data
            this.Health = (float)stream.ReceiveNext();
        }
    }

    #endregion
}
