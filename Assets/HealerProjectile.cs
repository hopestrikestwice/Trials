using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class HealerProjectile : MonoBehaviour
{
    private int sigCharge;
    private GameObject playerSource;

    private float speed = 10f;

    private float lifetime = float.MaxValue;
    private float aliveTime = 0f;

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

    public int GetCharge()
    {
        return this.sigCharge;
    }

    public void SetCharge(int charge)
    {
        this.sigCharge = charge;
    }

    //Update is called once per frame
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
        if (other.CompareTag("BossProjectile")) //Boss?
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
        if (other.CompareTag("Player") && other.gameObject != this.playerSource)
        {
            Debug.Log("The healerProjectile's PlayerSource: "+this.playerSource);
            //If tank is main, says this playerSource is none
            Debug.Log("The player hit by heal: "+other.gameObject);
            this.playerSource.GetComponent<PlayerManagerCore>().HealPlayer(this.sigCharge);
            // other.gameObject.GetComponent<PlayerManagerCore>().HealPlayer(this.sigCharge);
            //This does not heal that actual player even though it says it does ^ (might be healing clone?)

            PhotonNetwork.Destroy(this.gameObject);
        }
    }

}
