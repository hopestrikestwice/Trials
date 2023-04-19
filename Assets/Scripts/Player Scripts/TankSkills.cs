/// TankSkills.cs
///
/// Script-side implementation of Tank's unique abilities.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class TankSkills : MonoBehaviourPunCallbacks, IPlayerSkills, IPunObservable
{
    #region Private Fields
    private PlayerUI playerUI;

    private Animator animator;
    private PlayerActionCore actionCoreScript;

    #region Shield Variables
     [Header("Shielding GameObjects")]
    [SerializeField]
    private GameObject shieldSmallParticles;
    [SerializeField]
    private GameObject shieldLargeParticles;
    [SerializeField]
    private GameObject smallShieldCollider;
    [SerializeField]
    private GameObject largeShieldCollider;
    [SerializeField]
    private GameObject smallShieldVfx;
    [SerializeField]
    private GameObject largeShieldVfx;
    // Keep track of moving shield colliders up / down when enabled / disabled
    private const float shieldEnableTime = 0.1f;
    private bool smallShieldEnabled = false;
    private float smallShieldStartPosY;
    private float smallShieldEndPosY;
    private bool largeShieldEnabled = false;
    private float largeShieldStartPosY;
    private float largeShieldEndPosY;
    private const float startVfxDisolve = 1f;
    private const float endVfxDisolve = 0f;

    #endregion

    #region Animation variables
     [Header("Animation Clips")]
    // Used to tell how long the secondary/ultimate skills take
    [SerializeField]
    private AnimationClip secondarySkillClip;
    [SerializeField]
    private AnimationClip ultimateClip;
    #endregion

    #endregion

    #region Monobehaviour

    private void Update()
    {
        // Handle moving shield collider up / down
        Vector3 smallShieldPos = smallShieldCollider.transform.position;
        if (smallShieldEnabled && smallShieldPos.y < this.transform.position.y + smallShieldEndPosY) {
            float up_dist = smallShieldEndPosY - smallShieldStartPosY;
            float up_increment = up_dist / shieldEnableTime;
            smallShieldPos.y += up_increment * Time.deltaTime;         
        }
        else if (!smallShieldEnabled && smallShieldPos.y > this.transform.position.y + smallShieldStartPosY) {
            float down_dist = smallShieldEndPosY - smallShieldStartPosY;
            float down_increment = down_dist / shieldEnableTime;
            smallShieldPos.y -= down_increment * Time.deltaTime;
        }
        smallShieldCollider.transform.position = smallShieldPos;
        

        Vector3 largeShieldPos = largeShieldCollider.transform.position;
        if (largeShieldEnabled && largeShieldPos.y < this.transform.position.y + largeShieldEndPosY) {
            float up_dist = largeShieldEndPosY - largeShieldStartPosY;
            float up_increment = up_dist / shieldEnableTime;
            largeShieldPos.y += up_increment * Time.deltaTime;
        }
        else if (!largeShieldEnabled && largeShieldPos.y > this.transform.position.y + largeShieldStartPosY) {
            float down_dist = largeShieldEndPosY - largeShieldStartPosY;
            float down_increment = down_dist / shieldEnableTime;
            largeShieldPos.y -= down_increment * Time.deltaTime;
        }
        largeShieldCollider.transform.position = largeShieldPos;

        // Handle vfx for shield
        float disolve_increment = (endVfxDisolve - startVfxDisolve) /  shieldEnableTime;

        Material smallShieldMaterial = smallShieldVfx.GetComponent<Renderer>().material;
        float smallShieldDisolve = smallShieldMaterial.GetFloat("_Disolve");
        if (smallShieldEnabled && smallShieldDisolve > endVfxDisolve) {
            float newDisolve = smallShieldDisolve + disolve_increment * Time.deltaTime;
            smallShieldMaterial.SetFloat("_Disolve", newDisolve);
        }
        else if (!smallShieldEnabled && smallShieldDisolve < startVfxDisolve) {
            float newDisolve = smallShieldDisolve - disolve_increment * Time.deltaTime;
            smallShieldMaterial.SetFloat("_Disolve", newDisolve);
        }

        Material largeShieldMaterial = largeShieldVfx.GetComponent<Renderer>().material;
        float largeShieldDisolve = largeShieldMaterial.GetFloat("_Disolve");
        if (largeShieldEnabled && largeShieldDisolve > endVfxDisolve) {
            float newDisolve = largeShieldDisolve + disolve_increment * Time.deltaTime;
            largeShieldMaterial.SetFloat("_Disolve", newDisolve);
        }
        else if (!largeShieldEnabled && largeShieldDisolve < startVfxDisolve) {
            float newDisolve = largeShieldDisolve - disolve_increment * Time.deltaTime;
            largeShieldMaterial.SetFloat("_Disolve", newDisolve);
        }
    }

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

        if (!smallShieldCollider || !largeShieldCollider || !smallShieldVfx || !largeShieldVfx)
        {
            Debug.LogError("TankSkills is Missing Shield GameObject", this);
        }
        else {
            smallShieldEnabled = false;
            largeShieldEnabled = false;
            smallShieldStartPosY = smallShieldCollider.transform.localPosition.y;
            largeShieldStartPosY = largeShieldCollider.transform.localPosition.y;
            smallShieldEndPosY = smallShieldVfx.transform.localPosition.y;
            largeShieldEndPosY = largeShieldVfx.transform.localPosition.y;
            Debug.Log("smallShieldStart: " + smallShieldStartPosY + ", smallShieldEnd: " + smallShieldEndPosY);
        }
        if (!shieldSmallParticles || !shieldLargeParticles)
        {
            Debug.LogError("TankSkills is Missing Shield Particles", this);
        }
        else {
            shieldSmallParticles.GetComponent<ParticleSystem>().enableEmission = false;
            shieldLargeParticles.GetComponent<ParticleSystem>().enableEmission = false;
        }

        actionCoreScript = GetComponent<PlayerActionCore>();
        if (!actionCoreScript)
        {
            Debug.LogError("TankSkills is Missing PlayerActionCore.cs");
        }
        if (!secondarySkillClip)
        {
            Debug.LogError("TankSkills is Missing Secondary Skill Animation Clip");
        }
        if (!ultimateClip)
        {
            Debug.LogError("TankSkills is Missing Ultimate Animation Clip");
        }
    }

    #endregion

    #region IPlayerSkills Implementation

    public void ActivateSkill()
    {
        Debug.Log("Shield button pressed.");
        animator.SetBool("isSecondarySkilling", true);
        smallShieldEnabled = true; // enable the small shield, so that it collides with players

        actionCoreScript.Invoke("FinishSecondarySkillLogic", secondarySkillClip.length);
        Invoke("DeactivateSmallShield", secondarySkillClip.length);
    }

    public void ActivateUltimate()
    {
        Debug.Log("Large shield button pressed.");
        animator.SetBool("isUltimating", true);
        largeShieldEnabled = true; // enable the large shield, so that it collides with players

        actionCoreScript.Invoke("FinishUltimateLogic", ultimateClip.length);
        Invoke("DeactivateLargeShield", ultimateClip.length);
    }
    #endregion

    #region Invoked Functions
    /* Functions that deal with additional skill logic that need to be invoked.
    Usually signals the end of a skill */
    public void DeactivateSmallShield()
    {
        smallShieldEnabled = false;
    }

    public void DeactivateLargeShield()
    {
        largeShieldEnabled = false;
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
    /* Secondary Skill Animation Events */
    public void StartSmallShieldParticles()
    {
        shieldSmallParticles.GetComponent<ParticleSystem>().enableEmission = true;
    }
    public void FinishSmallShieldParticles()
    {
        shieldSmallParticles.GetComponent<ParticleSystem>().enableEmission = false;
    }
    /* Ultimate Skill Animation Events */
    public void StartLargeShieldParticles()
    {
        shieldLargeParticles.GetComponent<ParticleSystem>().enableEmission = true;
    }
    public void FinishLargeShieldParticles()
    {
        shieldLargeParticles.GetComponent<ParticleSystem>().enableEmission = false;
    }
    #endregion

    #region IPunObservable Implementation
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send others our data
            stream.SendNext(this.smallShieldEnabled);
            stream.SendNext(this.largeShieldEnabled);
        }
        else 
        {
            // Network player, receive data
            this.smallShieldEnabled = (bool)stream.ReceiveNext();
            this.largeShieldEnabled = (bool)stream.ReceiveNext();
        }
    }
    #endregion

}
