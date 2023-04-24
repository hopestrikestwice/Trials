/// SupportSkills.cs
///
/// Script-side implementation of Support's unique abilities.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class SupportSkills : MonoBehaviourPun, IPlayerSkills
{
    #region Private Fields
    private PlayerUI playerUI;

    private Animator animator;
    private PlayerActionCore actionCoreScript;
    private PlayerManagerCore managerCoreScript;

    #region Ultimate Variables
    private Element currentElement = Element.Fire;
    private const float fogDuration = 5f;
    #endregion

    #region Animation variables
    // Used to tell how long the secondary/ultimate skills take
    [SerializeField]
    private AnimationClip secondarySkillClip;
    [SerializeField]
    private AnimationClip ultimateClip;
    #endregion

    private float[] cooldown = { 5, 5, 5 };

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
            Debug.LogError("SupportSkills is Missing Animator Component", this);
        }

        actionCoreScript = GetComponent<PlayerActionCore>();
        if (!actionCoreScript)
        {
            Debug.LogError("SupportSkills is Missing PlayerActionCore.cs");
        }

        managerCoreScript = GetComponent<PlayerManagerCore>();
        if (!managerCoreScript)
        {
            Debug.LogError("SupportSkills is Missing PlayerManagerCore.cs");
        }

        if (!secondarySkillClip)
        {
            Debug.LogError("SupportSkills is Missing Secondary Skill Animation Clip");
        }
        if (!ultimateClip)
        {
            Debug.LogError("SupportSkills is Missing Ultimate Animation Clip");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        
        // if (isUltimatingSup)
        // {
        //     isUltimatingSup = false;
        // }
    }

    #endregion


    #region IPlayerSkills Implementation

    public void ActivateSkill()
    {
        Debug.Log("Dash button pressed.");
        animator.SetBool("isSecondarySkilling", true);

        // Handle dashing logic
        Vector3 dashDirection = this.transform.forward;
        if (managerCoreScript.getIsFogged()) {
            GetComponent<PhotonView>().RPC("DashAll", RpcTarget.All, new object [] { dashDirection });
        }
        else
        {
            // Dash only the support
            actionCoreScript.SetDashDirection(dashDirection);
        }
        
        actionCoreScript.Invoke("FinishSecondarySkillLogic", secondarySkillClip.length);
    }

    public void ActivateUltimate()
    {
        Debug.Log("Change element button pressed");

        animator.SetBool("isUltimating", true);
        StartCoroutine(CreateFog());

        actionCoreScript.Invoke("FinishUltimateLogic", ultimateClip.length);
        Invoke("FinishChannelingElement", ultimateClip.length);

        // mostRecentElement = ElementFunctions.NextElement(mostRecentElement);
        // this.GetComponent<PlayerManagerCore>().SetElement(mostRecentElement);
        // isUltimatingSup = true;
    }

    public float[] GetCooldown()
    {
        return cooldown;
    }

    #endregion

    #region Coroutines
    IEnumerator CreateFog() 
    {
        // Create fog
        Vector3 fogLocation = new Vector3(this.transform.position.x, 0, this.transform.position.z);
        GameObject fogObject = PhotonNetwork.Instantiate("Fog", fogLocation, Quaternion.identity, 0);
        // Set the fog's element
        GiveElement fogElement = fogObject.GetComponent<GiveElement>();
        if (!fogElement) Debug.LogError("Fog object does not have GiveElement script");
        else fogElement.setElement(currentElement);

        yield return new WaitForSeconds(fogDuration);

        // End fog
        fogObject.transform.position += new Vector3 (0, -20, 0); // Janky way to ensure players leave the trigger before destroy
        yield return new WaitForSeconds(0.2f);
        PhotonNetwork.Destroy(fogObject);
        // Change element for next fog
        currentElement = ElementFunctions.NextElement(currentElement);
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

    public void FinishChannelingElement()
    {
        Debug.Log("Finish Attack");
    }

    #endregion

    #region PUN RPC
    [PunRPC]
    public void DashAll(Vector3 direction)
    {
        Debug.Log("Dash all players");
        // Dash all characters in fog
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players) {
            bool playerFogged = player.GetComponent<PlayerManagerCore>().getIsFogged();
            if (playerFogged) player.GetComponent<PlayerActionCore>().SetDashDirection(direction);
        }
    }
    #endregion
}
