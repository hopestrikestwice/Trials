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

    #region RPC

    [PunRPC]
    public void TakeDamage(int damage)
    {
        /* We only want the MasterClient to be performing boss functions. */
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        this.krakenManager.Hit(damage);

        photonView.RPC("BeginHitTint", RpcTarget.All);
    }

    [PunRPC]
    void BeginHitTint()
    {
        this.timeSinceHit = 0;
    }
    #endregion
}
