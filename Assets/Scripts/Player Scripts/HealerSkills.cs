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

    #region VFX variables
    [Header("VFX GameObjects")]
    [SerializeField]
    private GameObject lightningParticles;
    private bool lightningEnabled = false;
    #endregion

    private float[] cooldown = { 1.5f, 5, 5 };

    #region Basic Attack Variables
    [SerializeField]
    private int basicAttackDamage;
    [SerializeField]
    private GameObject basicAttackVfx;
    private float basicAttackDelay = 0.4f;
    #endregion

    #region Animation variables
    // Used to tell how long the secondary/ultimate skills take
    [SerializeField]
    private AnimationClip basicAttackClip;
    [SerializeField]
    private AnimationClip[] secondarySkillClips;
    private float secondarySkillClipLength;
    [SerializeField]
    private AnimationClip ultimateClip;
    #endregion

    #region Attack Variables
    private int currentSignatureCharges = 5;
    [SerializeField]
    private int maxSignatureCharges = 5;
    [SerializeField]
    private int maxSignatureHeal = 50;
    [SerializeField]
    private int maxSignatureDamage = 50;


    [SerializeField]
    private GameObject lightningBolt;
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
        foreach (AnimationClip secondarySkillClip in secondarySkillClips)
        {
            secondarySkillClipLength += secondarySkillClip.length;
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
        // lightningParticles.SetActive(lightningEnabled);
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
        Debug.Log("Healer secondary skill activated");

        Vector3 cameraDirection = this.gameObject.GetComponent<CameraWork>().GetCameraForward();
        cameraDirection.y = 0;
        this.transform.forward = cameraDirection;

        animator.SetBool("isSecondarySkilling", true);

        StartCoroutine(SecondarySkill());
        //isBlocked = true;

        //// Calculate total length of secondary skill
        //float secondarySkillClipLength = 0;
        //foreach (AnimationClip secondarySkillClip in secondarySkillClips) {
        //    secondarySkillClipLength += secondarySkillClip.length;
        //}
        //actionCoreScript.Invoke("FinishSecondarySkillLogic", secondarySkillClipLength);
    }

    public void ActivateUltimate()
    {
        if (currentSignatureCharges > 0) {
            Debug.Log("AOE heal ability pressed");

            Vector3 cameraDirection = this.gameObject.GetComponent<CameraWork>().GetCameraForward();
            cameraDirection.y = 0;
            this.transform.forward = cameraDirection;

            animator.SetBool("isUltimating", true);
            DoSignature();
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
        RaycastHit hitInfo;

        if (Physics.Raycast(this.transform.position + Vector3.up, this.transform.forward, out hitInfo, 6f))
        {
            this.photonView.RPC("StartLightningParticles", RpcTarget.All);

            if (hitInfo.collider.CompareTag("Player"))
            {
                int healAmount = currentSignatureCharges * maxSignatureHeal / maxSignatureCharges;
                hitInfo.collider.GetComponent<PhotonView>().RPC("HealPlayer", RpcTarget.Others, healAmount);
                this.GetComponent<PlayerManagerCore>().HealPlayer(healAmount);
            }
            else
            {
                /* hit boss here */
                int damageAmount = currentSignatureCharges * maxSignatureDamage / maxSignatureCharges;
                hitInfo.collider.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.MasterClient, damageAmount, this.GetComponent<PlayerManagerCore>().GetElement());
                int healAmount = currentSignatureCharges * maxSignatureHeal / maxSignatureCharges;
                this.GetComponent<PlayerManagerCore>().HealPlayer(healAmount);
            }

        } else
        {
            /* Hit nothing, do nothing */
        }

        currentSignatureCharges = 0;

        ////Calculate direction for attack by intersecting mouse ray with selectable objects on raycastable layer.
        //Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        //// Ray mouseRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        //int mainRaycastMask = 1 << 6; // Mask to just the main Raycast layer, so we only find hits to objects in that layer.

        //RaycastHit hitInfo;

        //if (Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, mainRaycastMask))
        //{
        //    this.transform.LookAt(new Vector3(hitInfo.point.x, 1, hitInfo.point.z));
        //    Debug.Log("Hit: "+hitInfo.collider.name);
        //}

        //object[] myCustomInitData = new object[]
        //{
        //    GetCharge()
        //};

        //bullet = PhotonNetwork.Instantiate(this.defaultBulletPrefab.name, this.transform.position + Vector3.up * bulletOffset, this.transform.rotation, 0, myCustomInitData);
        //bullet.GetComponent<HealerProjectile>().SetLifetime(bulletLifetime);
        //bullet.GetComponent<HealerProjectile>().SetPlayer(this.gameObject);
    }

    public int GetCharge()
    {
        return this.currentSignatureCharges;
    }

    public int GetMaxCharge()
    {
        return this.maxSignatureCharges;
    }

    //Increments signature charge
    public void AddCharge()
    {
        if (this.currentSignatureCharges < maxSignatureCharges) this.currentSignatureCharges++;
        
        Debug.Log("Added charge - total charge: "+this.currentSignatureCharges);
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
    public void StartLightningBolt()
    {
        this.lightningBolt.SetActive(true);
    }

    public void FinishLightningBolt()
    {
        this.lightningBolt.SetActive(false);
    }

    /* Ultimate Skill Animation Events */
    public void FinishLightningParticles()
    {
        lightningParticles.GetComponent<ParticleSystem>().enableEmission = false;
        Debug.Log("Lightning Disabled");
    }
    public void PlayHealerImpactVfx() {
        basicAttackVfx.gameObject.SetActive(false);
        basicAttackVfx.gameObject.SetActive(true);

        if (photonView.IsMine)
        {
            RaycastHit hitInfo;

            /* Hit range is based on vfx distance */
            Debug.Log("distance: " + basicAttackVfx.transform.localPosition.z);
            if (Physics.Raycast(this.transform.position + Vector3.up, this.transform.forward, out hitInfo, basicAttackVfx.transform.localPosition.z))
            {
                if (hitInfo.collider.CompareTag("BossTentacle"))
                {
                    /* hit boss here */
                    hitInfo.collider.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.MasterClient, basicAttackDamage, this.GetComponent<PlayerManagerCore>().GetElement());
                    this.AddCharge();
                }

            }
            else
            {
                /* Hit nothing, do nothing */
            }
        }
    }

    #endregion

    #region RPCs
    [PunRPC]
    public void StartLightningParticles()
    {
        lightningParticles.GetComponent<ParticleSystem>().enableEmission = true;
        Debug.Log("Lightning Enabled");
    }
    #endregion

    #region Private Methods
    IEnumerator SecondarySkill()
    {
        // Start skill
        GetComponent<PlayerManagerCore>().SetShielded(true);

        // Wait until skill is over
        yield return new WaitForSeconds(secondarySkillClipLength);
        // End skill
        GetComponent<PlayerManagerCore>().SetShielded(false);
        actionCoreScript.FinishSecondarySkill();
    }
    #endregion

}
