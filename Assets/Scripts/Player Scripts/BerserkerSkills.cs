/// BerserkerSkills.cs
///
/// Script-side implementation of Berserker's unique abilities.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class BerserkerSkills : MonoBehaviourPun, IPlayerSkills
{
    #region Private Fields
    private PlayerUI playerUI;

    private Animator animator;
    private PlayerActionCore actionCoreScript;

    #region Basic Attack Variables
    [SerializeField]
    private GameObject basicAttackVfx;
    #endregion

    #region Attack Variables
    [SerializeField]
    private int basicAttackDamage;
    private float chargedBulletLifetime = 0.5f;

    [SerializeField]
    private int chargedAttackMaxDamage;
    private int unchargedPenalty = 5;
    private float charge = 0f;
    private float maxCharge = 3f; // how long it takes to max charge in seconds

    private bool isCharging = false;

    private CharacterController controller;
    private float slamForwardSpeed = 16f;
    private bool isUltimatingMoveForward = false;

    private float[] cooldown = {1, 2, 15};
    #endregion

    #region Animation variables
    // Used to tell how long the secondary/ultimate skills take
    [SerializeField]
    private AnimationClip basicAttackClip;
    [SerializeField]
    private AnimationClip secondarySkillClip;
    [SerializeField]
    private AnimationClip ultimateClip;
    private BerserkerShootSlash shootSlashScript;
    #endregion

    private Color originalBaseColor;

    #endregion

    #region Monobehaviour Callbacks
    private void Start()
    {
        if (photonView.IsMine)
        {
            this.playerUI = this.GetComponent<PlayerManagerCore>().getPlayerUI();
        }

        // Animator
        animator = GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("BeserkerSkills is Missing Animator Component", this);
        }

        // Basic Attack Objects
        if (!basicAttackVfx) 
        {
            Debug.LogError("BerserkerSkills is Missing Basic Attack VFX");
        }

        // Scripts 
        shootSlashScript = GetComponent<BerserkerShootSlash>();
        if (!shootSlashScript)
        {
            Debug.LogError("BerserkerSkills is Missing BerserkerShootSlash.cs", this);
        }
        actionCoreScript = GetComponent<PlayerActionCore>();
        if (!actionCoreScript)
        {
            Debug.LogError("BeserkerSkills is Missing PlayerActionCore.cs");
        }

        // Animation clips
        if (!basicAttackClip)
        {
            Debug.LogError("BerserkerSkills is Missing Basic Attack Animation Clip");
        }
        if (!secondarySkillClip)
        {
            Debug.LogError("BeserkerSkills is Missing Secondary Skill Animation Clip");
        }
        if (!ultimateClip)
        {
            Debug.LogError("BeserkerSkills is Missing Ultimate Animation Clip");
        }

        this.controller = this.GetComponent<CharacterController>();
        if (!controller)
        {
            Debug.LogError("PlayerActionCore is missing CharacterController Component", this);
        }

        originalBaseColor = this.transform.Find("Character Body").GetComponent<Renderer>().material.GetColor("_BaseColor");
    }

    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        if (isCharging)
        {
            HandleAttack();
        }

        if (isUltimatingMoveForward)
        {
            Vector3 distance = this.transform.forward * slamForwardSpeed * Time.deltaTime;

            /* Only move character if within bounds */
            if ((this.transform.position + distance - Vector3.zero).magnitude < 42.5)
            {
                controller.Move(distance);
            }
        }
    }
    #endregion

    #region IPlayerSkills Implementation
    public void ActivateBasicAttack()
    {
        animator.SetBool("isBasicAttacking", true);

        actionCoreScript.Invoke("FinishBasicAttackLogic", basicAttackClip.length);
    }

    public void ActivateSkill()
    {
        Debug.Log("Berserker Shield");

        /* Block in camera direction, not where character is currently facing */
        Vector3 cameraDirection = this.gameObject.GetComponent<CameraWork>().GetCameraForward();
        cameraDirection.y = 0;
        this.transform.forward = cameraDirection;

        animator.SetBool("isSecondarySkilling", true);

        StartCoroutine(SecondarySkill());
    }

    public void ActivateUltimate()
    {
        isCharging = true;
    }

    public float[] GetCooldown()
    {
        return cooldown;
    }

    #endregion

    #region Private Methods

    IEnumerator SecondarySkill() {
        // Start skill
        GetComponent<PlayerManagerCore>().SetShielded(true);
        
        // Wait until skill is over
        yield return new WaitForSeconds(secondarySkillClip.length);
        // End skill
        GetComponent<PlayerManagerCore>().SetShielded(false);
        actionCoreScript.FinishSecondarySkill();
    }

    /* not properly named, but this is for the ultimate (charged) attack */
    private void HandleAttack()
    {
        if ((Input.GetButtonDown("Fire3") || Input.GetButton("Fire3")))
        {
            if (charge < maxCharge)
            {
                charge += Time.deltaTime;
                this.transform.Find("Character Body").GetComponent<Renderer>().material.SetColor("_BaseColor", Color.Lerp(originalBaseColor, Color.red / 2, charge / maxCharge));
            } else {
                this.transform.Find("Character Body").GetComponent<Renderer>().material.SetColor("_BaseColor", Color.red);
            }
        }
            
        if (Input.GetButtonUp("Fire3")) // Does GetButtonUp imply Getbuttondown was called? And vice versa?
        {
            Vector3 cameraDirection = this.gameObject.GetComponent<CameraWork>().GetCameraForward();
            cameraDirection.y = 0;
            this.transform.forward = cameraDirection;

            //Animations and fire attack
            animator.SetBool("isUltimating", true);
            isCharging = false;

            isUltimatingMoveForward = true;

            actionCoreScript.Invoke("FinishUltimateLogic", ultimateClip.length);
            this.Invoke("FinishUltimateMoveForward", ultimateClip.length / 3); // The berserker doesn't actually jump the full length of the ultimateClip animation.
        }
    }

    private void FinishUltimateMoveForward()
    {
        isUltimatingMoveForward = false;

        if (charge >= maxCharge)
        {
            Debug.Log("Berserker MaxCharge Fire!");
            shootSlashScript.InstantiateProjectile(chargedAttackMaxDamage, chargedBulletLifetime * 2);
        }
        else
        {
            shootSlashScript.InstantiateProjectile(chargedAttackMaxDamage / unchargedPenalty, chargedBulletLifetime);
        }

        this.transform.Find("Character Body").GetComponent<Renderer>().material.SetColor("_BaseColor", originalBaseColor);
        charge = 0f;
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
    public void PlayImpactVfx() {
        basicAttackVfx.gameObject.SetActive(false);
        basicAttackVfx.gameObject.SetActive(true);

        if (photonView.IsMine)
        {
            /* check for hit if we are the owning client */
            RaycastHit hitInfo;

            /* Hit range is based on vfx distance */
            Debug.Log("distance: " + basicAttackVfx.transform.localPosition.z);
            if (Physics.Raycast(this.transform.position + Vector3.up, this.transform.forward, out hitInfo, basicAttackVfx.transform.localPosition.z))
            {
                if (hitInfo.collider.CompareTag("BossTentacle"))
                {
                    /* hit boss here */
                    hitInfo.collider.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.MasterClient, basicAttackDamage, this.GetComponent<PlayerManagerCore>().GetElement());
                }

            }
            else
            {
                /* Hit nothing, do nothing */
            }
        }
    }
    #endregion
}
