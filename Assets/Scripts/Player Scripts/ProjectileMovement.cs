/// ProjectileMovement.cs
/// 
/// Attach to a projectile to control its speed and lifetime. In this way we
/// can create "melee" and "ranged" projectiles.
///

//TODO: only used by the support now


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class ProjectileMovement : MonoBehaviourPun
{
    private float speed = 5f;

    private float lifetime = 3f;
    private float aliveTime = 0f;

    private GameObject playerSource;

    private int projectileDamage = 10;

    public void SetLifetime(float l)
    {
        this.lifetime = l;
    }

    public void SetPlayer(GameObject player)
    {
        this.playerSource = player;
        Debug.Log("This player: "+player);
    }

    public GameObject GetPlayer()
    {
        return this.playerSource;
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
            other.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.MasterClient, projectileDamage, playerSource.GetComponent<PlayerManagerCore>().GetElement());

            if (playerSource != null && playerSource.GetComponent<HealerSkills>() != null)
            {
                playerSource.GetComponent<HealerSkills>().AddCharge();
            }

            /* Destroy projectile on hit */
            PhotonNetwork.Destroy(this.gameObject);
        }
    }

}
