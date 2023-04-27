using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class KrakenHeadAnimationManager : MonoBehaviourPun
{
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("Tentacle is Missing Animator Component", this);
        }
    }

    #region AnimationEvents
    public void SetScreech(int val)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (val == 1)
        {
            animator.SetBool("screeching", true);
        }
        else if (val == 0)
        {
            animator.SetBool("screeching", false);
        }
        else
        {
            Debug.LogError("Invalid input to Kraken SetScreech!");
        }
    }
    #endregion
}
