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
    public void ActivateSkill()
    {
        Debug.Log("Healer secondary skill activated");
        animator.SetBool("isSecondarySkilling", true);
    }

    public void ActivateUltimate()
    {
        Debug.Log("AOE heal ability pressed");
        animator.SetBool("isUltimating", true);
    }
    #endregion

    #region Animation Events
    /*  Each ability animation dispatches an event named
            Finish{BasicAttack, SecondarySkill, Ultimate}          
        
        This event is handled in two places:
        1) In PlayerActionCore.cs, for general shared behavior like resetting
            the immobile flag, UI, and animation parameter.
        2) [Optionally] Here for character-specific effects. Not all abilities
        have character-specific effects, so should see which stubs can be
        deleted later on.
        
        Note: If we want more control over when the character-specific
        effects take place (before/after the character regains mobility, how
        long before/after), we can dispatch a separate event instead of handling
        the same-named event.
    */
    #endregion

}
