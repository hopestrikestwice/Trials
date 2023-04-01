/// ProjectileMovement.cs
/// 
/// Attach to a projectile to control its speed and lifetime. In this way we
/// can create "melee" and "ranged" projectiles.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class ProjectileMovement : MonoBehaviour
{
    private float speed = 10f;

    private float lifetime = float.MaxValue;
    private float aliveTime = 0f;

    private GameObject playerSource;

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
        if (other.CompareTag("BossProjectile"))
        {
            Debug.Log("Proj hit Boss");
            // Debug.Log("PlayerSource "+playerSource+" PlayerType "+playerSource.GetComponent<PlayerManagerCore>().GetPlayerType());
            if (playerSource != null && playerSource.GetComponent<PlayerManagerCore>().GetPlayerType() == PlayerType.Healer)
            {
                playerSource.GetComponent<HealerSkills>().AddCharge();
            }
            PhotonNetwork.Destroy(this.gameObject);
        }
    }

}
