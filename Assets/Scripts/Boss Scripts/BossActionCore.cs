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
    private float attackCooldown = 5f;
    private float currentAttackCooldown = 5f;

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
                    currentAttackCooldown = attackCooldown;
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
        int randNum = Random.Range(0, 3);

        switch (randNum)
        {
            case 0:
            case 1:
                this.skills.ActivateRandomBasicAttack();
                break;
            case 2:
                this.skills.ActivateRandomSpecialAttack();
                break;
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
