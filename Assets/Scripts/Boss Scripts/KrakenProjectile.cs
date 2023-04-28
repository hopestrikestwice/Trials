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
    private float timePassed = 0;

    private void Update()
    {
        if (this.transform.position.y > 0)
        {
            this.GetComponent<Rigidbody>().AddForce(Vector3.down * 1000f * timePassed);
            timePassed += Time.deltaTime;
        }

        if (this.transform.position.y < 0)
        {
            this.GetComponent<Rigidbody>().useGravity = false;
            this.GetComponent<Rigidbody>().velocity = Vector3.zero;
            Invoke("DestroySelf", 1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        Invoke("DestroySelf", 1f);
    }

    private void DestroySelf()
    {
        Destroy(this.gameObject);
    }
}