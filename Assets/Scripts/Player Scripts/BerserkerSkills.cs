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

    #region Secondary Skill Variables
    [SerializeField]
    private GameObject secondarySkillShield;
    #endregion

    #region Attack Variables
    [SerializeField]
    private GameObject defaultBulletPrefab;
    [SerializeField]
    private GameObject chargedBulletPrefab;
    private GameObject bullet;

    private float bulletOffset = 1f;
    private float bulletLifetime = 1f;

    private float charge = 0f;
    private float maxCharge = 1f;

    private bool isCharging = false;
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
            Debug.LogError("BeserkerSkills is Missing Animator Component", this);
        }
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
    }
    #endregion

    #region IPlayerSkills Implementation

    public void ActivateSkill()
    {
        Debug.Log("Berserker Shield");
        animator.SetBool("isSecondarySkilling", true);
    }

    public void ActivateUltimate()
    {
        if (bullet == null)
        {
            isCharging = true;
        } else {
            if (photonView.IsMine)
            {
                playerUI.UnshadeIcon(SkillUI.ULTIMATE);
            }
            this.gameObject.GetComponent<PlayerActionCore>().setImmobile(false);
        }
    }
    #endregion

    #region Private Methods

    /* not properly named, but this is for the ultimate (charged) attack */
    private void HandleAttack()
    {
        if (bullet == null)
        {
            if ((Input.GetButtonDown("Fire3") || Input.GetButton("Fire3")))
            {

                animator.SetBool("isCharging", true);

                if (charge < maxCharge)
                {
                    charge += Time.deltaTime;
                }
            }
            
            if (Input.GetButtonUp("Fire3")) // Does GetButtonUp imply Getbuttondown was called? And vice versa?
            {
                //Calculate direction for attack by intersecting mouse ray with selectable objects on raycastable layer.
                Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                int mainRaycastMask = 1 << 6; // Mask to just the main Raycast layer, so we only find hits to objects in that layer.

                RaycastHit hitInfo;

                if (Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, mainRaycastMask))
                {
                    Debug.Log("Hit object is: " + hitInfo.collider.name);
                    //TODO: when looking at a point too close to the character, it faces downward. Maybe need a fixed, higher y value.
                    this.transform.LookAt(new Vector3(hitInfo.point.x, 0, hitInfo.point.z));
                }

                //Animations and fire attack
                animator.SetBool("isUltimating", true);
                animator.SetBool("isCharging", false);
                isCharging = false;

                if (charge >= maxCharge)
                {
                    bullet = PhotonNetwork.Instantiate(this.chargedBulletPrefab.name, this.transform.position + Vector3.up * bulletOffset, this.transform.rotation);
                    bullet.GetComponent<ProjectileMovement>().SetLifetime(bulletLifetime * 2);
                }
                else
                {
                    bullet = PhotonNetwork.Instantiate(this.defaultBulletPrefab.name, this.transform.position + Vector3.up * bulletOffset, this.transform.rotation);
                    bullet.GetComponent<ProjectileMovement>().SetLifetime(bulletLifetime);
                }

                charge = 0f;
            }
        }
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
