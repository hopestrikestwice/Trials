using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class ProjectileMovement : MonoBehaviour
{
    private float speed = 10f;

    private float lifetime = float.MaxValue;
    private float aliveTime = 0f;

    public void SetLifetime(float l)
    {
        this.lifetime = l;
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
            PhotonNetwork.Destroy(this.gameObject);
        }
    }
}
