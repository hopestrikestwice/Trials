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

    [SerializeField]
    private int maxDamageValue = 50;
    [SerializeField]
    private int maxHealValue = 50;

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

    void LateUpdate()
    {
        //Destroys projectile after healing other player
        if (playerHealed)
        {
            playerHealed = false;
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(this.gameObject);
            }
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
            Debug.Log("Healer Projectile hit Boss");
            other.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.MasterClient, this.maxDamageValue * this.sigCharge / 5.0);

            PhotonNetwork.Destroy(this.gameObject);
        }
        /* Need to make sure doesn't have HealerSkills component so healer doesn't autoheal self on cast */
        if (other.CompareTag("Player") && other.gameObject.GetComponent<HealerSkills>() == null)
        {
            Debug.Log("Healer Projectile hit Player");

            PhotonView otherPhotonView = other.GetComponent<PhotonView>();

            otherPhotonView.RPC("HealPlayer", otherPhotonView.Owner, (int)(this.maxHealValue * this.sigCharge / 5.0));

            if (playerSource != null)
            {
                //Heals healer (self) if still alive
                this.playerSource.GetComponent<PlayerManagerCore>().HealPlayer((int)(this.maxHealValue * this.sigCharge / 5.0));
                playerHealed = true;
            }

            PhotonNetwork.Destroy(this.gameObject);
        }
    }
}
