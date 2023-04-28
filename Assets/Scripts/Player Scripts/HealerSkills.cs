/// HealerSkills.cs
///
/// Script-side implementation of Healer's unique abilities.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class HealerSkills : MonoBehaviourPun, IPlayerSkills
{
    #region Private Fields
    private PlayerUI playerUI;
    private Animator animator;
    private PlayerActionCore actionCoreScript;
    private GameObject healer;

    #region VFX variables
    [Header("VFX GameObjects")]
    [SerializeField]
    private GameObject lightningParticles;
    private bool lightningEnabled = false;
    #endregion

    private float[] cooldown = { 5, 5, 5 };

    #region Animation variables
    // Used to tell how long the secondary/ultimate skills take
    [SerializeField]
    private AnimationClip[] secondarySkillClips;
    [SerializeField]
    private AnimationClip ultimateClip;
    #endregion

    #region Attack Variables
    [SerializeField]
    private GameObject defaultBulletPrefab;
    [SerializeField]
    private GameObject bullet;

    private float bulletOffset = 1f;
    private float bulletLifetime = 1f;
    private int sigCharge = 0;
    private bool isBlocked = false;
    #endregion

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

        actionCoreScript = GetComponent<PlayerActionCore>();
        if (!actionCoreScript)
        {
            Debug.LogError("HealerSkills is Missing PlayerActionCore.cs");
        }
        if (secondarySkillClips.Length == 0)
        {
            Debug.LogError("HealerSkills is Missing Secondary Skill Animation Clip");
        }
        if (!ultimateClip)
        {
            Debug.LogError("HealerSkills is Missing Ultimate Animation Clip");
        }

        if (!lightningParticles)
        {
            Debug.LogError("HealerSkills is Missing Lightning Particles", this);
        }
        else {
            lightningParticles.GetComponent<ParticleSystem>().enableEmission = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        if (!animator.GetBool("isSecondarySkilling"))
        {
            this.isBlocked = false;
        }
        // lightningParticles.SetActive(lightningEnabled);
    }

    #endregion

    #region IPlayerSkills Implementation
    public void ActivateSkill()
    {
        Debug.Log("Healer secondary skill activated");

        Vector3 cameraDirection = this.gameObject.GetComponent<CameraWork>().GetCameraForward();
        cameraDirection.y = 0;
        this.transform.forward = cameraDirection;

        animator.SetBool("isSecondarySkilling", true);
        isBlocked = true;

        // Calculate total length of secondary skill
        float secondarySkillClipLength = 0;
        foreach (AnimationClip secondarySkillClip in secondarySkillClips) {
            secondarySkillClipLength += secondarySkillClip.length;
        }
        actionCoreScript.Invoke("FinishSecondarySkillLogic", secondarySkillClipLength);
    }

    public void ActivateUltimate()
    {
        if (GetCharge() > 0) {
            Debug.Log("AOE heal ability pressed");

            Vector3 cameraDirection = this.gameObject.GetComponent<CameraWork>().GetCameraForward();
            cameraDirection.y = 0;
            this.transform.forward = cameraDirection;

            animator.SetBool("isUltimating", true);
            DoSignature();
            ResetCharge();
            actionCoreScript.Invoke("FinishUltimateLogic", ultimateClip.length);
        }
        else {
            Debug.Log("Healer not enough charges");
            this.GetComponent<PlayerActionCore>().setImmobile(false);
            playerUI.UnshadeIcon(SkillUI.ULTIMATE);
        }
    }

    #endregion

    #region Public Methods
    public void DoSignature()
    {
        //Calculate direction for attack by intersecting mouse ray with selectable objects on raycastable layer.
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        // Ray mouseRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        int mainRaycastMask = 1 << 6; // Mask to just the main Raycast layer, so we only find hits to objects in that layer.

        RaycastHit hitInfo;

        if (Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, mainRaycastMask))
        {
            this.transform.LookAt(new Vector3(hitInfo.point.x, 1, hitInfo.point.z));
            Debug.Log("Hit: "+hitInfo.collider.name);
        }

        object[] myCustomInitData = new object[]
        {
            GetCharge()
        };

        bullet = PhotonNetwork.Instantiate(this.defaultBulletPrefab.name, this.transform.position + Vector3.up * bulletOffset, this.transform.rotation, 0, myCustomInitData);
        bullet.GetComponent<HealerProjectile>().SetLifetime(bulletLifetime);
        bullet.GetComponent<HealerProjectile>().SetPlayer(this.gameObject);
    }

    //Increments signature charge
    public void AddCharge()
    {
        if (this.sigCharge < 5) this.sigCharge++;
        Debug.Log("Added charge - total charge: "+this.sigCharge);
    }

    public void ResetCharge()
    {
        this.sigCharge = 0;
    }

    public int GetCharge()
    {
        return this.sigCharge;
    }

    #endregion

    #region Private Methods

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("BossTentacle") || other.CompareTag("BossProjectile")) && isBlocked)
        {
            Debug.Log("Boss atk blocked by healer");
            AddCharge();
            //Negates damage from atk
            this.GetComponent<PlayerManagerCore>().HealPlayer(20);
        }
    }

    #endregion
    
    public float[] GetCooldown()
    {
        return cooldown;
    }

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

    /* Ultimate Skill Animation Events */
    public void StartLightningParticles()
    {
        lightningParticles.GetComponent<ParticleSystem>().enableEmission = true;
        Debug.Log("Lightning Enabled");
    }
    public void FinishLightningParticles()
    {
        lightningParticles.GetComponent<ParticleSystem>().enableEmission = false;
        Debug.Log("Lightning Disabled");
    }

    #endregion

}
