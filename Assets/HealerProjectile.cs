using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class HealerProjectile : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    private int sigCharge;
    private GameObject playerSource;

    private float speed = 10f;

    private float lifetime = float.MaxValue;
    private float aliveTime = 0f;

    private bool playerHealed = false;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        sigCharge = (int)instantiationData[0];
    }

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
        if (other.CompareTag("BossProjectile"))
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
        if (other.CompareTag("Player") && other.gameObject.GetComponent<HealerSkills>() == null)
        {
            Debug.Log("Proj hit Player");

            if (playerSource != null)
            {
                //Heals healer
                this.playerSource.GetComponent<PlayerManagerCore>().HealPlayer((float)(this.sigCharge/5.0));

                //Heals player
                // other.gameObject.GetComponent<PlayerManagerCore>().HealPlayer((float)(this.sigCharge/5.0));

                //Ideally, should destory the projectile after collision with player
                // if (photonView.IsMine)
                // {
                //     PhotonNetwork.Destroy(this.gameObject);
                // }
            }
        }
    }
}
