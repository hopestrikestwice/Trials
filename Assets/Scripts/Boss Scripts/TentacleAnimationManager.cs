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

    private KrakenAnimationManager parentKrakenAnimationManager;

    #region Monobehaviour
    private void Start()
    {
        animator = this.GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("Tentacle is Missing Animator Component", this);
        }

        parentKrakenAnimationManager = this.GetComponentInParent<KrakenAnimationManager>();
    }

    private void Update()
    {
        if (this.photonView.IsMine)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Tentacle Idle"))
            {
                TargetNearestPlayer();
            }
            if (animator.GetBool("chainslam"))
            {
                this.transform.rotation = Quaternion.Euler(0, 270, 0);
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
            parentKrakenAnimationManager.SetSlam(0);
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
            parentKrakenAnimationManager.SetSwipe(0);
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

        parentKrakenAnimationManager.ReportChainslam();
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
    #endregion

    #region Private Methods

    private void TargetNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closestPlayer = null;
        float closestDist = Mathf.Infinity;
        foreach (GameObject player in players)
        {
            float distToPlayer = (player.transform.position - this.transform.position).sqrMagnitude;
            if (distToPlayer < closestDist)
            {
                closestPlayer = player;
                closestDist = distToPlayer;
            }
        }

        if (closestPlayer == null)
        {
            Debug.Log("no closest player!");
            return;
        }

        this.transform.LookAt(closestPlayer.transform);
        this.transform.Rotate(0, 90, 0);
    }

    #endregion
}
