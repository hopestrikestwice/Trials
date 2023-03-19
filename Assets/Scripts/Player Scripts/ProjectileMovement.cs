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

    private GameObject playerSource = null;

    public void SetLifetime(float l)
    {
        this.lifetime = l;
    }

    public void SetPlayer(GameObject player)
    {
        this.playerSource = player;
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
            if (playerSource != null && playerSource.GetComponent<PlayerManagerCore>().GetPlayerType() == PlayerType.Healer)
            {
                playerSource.GetComponent<PlayerActionCore>().AddCharge();
                Debug.Log("Healer Charge +1");
            }
            PhotonNetwork.Destroy(this.gameObject);
        }
    }

}
