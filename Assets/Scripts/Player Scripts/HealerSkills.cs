using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class HealerSkills : MonoBehaviourPun, IPlayerSkills
{
    #region Private Fields
    private PlayerUI playerUI;

    private Animator animator;

    #endregion

    #region Monobehaviour Callbacks

    private void Start()
    {
        if (photonView.IsMine)
        {
            this.playerUI = this.GetComponent<PlayerManagerCore>().getPlayerUI();
        }

        animator = GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("HealerSkills is Missing Animator Component", this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
    }

    #endregion

    #region IPlayerSkills Implementation
    public void ActivateBasicAttack()
    {
        Debug.Log("Healer basic attack");
    }

    public void ActivateSkill()
    {
        Debug.Log("Healer secondary skill activated");
        animator.SetBool("isSecondarySkilling", true);
    }

    public void ActivateUltimate()
    {
        Debug.Log("AOE heal ability pressed");
        animator.SetBool("isLargeHealing", true);
    }
    #endregion

    #region Animation Events
    public void FinishHealerSecondary()
    {
        playerUI.UnshadeIcon(SkillUI.SECONDARY);
        animator.SetBool("isSecondarySkilling", false);
        this.gameObject.GetComponent<PlayerActionCore>().setImmobile(false);
    }

    public void FinishLargeHeal()
    {
        playerUI.UnshadeIcon(SkillUI.ULTIMATE);
        animator.SetBool("isLargeHealing", false);
        this.gameObject.GetComponent<PlayerActionCore>().setImmobile(false);
    }
    #endregion

}
