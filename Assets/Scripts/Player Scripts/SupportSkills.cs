using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class SupportSkills : MonoBehaviourPun, IPlayerSkills
{
    #region Private Fields
    private PlayerUI playerUI;

    private Animator animator;

    #region Dash Variables
    private CharacterController controller;

    // Dash direction, is not null when in middle of dash
    private Vector3 dashDirection = Vector3.zero;
    private float dashSpeed = 50f;
    // Maximum dash time in seconds
    private float dashTimeMax = 0.1f;
    // Current amount of time spent dashing.
    private float dashTimeCurrent = 0f;
    #endregion

    private Element mostRecentElement;

    #endregion

    #region Monobehaviour Callbacks

    private void Start()
    {
        if (photonView.IsMine)
        {
            this.playerUI = this.GetComponent<PlayerManagerCore>().getPlayerUI();
        }

        this.controller = this.GetComponent<CharacterController>();
        if (!controller)
        {
            Debug.LogError("PlayerMovement is Missing CharacterController Component", this);
        }

        animator = GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("BeserkerSkills is Missing Animator Component", this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        if (!dashDirection.Equals(Vector3.zero))
        {
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            dashTimeCurrent += Time.deltaTime;
            if (dashTimeCurrent >= dashTimeMax)
            {
                dashTimeCurrent = 0;
                dashDirection = Vector3.zero;
            }
        }
    }

    #endregion


    #region IPlayerSkills Implementation

    public void ActivateSkill()
    {
        Debug.Log("Dash button pressed.");
        animator.SetBool("isSecondarySkilling", true);

        dashDirection = this.transform.forward;
    }

    public void ActivateUltimate()
    {
        Debug.Log("Change element button pressed");
        animator.SetBool("isUltimating", true);

        mostRecentElement = ElementFunctions.NextElement(mostRecentElement);
        this.GetComponent<PlayerManagerCore>().SetElement(mostRecentElement);
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

    public void FinishChannelingElement()
    {
        Debug.Log("Finish Attack");
    }

    #endregion
}
