/// ProjectileMovement.cs
/// 
/// Attach to a projectile to control its speed and lifetime. In this way we
/// can create "melee" and "ranged" projectiles.
///

//TODO: note this is same as projmovement but without destroy on hit.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class TankProjectile : MonoBehaviourPun
{
    private float speed = 10f;

    private float lifetime = float.MaxValue;
    private float aliveTime = 0f;

    private int projectileDamage = 10;

    public void SetLifetime(float l)
    {
        this.lifetime = l;
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        aliveTime += Time.deltaTime;

        if (aliveTime >= lifetime)
        {
            PhotonNetwork.Destroy(this.gameObject);
        } else
        {
            this.transform.position += this.transform.forward * this.speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        /* We let localclients handle hit collision, to be more accurate for the local player */
        if (!photonView.IsMine)
        {
            return;
        }

        if (other.CompareTag("BossTentacle"))
        {
            Debug.Log("Player Projectile hit Boss");
            other.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.MasterClient, projectileDamage);
        }
    }

}
