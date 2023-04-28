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
    // Store camera direction along x-z plane to calculate movement and ability direction
    private Vector3 cameraDirection;

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
    
    #region Cooldown Variables
    private bool isPrimaryCooldown = false;
    private bool isSecondaryCooldown = false;
    private bool isUltimateCooldown = false;
    
    [SerializeField] private CooldownData primaryCooldown;
    [SerializeField] private CooldownData secondaryCooldown;
    [SerializeField] private CooldownData ultimateCooldown;
    #endregion

    #region Dash Variables
    // Dash direction, is not null when in middle of dash
    private Vector3 dashDirection = Vector3.zero;
    private float dashSpeed = 50f;
    // Maximum dash time in seconds
    private float dashTimeMax = 0.1f;
    // Current amount of time spent dashing.
    private float dashTimeCurrent = 0f;
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
            
            playerUI.ResetCooldown(SkillUI.PRIMARY);
            playerUI.ResetCooldown(SkillUI.SECONDARY);
            playerUI.ResetCooldown(SkillUI.ULTIMATE);
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
            this.cameraDirection = this.gameObject.GetComponent<CameraWork>().GetCameraForward();
            this.cameraDirection.y = 0;

            if (Input.GetButtonDown("Fire1") && currentAttackProjectile == null && !isPrimaryCooldown)
            {
                immobile = true;
                this.transform.forward = this.cameraDirection;

                if (photonView.IsMine)
                {
                    playerUI.ShadeIcon(SkillUI.PRIMARY);
                }
                isPrimaryCooldown = true;
                primaryCooldown = new CooldownData(Time.time, this.GetComponent<IPlayerSkills>().GetCooldown()[0]);
                skills.ActivateBasicAttack();
            } else if (Input.GetButtonDown("Fire2") && !isSecondaryCooldown)
            {
                immobile = true;

                if (photonView.IsMine)
                {
                    playerUI.ShadeIcon(SkillUI.SECONDARY);
                }

                isSecondaryCooldown = true;
                secondaryCooldown = new CooldownData(Time.time, this.GetComponent<IPlayerSkills>().GetCooldown()[1]);
                skills.ActivateSkill();
            } else if (Input.GetButtonDown("Fire3") && !isUltimateCooldown)
            {
                immobile = true;

                if (photonView.IsMine)
                {
                    playerUI.ShadeIcon(SkillUI.ULTIMATE);
                }

                isUltimateCooldown = true;
                ultimateCooldown = new CooldownData(Time.time, this.GetComponent<IPlayerSkills>().GetCooldown()[2]);
                skills.ActivateUltimate();
            } else
            {
                MoveCharacter();
            }
        }        

        //Shows cooldown bar on UI
        if (isPrimaryCooldown)
        {
            primaryCooldown.calculateTimePassed(Time.time);
            if (photonView.IsMine)
            {
                playerUI.SkillCooldown(SkillUI.PRIMARY, primaryCooldown.getTimePassed());
            }
        }
        if (isSecondaryCooldown)
        {
            secondaryCooldown.calculateTimePassed(Time.time);
            if (photonView.IsMine)
            {
                playerUI.SkillCooldown(SkillUI.SECONDARY, secondaryCooldown.getTimePassed());
            }
        }
        if (isUltimateCooldown)
        {
            ultimateCooldown.calculateTimePassed(Time.time);
            if (photonView.IsMine)
            {
                playerUI.SkillCooldown(SkillUI.ULTIMATE, ultimateCooldown.getTimePassed());
            }
        }

        //resets the cooldowns
        if (isPrimaryCooldown && primaryCooldown.getEndTime() < Time.time)
        {
            isPrimaryCooldown = false;
            if (photonView.IsMine)
            {
                playerUI.ResetCooldown(SkillUI.PRIMARY);
            }
            primaryCooldown.reset();
        }
        if (isSecondaryCooldown && secondaryCooldown.getEndTime() < Time.time)
        {
            isSecondaryCooldown = false;
            if (photonView.IsMine)
            {
                playerUI.ResetCooldown(SkillUI.SECONDARY);
            }
            secondaryCooldown.reset();
        }
        if (isUltimateCooldown && ultimateCooldown.getEndTime() < Time.time)
        {
            isUltimateCooldown = false;
            if (photonView.IsMine)
            {
                playerUI.ResetCooldown(SkillUI.ULTIMATE);
            }
            ultimateCooldown.reset();
        }

        if (!dashDirection.Equals(Vector3.zero))
        {
            Vector3 dashVector = dashDirection * dashSpeed * Time.deltaTime;

            /* Only move character if within bounds */
            if ((this.transform.position + dashVector - Vector3.zero).magnitude < 42.5)
            {
                controller.Move(dashVector);
            }

            dashTimeCurrent += Time.deltaTime;
            if (dashTimeCurrent >= dashTimeMax)
            {
                dashTimeCurrent = 0;
                dashDirection = Vector3.zero;
            }
        }
    }
    #endregion

    #region Public Functions
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

    public void FinishBasicAttackLogic() {
        animator.SetBool("isBasicAttacking", false);

        if (photonView.IsMine)
        {
            playerUI.UnshadeIcon(SkillUI.PRIMARY);
        }

        this.immobile = false;
    }

    public void FinishSecondarySkillLogic() {
        if (photonView.IsMine) {
            playerUI.UnshadeIcon(SkillUI.SECONDARY);
        }

        this.immobile = false;
    }

    public void FinishUltimateLogic() {
        if (photonView.IsMine) {
            playerUI.UnshadeIcon(SkillUI.ULTIMATE);
        }

        this.immobile = false;
    }
    #endregion

    #region Private Functions
    public void ActivateBasicAttack() // Change eventually so all logic is in individual players; used to be private and used for all characters
    {
        animator.SetBool("isBasicAttacking", true);

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
        this.currentAttackProjectile.GetComponent<ProjectileMovement>().SetPlayer(this.gameObject);
    }

    private void MoveCharacter()
    {
        // Get Horizontal and Vertical Input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate the Direction to Move based on the tranform of the Player
        Vector3 moveDirectionForward = this.cameraDirection * verticalInput;
        Vector3 moveDirectionSide = -1 * Vector3.Cross(this.cameraDirection, Vector3.up) * horizontalInput;
        // Normalize the direction
        Vector3 direction = (moveDirectionForward + moveDirectionSide).normalized;

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

        /* Only move character if within bounds */
        if ((this.transform.position + distance - Vector3.zero).magnitude < 42.5)
        {
            controller.Move(distance);
        }
    }

    #endregion

    #region Public Getters/Setters

    public bool SetImmobile(bool immobile)
    {
        return immobile;
    }

    public void SetDashDirection(Vector3 direction)
    {
        this.dashDirection = direction;
    }

    #endregion

    #region Animation Events

    public void FinishSecondarySkillAnimation()
    {
        animator.SetBool("isSecondarySkilling", false);
    }
    public void FinishUltimateAnimation() 
    {
        animator.SetBool("isUltimating", false);
    }

    public void FinishBasicAttack() // TODO: Eventually refactor so that this only finishes the animation?
    {
        animator.SetBool("isBasicAttacking", false);

        if (photonView.IsMine)
        {
            playerUI.UnshadeIcon(SkillUI.PRIMARY);
        }

        this.immobile = false;
    }

    public void FinishSecondarySkill() { // TODO: Delete function after all characters using FinishAnimation & FinishLogic
        animator.SetBool("isSecondarySkilling", false);

        if (photonView.IsMine) {
            playerUI.UnshadeIcon(SkillUI.SECONDARY);
        }

        this.immobile = false;
    }

    public void FinishUltimate() { // TODO: Delete function after all characters using FinishAnimation & FinishLogic
        animator.SetBool("isUltimating", false);

        if (photonView.IsMine) {
            playerUI.UnshadeIcon(SkillUI.ULTIMATE);
        }

        this.immobile = false;
    }

    #endregion
}
