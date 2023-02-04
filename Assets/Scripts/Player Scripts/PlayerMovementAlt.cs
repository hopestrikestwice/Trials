using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class PlayerMovementAlt : MonoBehaviourPun
{
    #region Private Fields
    private CharacterController controller;

    private Animator animator;

    private float walkSpeed = 7f;
    private float gravity = 9.8f;
    #endregion

    #region Monobehaviour Callbacks
    private void Start()
    {
        animator = this.GetComponent<Animator>();
        this.controller = this.GetComponent<CharacterController>();
        if (!controller)
        {
            Debug.LogError("PlayerMovement is Missing CharacterController Component", this);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        // Get Horizontal and Vertical Input
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Calculate the Direction to Move based on the tranform of the Player
        Vector3 moveDirectionForward = Vector3.forward * verticalInput;
        Vector3 moveDirectionSide = Vector3.right * horizontalInput;

        //find the direction
        Vector3 direction = (moveDirectionForward + moveDirectionSide).normalized;
        direction = Quaternion.Euler(0, -45, 0) * direction;

        //cue animator bools for walking
        if (direction != Vector3.zero)
        {
            animator.SetBool("IsMoving", true);
            this.transform.forward = direction;
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }

        //find the distance
        Vector3 distance = this.transform.forward * direction.magnitude * walkSpeed * Time.deltaTime;

        //Apply gravity to distance
        distance.y -= gravity * Time.deltaTime;

        // Apply Movement to Player
        controller.Move(distance);
    }
    #endregion
}
