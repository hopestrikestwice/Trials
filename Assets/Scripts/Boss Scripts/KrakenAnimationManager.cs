using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class KrakenAnimationManager : MonoBehaviourPun
{
    #region Private Variables

    private Animator animator;

    //AI logic
    private float attackCooldown = 5f;
    private float currentAttackCooldown = 5f;

    private int chainslamWait;

    #endregion

    #region Monobehavior
    private void Start()
    {
        animator = this.GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("Kraken is Missing Animator Component", this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            if (currentAttackCooldown <= 0f && animator.GetCurrentAnimatorStateInfo(0).IsName("Kraken Idle"))
            {
                randomAttack();
                currentAttackCooldown = attackCooldown;
            } else
            {
                currentAttackCooldown -= Time.deltaTime;
            }
        }
    }
    #endregion

    #region Public Methods

    public void SetAnimatorParam(string paramName, bool val)
    {
        //set animator param;
    }
    #endregion

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
        }
        else if (val == 0)
        {
            animator.SetBool("slamming", false);
        }
        else
        {
            Debug.LogError("Invalid input to Kraken SetSlam!");
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
        }
        else
        {
            Debug.LogError("Invalid input to Kraken SetSwipe!");
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
            chainslamWait = 3;
            animator.SetBool("chainslam", true);
        }
        else if (val == 0)
        {
            animator.SetBool("chainslam", false);
            
            //iterate through tentacles, finishing the chainslam on each.
            for (int i = 0; i < 3; i++)
            {
                Transform tentacle = this.transform.GetChild(i);
                tentacle.GetComponent<TentacleAnimationManager>().SetChainslam(0);
            }
        }
        else
        {
            Debug.LogError("Invalid input to Kraken SetChainslam!");
        }
    }

    public void SlamRandomTentacle()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        this.SlamTentacle(Random.Range(0, 3));
    }

    public void SwipeRandomTentacle()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        this.SwipeTentacle(Random.Range(0, 3));
    }
    #endregion

    #region Public Methods
    public void ReportChainslam()
    {
        chainslamWait--;
        if (chainslamWait == 0)
        {
            this.SetChainslam(0);
        }
    }
    #endregion

    #region Private Methods
    private void randomAttack()
    {
        int randNum = Random.Range(0, 3);

        switch (randNum)
        {
            case 0:
                this.SetSlam(1);
                break;
            case 1:
                this.SetSwipe(1);
                break;
            case 2:
                this.SetChainslam(1);
                break;
            //case 3:
            //    animator.SetBool("Scream", true);
            //    break;
            //case 4:
            //    animator.SetBool("Drill", true);
            //    break;
            default:
                Debug.Log("Should never reach here!");
                break;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="tentacleIndex"> Child index of tentacle to slam </param>
    private void SlamTentacle(int tentacleIndex)
    {
        Transform tentacle = this.transform.GetChild(tentacleIndex);
        tentacle.GetComponent<TentacleAnimationManager>().SetSlam(1);
        //TODO: error handling
    }

    /// <summary>
    /// </summary>
    /// <param name="tentacleIndex"> Child index of tentacle to sweep </param>
    private void SwipeTentacle(int tentacleIndex)
    {
        Transform tentacle = this.transform.GetChild(tentacleIndex);
        tentacle.GetComponent<TentacleAnimationManager>().SetSwipe(1);
        //TODO: error handling
    }

    /// <summary>
    /// For Chainslam
    /// </summary>
    /// <param name="tentacleIndex"> Child index of tentacle to hover </param>
    private void HoverTentacle(int tentacleIndex)
    {
        Transform tentacle = this.transform.GetChild(tentacleIndex);
        tentacle.GetComponent<TentacleAnimationManager>().SetChainslam(1);
        //TODO: error handling
    }

    /// <summary>
    /// For Chainslam
    /// </summary>
    /// <param name="tentacleIndex"> Child index of tentacle to hoverslam </param>
    private void ChainslamTentacle(int tentacleIndex)
    {
        Transform tentacle = this.transform.GetChild(tentacleIndex);
        tentacle.GetComponent<TentacleAnimationManager>().SetReadyChainslam(1);
        //TODO: error handling
    }
    #endregion
}
