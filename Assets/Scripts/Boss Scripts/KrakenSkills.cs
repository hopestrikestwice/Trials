using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class KrakenSkills : MonoBehaviourPun, IBossSkills, IPunObservable
{
    [SerializeField]
    private GameObject projectile;
    [SerializeField]
    private AnimationClip tentacleProjectileThrowClip;

    [SerializeField]
    private GameObject laserTentacles;
    private bool lasersActive = false;
    private float laserTime = 10; // How long lasers are up in seconds.

    private bool rotateLasers = false;
    private float rotateLasersSpeed = 15; // degrees per second

    private Animator animator;

    private int activeTentacles = 4;

    private int chainslamWait;
    private int projectileWait;

    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("Kraken is Missing Animator Component", this);
        }
    }

    private void Update()
    {
        if (lasersActive && !laserTentacles.activeSelf)
        {
            laserTentacles.SetActive(true);
        }
        else if (!lasersActive && laserTentacles.activeSelf)
        {
            laserTentacles.SetActive(false);
        }

        /* PhotonView isMine only after this point */
        if (!photonView.IsMine)
        {
            return;
        }

        if (rotateLasers) {
            laserTentacles.transform.Rotate(rotateLasersSpeed * Vector3.up * Time.deltaTime);
        }
    }

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
            chainslamWait = activeTentacles;
            animator.SetBool("chainslam", true);
        }
        else if (val == 0)
        {
            animator.SetBool("chainslam", false);

            //iterate through tentacles, finishing the chainslam on each.
            for (int i = 0; i < 4; i++)
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

    public void SetProjectileThrow(int val)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (val == 1)
        {
            projectileWait = activeTentacles;
            animator.SetBool("throwing", true);

            //start throwing animation on every tentacle
            for (int i = 0; i < 4; i++)
            {
                ProjectileThrowTentacle(i);
            }

            Invoke("DropProjectiles", tentacleProjectileThrowClip.length / 2);
        }
        else if (val == 0)
        {
            animator.SetBool("throwing", false);
        }
        else
        {
            Debug.LogError("Invalid input to Kraken SetProjectileThrow!");
        }
    }

    public void SlamRandomTentacle()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        this.SlamTentacle(Random.Range(0, 4));
    }

    public void SwipeRandomTentacle()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        this.SwipeTentacle(Random.Range(0, 4));
    }
    #endregion

    #region IBossSkills Implementation
    public void ActivateRandomBasicAttack()
    {
        int randNum = Random.Range(0, 2);

        switch (randNum)
        {
            case 0:
                this.SetSlam(1);
                break;
            case 1:
                this.SetSwipe(1);
                break;
        }
    }

    public void ActivateRandomSpecialAttack()
    {

        int randNum = Random.Range(0, 4);

        switch (randNum)
        {
            case 0:
                this.SetChainslam(1);
                break;
            case 1:
                this.SetProjectileThrow(1);
                break;
            case 2:
                this.BeginLaser();
                break;
            case 3:
                this.BeginRotateLaser();
                break;
        }
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

    public void ReportProjectileThrow()
    {
        projectileWait--;
        if (projectileWait == 0)
        {
            Debug.Log("Successful throw");
            this.SetProjectileThrow(0);
        }
    }
    #endregion

    #region IPunObservable Implementation
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own the kraken: send other clients our data
            stream.SendNext(this.lasersActive);
        }
        else
        {
            // Network kraken, receive data
            this.lasersActive = (bool)stream.ReceiveNext();
        }
    }
    #endregion

    #region Private Methods
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

    private void ProjectileThrowTentacle(int tentacleIndex)
    {
        Transform tentacle = this.transform.GetChild(tentacleIndex);
        tentacle.GetComponent<TentacleAnimationManager>().SetProjectileThrow(1);
    }

    private void DropProjectiles()
    {
        /* Create a rock above every player and drop it */
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PhotonNetwork.Instantiate(this.projectile.name, player.transform.position + Vector3.up * 10, Quaternion.Euler(90, 0, 0));
        }
    }

    private void BeginLaser()
    {
        if (!this.laserTentacles.activeSelf)
        {
            this.lasersActive = true;
            this.Invoke("EndLaser", laserTime);
        }
    }

    private void BeginRotateLaser()
    {
        if (!this.laserTentacles.activeSelf)
        {
            this.lasersActive = true;
            this.Invoke("EndLaser", laserTime);
        }

        this.rotateLasers = true;
    }

    private void EndLaser()
    {
        this.rotateLasers = false;
        this.lasersActive = false;
    }
    #endregion
}
