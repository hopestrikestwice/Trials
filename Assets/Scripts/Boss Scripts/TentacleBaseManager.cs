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

    /* Kraken hit feedback. */
    private Color originalTint;
    private Color hitTint = Color.red;
    private float timeSinceHit = 1;

    #region Monobehavipur
    private void Start()
    {
        this.krakenManager = transform.parent.GetComponent<BossManagerCore>();

        this.originalTint = this.transform.Find("Cone").GetComponent<Renderer>().material.GetColor("Toon_Ramp_Tint");
    }

    private void Update()
    {
        this.transform.Find("Cone").GetComponent<Renderer>().material.SetColor("Toon_Ramp_Tint", Color.Lerp(hitTint, originalTint, timeSinceHit));
        
        timeSinceHit = Mathf.Min(timeSinceHit + Time.deltaTime, 1);
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        //If object is a player projectile, decrement health
        if (other.CompareTag("PlayerProjectile"))
        {
            photonView.RPC("HitTint", RpcTarget.All);
            this.krakenManager.Hit();
        }

        //If object is a healer projectile (signature), decrement health more
        if (other.CompareTag("Heal"))
        {
            float scale = (float)((other.GetComponent<HealerProjectile>().GetCharge())/5.0);
            this.krakenManager.SignatureHit(scale);
        }
    }

    #region RPC
    [PunRPC]
    void HitTint()
    {
        this.timeSinceHit = 0;
    }
    #endregion
}
