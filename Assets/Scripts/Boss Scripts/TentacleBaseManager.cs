/// KrakenAnimationManager.cs
/// 
/// Kraken collision detection. Forwards collisions to KrakenAnimationManager.cs
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class TentacleBaseManager : MonoBehaviourPun
{
    private BossManagerCore krakenManager; 

    private void Start()
    {
        this.krakenManager = transform.parent.GetComponent<BossManagerCore>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        //If object is a player projectile, decrement health
        if (other.CompareTag("PlayerProjectile"))
        {
            this.krakenManager.Hit();
        }

        //If object is a healer projectile (signature), decrement health more
        if (other.CompareTag("Heal"))
        {
            float scale = (float)((other.GetComponent<HealerProjectile>().GetCharge())/5.0);
            this.krakenManager.SignatureHit(scale);
        }
    }
}
