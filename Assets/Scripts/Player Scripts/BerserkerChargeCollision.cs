/* TODO: jank file to get berserker ult working in time for release */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class BerserkerChargeCollision : MonoBehaviourPun
{
    private BerserkerShootSlash script;

    public void SetSlashScript(BerserkerShootSlash script)
    {
        this.script = script;
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
            script.ReportHitBoss(other);
        }
    }
}
