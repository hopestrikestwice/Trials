/// BossManagerCore.cs
/// 
/// Manages Boss's stats (health, etc.) and syncs them across the network.
/// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class BossManagerCore : MonoBehaviourPun, IPunObservable
{
    [Tooltip("The current Health of the boss")]
    private float Health = 1f;

    public void Hit()
    {
        Debug.Log("Boss Hit!");
        Health -= 0.25f;
    }

    public float GetHealth()
    {
        return this.Health;
    }

    #region IPunObservable Implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send others our data
            stream.SendNext(this.Health);
        }
        else
        {
            //Network player, receive data
            this.Health = (float)stream.ReceiveNext();
        }
    }

    #endregion
}
