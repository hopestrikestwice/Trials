/// BossActionCore.cs
/// 
/// Handles generic boss ai behavior, such as how often to attack,
/// whether to make a basic or special attack, and the current target.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class BossActionCore : MonoBehaviourPun
{
    #region Private Variables

    private Animator animator;
    private IBossSkills skills;

    // AI logic
    private float phase1Cooldown = 7f;
    private float phase2Cooldown = 5f;
    private float phase3Cooldown = 4f;
    private float currentAttackCooldown = 7f;

    private GameObject targetPlayer;

    #endregion

    #region Monobehavior
    private void Start()
    {
        animator = this.GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("Boss is Missing Animator Component", this);
        }

        this.skills = this.GetComponent<IBossSkills>();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            // Targeting logic.
            TargetNearestPlayer();

            // Attack logic
            if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle"))
            {
                if (currentAttackCooldown <= 0f)
                {
                    RandomAttack();

                    switch (this.GetComponent<BossManagerCore>().GetPhase()) {
                        case 1:
                            currentAttackCooldown = phase1Cooldown;
                            break;
                        case 2:
                            currentAttackCooldown = phase2Cooldown;
                            break;
                        case 3:
                            currentAttackCooldown = phase3Cooldown;
                            break;
                    }
                }
                else
                {
                    currentAttackCooldown -= Time.deltaTime;
                }
            }
        }
    }
    #endregion

    #region Public Methods
    public GameObject GetTargetPlayer()
    {
        return this.targetPlayer;
    }
    #endregion

    #region Private Methods
    private void RandomAttack()
    {
        int randNum = Random.Range(0, 5);

        if (this.GetComponent<BossManagerCore>().GetPhase() == 1)
        {
            switch (randNum)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    this.skills.ActivateRandomBasicAttack();
                    break;
                case 4:
                    this.skills.ActivateRandomSpecialAttack();
                    break;
            }
        } else if (this.GetComponent<BossManagerCore>().GetPhase() == 2)
        {
            switch (randNum)
            {
                case 0:
                case 1:
                case 2:
                    this.skills.ActivateRandomBasicAttack();
                    break;
                case 3:
                case 4:
                    this.skills.ActivateRandomSpecialAttack();
                    break;
            }
        } else
        {
            switch (randNum)
            {
                case 0:
                case 1:
                    this.skills.ActivateRandomBasicAttack();
                    break;
                case 2:
                case 3:
                case 4:
                    this.skills.ActivateRandomSpecialAttack();
                    break;
            }
        }

    }

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
        this.targetPlayer = closestPlayer;
    }
    #endregion
}
