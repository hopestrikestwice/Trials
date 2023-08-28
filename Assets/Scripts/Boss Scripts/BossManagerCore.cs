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

    [SerializeField]
    private int maxHealth;

    [Tooltip("The current Health of the boss")]
    private float health;

    private GameObject krakenUI;
    private int bossPhase = 1;

    private const float phase1Cutoff = 0.75f;
    private const float phase2Cutoff = 0.3f;

    private void Start()
    {
        health = maxHealth;

        krakenUI = Instantiate(KrakenUiPrefab);
        krakenUI.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);

        /* TODO: Background music handling should probably get refactored
         * out of this file. */
        /* Turn music lower so we can balance sfx */
        FMOD.Studio.Bus masterBus = FMODUnity.RuntimeManager.GetBus("bus:/");
        masterBus.setVolume(0.25f);
    }

    private void Update()
    {
        PhaseCheck();
    }

    public int GetPhase()
    {
        return bossPhase;
    }

    public void Hit(int damage)
    {
        /* Only want MasterClient to handle boss logic */
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        Debug.Log("Boss Hit!");
        health -= damage;
    }

    public void SignatureHit(float scale)
    {
        Debug.Log("Boss Hit (by signature)! scale:"+scale);
        health -= scale;
    }

    public float GetMaxHealth()
    {
        return this.maxHealth;
    }

    public float GetHealth()
    {
        return this.health;
    }

    public void PhaseCheck()
    {
        if (health < phase1Cutoff * maxHealth && bossPhase == 1)
        {
            bossPhase = 2;
            RuntimeManager.StudioSystem.setParameterByName("boss_phase", 1);
        } else if (health < phase2Cutoff * maxHealth && bossPhase == 2)
        {
            bossPhase = 3;
            RuntimeManager.StudioSystem.setParameterByName("boss_phase", 2);
        }
    }

    #region IPunObservable Implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send others our data
            stream.SendNext(this.health);
        }
        else
        {
            //Network player, receive data
            this.health = (float)stream.ReceiveNext();
        }
    }

    #endregion
}
