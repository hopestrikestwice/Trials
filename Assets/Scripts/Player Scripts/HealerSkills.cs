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
    private GameObject healer;

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
        Block(true);
    }

    public void ActivateUltimate()
    {
        Debug.Log("AOE heal ability pressed");
        animator.SetBool("isUltimating", true);
        DoSignature();
    }
    #endregion

    #region Public Methods
    public void DoSignature()
    {
        //Calculate direction for attack by intersecting mouse ray with selectable objects on raycastable layer.
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        int mainRaycastMask = 1 << 6; // Mask to just the main Raycast layer, so we only find hits to objects in that layer.

        RaycastHit hitInfo;

        if (Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, mainRaycastMask))
        {
            this.transform.LookAt(new Vector3(hitInfo.point.x, 1, hitInfo.point.z));
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

    public bool IsBlocked()
    {
        return this.isBlocked;
    }

    public void Block(bool blocked)
    {
        this.isBlocked = blocked;
    }

    #endregion

    #region Private Methods

    private void OnTriggerEnter(Collider other)
    {
        // if (other.CompareTag("BossProjectile") && (this.playerType == PlayerType.Healer && this.gameObject.GetComponent<HealerSkills>().IsBlocked()))
        // {
        //     Debug.Log("Boss atk blocked by healer");
        //     this.gameObject.GetComponent<HealerSkills>().AddCharge();
        // }
        if (other.CompareTag("BossProjectile") && IsBlocked())
        {
            Debug.Log("Boss atk blocked by healer");
            AddCharge();
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
