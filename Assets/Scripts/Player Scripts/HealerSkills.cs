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

    #region Animation variables
    // Used to tell how long the secondary/ultimate skills take
    [SerializeField]
    private AnimationClip[] secondarySkillClips;
    [SerializeField]
    private AnimationClip ultimateClip;
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
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
    }

    #endregion

    #region IPlayerSkills Implementation
    public void ActivateSkill()
    {
        Debug.Log("Healer secondary skill activated");
        animator.SetBool("isSecondarySkilling", true);

        // Calculate total length of secondary skill
        float secondarySkillClipLength = 0;
        foreach (AnimationClip secondarySkillClip in secondarySkillClips) {
            secondarySkillClipLength += secondarySkillClip.length;
        }
        actionCoreScript.Invoke("FinishSecondarySkillLogic", secondarySkillClipLength);
    }

    public void ActivateUltimate()
    {
        Debug.Log("AOE heal ability pressed");
        animator.SetBool("isUltimating", true);

        actionCoreScript.Invoke("FinishUltimateLogic", ultimateClip.length);
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
    #endregion

}
