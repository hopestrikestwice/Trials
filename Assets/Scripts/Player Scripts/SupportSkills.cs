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
        dashDirection = this.transform.forward;
    }

    public void ActivateUltimate()
    {
        Debug.Log("Change element button pressed");
        animator.SetBool("isChannelingElement", true);

        mostRecentElement = ElementFunctions.NextElement(mostRecentElement);
        this.GetComponent<PlayerManagerCore>().SetElement(mostRecentElement);
    }
    #endregion

    #region Animation Events

    public void FinishChannelingElement()
    {
        Debug.Log("Finish Attack");

        animator.SetBool("IsChannelingElement", false);

        if (photonView.IsMine)
        {
            playerUI.UnshadeUltimateIcon();
        }

        this.gameObject.GetComponent<PlayerActionCore>().setImmobile(false);
    }

    #endregion
}
