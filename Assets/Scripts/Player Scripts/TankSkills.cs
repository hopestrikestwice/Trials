using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class TankSkills : MonoBehaviourPunCallbacks, IPlayerSkills
{
    #region Private Fields
    private PlayerUI playerUI;

    private Animator animator;

    #region Shield Variables
    [SerializeField]
    private GameObject shieldSmall;
    [SerializeField]
    private GameObject shieldLarge;
    #endregion

    #endregion

    #region Monobehaviour

    private void Start()
    {
        if (photonView.IsMine)
        {
            this.playerUI = this.GetComponent<PlayerManagerCore>().getPlayerUI();
        }

        animator = GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("BeserkerSkills is Missing Animator Component", this);
        }
    }

    #endregion

    #region IPlayerSkills Implementation

    public void ActivateSkill()
    {
        Debug.Log("Shield button pressed.");
        animator.SetBool("isSmallShielding", true);
    }

    public void ActivateUltimate()
    {
        Debug.Log("Large shield button pressed.");
        animator.SetBool("isLargeShielding", true);
    }
    #endregion

    #region Animation Events

    public void FinishShieldSmall()
    {
        animator.SetBool("isSmallShielding", false);

        if (photonView.IsMine)
        {
            playerUI.UnshadeIcon(SkillUI.SECONDARY);
        }

        this.gameObject.GetComponent<PlayerActionCore>().setImmobile(false);
    }
    
    public void FinishShieldLarge()
    {
        animator.SetBool("isLargeShielding", false);

        if (photonView.IsMine)
        {
            playerUI.UnshadeIcon(SkillUI.ULTIMATE);
        }

        this.gameObject.GetComponent<PlayerActionCore>().setImmobile(false);
    }
    #endregion

}
