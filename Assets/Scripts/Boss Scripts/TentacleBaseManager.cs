using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class TentacleBaseManager : MonoBehaviourPun
{
    private KrakenManager krakenManager;

    private void Start()
    {
        this.krakenManager = transform.parent.GetComponent<KrakenManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        //If object is a boss projectile, decrement health
        if (other.CompareTag("PlayerProjectile"))
        {
            this.krakenManager.Hit();
        }
    }
}
