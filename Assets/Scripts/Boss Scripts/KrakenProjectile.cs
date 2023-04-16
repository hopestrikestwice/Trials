/// KrakenRock.cs
/// 
/// Script to control the Kraken's rock projectile. This projectile starts
/// above each player and shoots downward until colliding with a player or terrain.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class KrakenProjectile : MonoBehaviour
{
    private float speed = 10f;

    // Update is called once per frame
    void Update()
    {
        this.transform.position += this.transform.forward * this.speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("BossProjectile"))
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }
}