using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class KrakenHeadAnimationManager : MonoBehaviourPun
{
    Animator animator;

    private AudioSource audioSource;
    [SerializeField]
    private AudioClip screechSFX;

    [SerializeField]
    private GameObject kraken;

    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("Tentacle is Missing Animator Component", this);
        }

        audioSource = this.GetComponent<AudioSource>();
        if (!audioSource)
        {
            Debug.LogError("Tentacle is Missing AudioSource Component", this);
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


            Element newElement = ElementFunctions.RandomElementNotGiven(this.GetComponentInParent<KrakenSkills>().GetElement());
            kraken.GetComponent<PhotonView>().RPC("SetAura", RpcTarget.All, newElement);
        }
        else
        {
            Debug.LogError("Invalid input to Kraken SetScreech!");
        }
    }

    public void PlayScreech()
    {
        audioSource.PlayOneShot(screechSFX);
    }
    #endregion
}
