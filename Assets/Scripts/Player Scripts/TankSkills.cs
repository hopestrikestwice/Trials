/// TankSkills.cs
///
/// Script-side implementation of Tank's unique abilities.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class TankSkills : MonoBehaviourPunCallbacks, IPlayerSkills
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
    private GameObject smallShield;
    [SerializeField]
    private GameObject largeShield;
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

        if (!smallShield || !largeShield)
        {
            Debug.LogError("TankSkills is Missing Shield GameObject", this);
        }
        else {
            smallShield.SetActive(false);
            largeShield.SetActive(false);
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
        smallShield.SetActive(true); // enable the small shield, so that it collides with players

        actionCoreScript.Invoke("FinishSecondarySkillLogic", secondarySkillClip.length);
        Invoke("DeactivateSmallShield", secondarySkillClip.length);
    }

    public void ActivateUltimate()
    {
        Debug.Log("Large shield button pressed.");
        animator.SetBool("isUltimating", true);
        largeShield.SetActive(true); // enable the large shield, so that it collides with players

        actionCoreScript.Invoke("FinishUltimateLogic", ultimateClip.length);
        Invoke("DeactivateLargeShield", ultimateClip.length);
    }
    #endregion

    #region Invoked Functions
    /* Functions that deal with additional skill logic that need to be invoked.
    Usually signals the end of a skill */
    public void DeactivateSmallShield()
    {
        smallShield.SetActive(false);
    }

    public void DeactivateLargeShield()
    {
        largeShield.SetActive(false);
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

}
