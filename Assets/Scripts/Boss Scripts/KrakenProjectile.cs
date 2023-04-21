/// KrakenRock.cs
/// 
/// Script to control the Kraken's rock projectile. This projectile starts
/// above each player and shoots downward until colliding with a player or terrain.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class KrakenProjectile : MonoBehaviourPun
{
    private float speed = 10f;

    private bool markedForDestroy = false;

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        this.transform.position += this.transform.forward * this.speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (this.transform.position.y < 0)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }
}