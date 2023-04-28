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
    private Color originalBaseColor;
    private Color originalFirstShadeColor;
    private Color hitTint = Color.red;
    private float timeSinceHit = 1;

    #region Monobehavipur
    private void Start()
    {
        this.krakenManager = transform.parent.GetComponent<BossManagerCore>();

        this.originalBaseColor = this.transform.Find("Cone").GetComponent<Renderer>().material.GetColor("_BaseColor");
        this.originalFirstShadeColor = this.transform.Find("Cone").GetComponent<Renderer>().material.GetColor("_1st_ShadeColor");
    }

    private void Update()
    {
        this.transform.Find("Cone").GetComponent<Renderer>().material.SetColor("_BaseColor", Color.Lerp(hitTint, originalBaseColor, timeSinceHit));
        this.transform.Find("Cone").GetComponent<Renderer>().material.SetColor("_1st_ShadeColor", Color.Lerp(hitTint, originalFirstShadeColor, timeSinceHit));

        timeSinceHit = Mathf.Min(timeSinceHit + Time.deltaTime, 1);
    }
    #endregion

    #region RPC

    [PunRPC]
    public void TakeDamage(int damage, Element playerElement)
    {
        /* We only want the MasterClient to be performing boss functions. */
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (ElementFunctions.isWeakness(krakenManager.GetComponent<KrakenSkills>().GetElement(), playerElement))
        {
            damage = damage * 5 / 4;
        }
        else if (ElementFunctions.isResistance(krakenManager.GetComponent<KrakenSkills>().GetElement(), playerElement))
        {
            damage = damage * 3 / 4;
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
