/// BossManagerCore.cs
/// 
/// Manages Boss's stats (health, etc.) and syncs them across the network.
/// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using FMODUnity;

public class BossManagerCore : MonoBehaviourPun, IPunObservable
{
    [Tooltip("The Kraken's UI GameObject Prefab")]
    [SerializeField]
    public GameObject KrakenUiPrefab;

    private GameObject krakenUI;
    private int bossPhase = 1;

    [Tooltip("The current Health of the boss")]
    private float Health = 1f;

    private const float phase1Cutoff = 0.75f;

    private void Start()
    {
        krakenUI = Instantiate(KrakenUiPrefab);
        krakenUI.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
    }

    public void Hit()
    {
        Debug.Log("Boss Hit!");
        Health -= 0.25f;
        PhaseCheck();
    }

    public float GetHealth()
    {
        return this.Health;
    }

    public void PhaseCheck()
    {
        if (Health < phase1Cutoff && bossPhase == 1)
        {
            bossPhase = 2;
            RuntimeManager.StudioSystem.setParameterByName("boss_phase", 1);
        }
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
