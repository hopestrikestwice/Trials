/// TentacleAnimationManager.cs
/// 
/// Handles Tentacle's attacks and animations. Also reports state to
/// KrakenAnimationManager.cs as necessary.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class TentacleAnimationManager : MonoBehaviourPun
{
    private Animator animator;

    private BossActionCore parentActionCore;
    private KrakenSkills parentKrakenSkills;

    #region Monobehaviour
    private void Start()
    {
        animator = this.GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("Tentacle is Missing Animator Component", this);
        }

        parentActionCore = this.GetComponentInParent<BossActionCore>();

        parentKrakenSkills = this.GetComponentInParent<KrakenSkills>();
    }

    private void Update()
    {
        if (this.photonView.IsMine)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Tentacle Idle"))
            {
                FaceTarget();
            }
            if (animator.GetBool("chainslam"))
            {
                this.transform.LookAt(Vector3.zero);
            }
        }
    }
    #endregion Monobehaviour

    #region Animation Events
    /// <summary>
    /// val = 1 to set to true, val = 0 for false.
    /// </summary>
    /// <param name="val"> val = 1 to set to true, val = 0 for false. </param>
    public void SetSlam(int val)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (val == 1)
        {
            animator.SetBool("slamming", true);
        } else if (val == 0)
        {
            animator.SetBool("slamming", false);
            parentKrakenSkills.SetSlam(0);
        } else
        {
            Debug.LogError("Invalid input to SetSlam!");
        }
    }

    public void SetSwipe(int val)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (val == 1)
        {
            animator.SetBool("swiping", true);
        }
        else if (val == 0)
        {
            animator.SetBool("swiping", false);
            parentKrakenSkills.SetSwipe(0);
        }
        else
        {
            Debug.LogError("Invalid input to SetSwipe!");
        }
    }

    public void SetChainslam(int val)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (val == 1)
        {
            animator.SetBool("chainslam", true);
        }
        else if (val == 0)
        {
            animator.SetBool("chainslam", false);
        }
        else
        {
            Debug.LogError("Invalid input to SetChainslam!");
        }
    }

    public void ReportChainslamUpwards()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        parentKrakenSkills.ReportChainslam();
    }

    public void SetReadyChainslam(int val)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (val == 1)
        {
            animator.SetBool("readyChainslam", true);
        }
        else if (val == 0)
        {
            animator.SetBool("readyChainslam", false);
        }
        else
        {
            Debug.LogError("Invalid input to SetReadyChainslam!");
        }
    }

    public void SetProjectileThrow(int val)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (val == 1)
        {
            animator.SetBool("throwing", true);
        }
        else if (val == 0)
        {
            animator.SetBool("throwing", false);
            parentKrakenSkills.ReportProjectileThrow();
        }
        else
        {
            Debug.LogError("Invalid input to SetProjectileThrow!");
        }
    }
    #endregion

    #region Private Methods

    private void FaceTarget()
    {
        GameObject closestPlayer = this.parentActionCore.GetTargetPlayer();

        if (closestPlayer == null)
        {
            Debug.Log("no closest player!");
            return;
        }

        this.transform.LookAt(closestPlayer.transform);
    }

    #endregion
}
