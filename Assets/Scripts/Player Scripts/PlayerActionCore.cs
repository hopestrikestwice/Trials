/// PlayerActionCore.cs
///
/// Player movement and keypresses for abilities, calling the character
/// specific skills implementation.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class PlayerActionCore : MonoBehaviourPun
{
    #region Private Fields
    private PlayerUI playerUI;

    private CharacterController controller;
    private float walkSpeed = 7f;
    private float gravity = 9.8f;
    private bool immobile = false;

    private Animator animator;
    private IPlayerSkills skills;

    #region Attack Variables
    [SerializeField]
    [Tooltip("The attack prefab without any enhancements.")]
    private GameObject defaultAttackPrefab;
    [SerializeField]
    [Tooltip("Array of attack with elemental shaders in order of Fire, Water, Earth")]
    private GameObject[] elementalAttackPrefabs;

    private GameObject currentAttackProjectile = null;

    private Element currentElement = Element.None;

    private float attackProjectileOffset = 1f;
    private float attackProjectileLifetime = 1f;
    #endregion

    #endregion

    #region Monobehaviour Callbacks
    private void Start()
    {
        //TODO: somewhat hacky fix for UIs isolated to each player. Maybe come up with something better in time?
        if (photonView.IsMine)
        {
            this.playerUI = this.GetComponent<PlayerManagerCore>().getPlayerUI();
            if (!playerUI)
            {
                Debug.LogError("PlayerActionCore is missing PlayerUI component", this);
            }
        }

        this.animator = this.GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("PlayerActionCore is missing Animator component", this);
        }

        this.controller = this.GetComponent<CharacterController>();
        if (!controller)
        {
            Debug.LogError("PlayerActionCore is missing CharacterController Component", this);
        }

        this.skills = this.GetComponent<IPlayerSkills>();
    }
    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        if (!immobile)
        {
            if (Input.GetButtonDown("Fire1") && currentAttackProjectile == null)
            {
                immobile = true;

                if (photonView.IsMine)
                {
                    playerUI.ShadeIcon(SkillUI.PRIMARY);
                }

                this.ActivateBasicAttack();
            }

            if (Input.GetButtonDown("Fire2"))
            {
                immobile = true;

                if (photonView.IsMine)
                {
                    playerUI.ShadeIcon(SkillUI.SECONDARY);
                }

                skills.ActivateSkill();
            }

            if (Input.GetButtonDown("Fire3"))
            {
                immobile = true;

                if (photonView.IsMine)
                {
                    playerUI.ShadeIcon(SkillUI.ULTIMATE);
                }

                skills.ActivateUltimate();
            }
        }

        MoveCharacter();
    }
    #endregion

    /* TODO: more clear method to manipulate moveability? */
    public void setImmobile(bool m)
    {
        immobile = m;
    }

    public void setElement(Element element)
    {
        this.currentElement = element;
        Debug.Log("Changing Element (action): "+element);
    }

    #region Private Functions
    private void ActivateBasicAttack()
    {
        animator.SetBool("isBasicAttacking", true);

        //Calculate direction for attack by intersecting mouse ray with selectable objects on raycastable layer.
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        int mainRaycastMask = 1 << 6; // Mask to just the main Raycast layer, so we only find hits to objects in that layer.

        RaycastHit hitInfo;

        if (Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, mainRaycastMask))
        {
            Debug.Log("Hit object is: " + hitInfo.collider.name);
            this.transform.LookAt(new Vector3(hitInfo.point.x, 1, hitInfo.point.z));
        }

        if (currentElement == Element.None)
        {
            this.currentAttackProjectile = PhotonNetwork.Instantiate(this.defaultAttackPrefab.name, this.transform.position + Vector3.up * this.attackProjectileOffset, this.transform.rotation);
            this.currentAttackProjectile.GetComponent<ProjectileMovement>().SetLifetime(attackProjectileLifetime);
        }
        else
        {
            this.currentAttackProjectile = PhotonNetwork.Instantiate(this.elementalAttackPrefabs[(int)currentElement].name, this.transform.position + Vector3.up * this.attackProjectileOffset, this.transform.rotation);
            this.currentAttackProjectile.GetComponent<ProjectileMovement>().SetLifetime(attackProjectileLifetime);
        }
    }

    private void MoveCharacter()
    {
        // Get Horizontal and Vertical Input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate the Direction to Move based on the tranform of the Player
        Vector3 moveDirectionForward = Vector3.forward * verticalInput;
        Vector3 moveDirectionSide = Vector3.right * horizontalInput;

        //find the direction
        Vector3 direction = (moveDirectionForward + moveDirectionSide).normalized;
        direction = Quaternion.Euler(0, -45, 0) * direction;

        //cue animator bools for walking
        if (direction != Vector3.zero)
        {
            animator.SetBool("isMoving", true);
            this.transform.forward = direction;
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

        //find the distance
        Vector3 distance = this.transform.forward * direction.magnitude * walkSpeed * Time.deltaTime;
        
        if (immobile)
        {
            distance = Vector3.zero;
        }

        //Apply gravity to distance
        distance.y -= gravity * Time.deltaTime;

        // Apply Movement to Player
        controller.Move(distance);
    }

    #endregion

    #region Animation Events

    public void FinishBasicAttack()
    {
        animator.SetBool("isBasicAttacking", false);

        if (photonView.IsMine)
        {
            playerUI.UnshadeIcon(SkillUI.PRIMARY);
        }

        this.immobile = false;
    }

    public void FinishSecondarySkill() {
        animator.SetBool("isSecondarySkilling", false);

        if (photonView.IsMine) {
            playerUI.UnshadeIcon(SkillUI.SECONDARY);
        }

        this.immobile = false;
    }

    public void FinishUltimate() {
        animator.SetBool("isUltimating", false);

        if (photonView.IsMine) {
            playerUI.UnshadeIcon(SkillUI.ULTIMATE);
        }

        this.immobile = false;
    }

    #endregion
}
