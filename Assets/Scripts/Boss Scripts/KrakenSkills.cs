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
    [SerializeField]
    private GameObject lasers;
    private bool laserTentaclesActive = false;
    private bool lasersActive = false;
    private float laserTime = 10; // How long lasers are up in seconds.

    private bool rotateLasers = false;
    private float rotateLasersSpeed = 10; // degrees per second

    [SerializeField]
    private GameObject krakenHead;

    private Animator animator;

    private int activeTentacles = 4;

    private int chainslamTentacles = 16;

    private int chainslamWait;
    private int projectileWait;

    [SerializeField]
    private GameObject[] elementBuffs;
    private Element currentElement = 0;

    #region Sound Variables
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip emergeSFX;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("Kraken is Missing Animator Component", this);
        }

        audioSource = this.GetComponent<AudioSource>();
        if (!animator)
        {
            Debug.LogError("Kraken is Missing AudioSource Component", this);
        }

        animator.SetBool("rising", true);
    }

    private void Update()
    {
        if (laserTentaclesActive && !laserTentacles.activeSelf)
        {
            laserTentacles.SetActive(true);
        }
        else if (!laserTentaclesActive && laserTentacles.activeSelf)
        {
            laserTentacles.SetActive(false);
        }

        if (lasersActive && !lasers.activeSelf)
        {
            lasers.SetActive(true);
        }
        else if (!lasersActive && lasers.activeSelf)
        {
            lasers.SetActive(false);
        }

        /* PhotonView isMine only after this point */
        if (!photonView.IsMine)
        {
            return;
        }

        if (rotateLasers) {
            float laserRotation = laserTentacles.transform.rotation.eulerAngles.y;

            if (345 <= laserRotation || laserRotation <= 15 || BetweenTwoValues(laserRotation, 75, 105) || BetweenTwoValues(laserRotation, 165, 195) || BetweenTwoValues(laserRotation, 265, 285))
            {
                lasersActive = false;
            } else
            {
                lasersActive = true;
            }

            laserTentacles.transform.Rotate(rotateLasersSpeed * Vector3.up * Time.deltaTime);
        }
    }

    #region Animation Events
    public void PlayEmerge()
    {
        audioSource.PlayOneShot(emergeSFX);
    }
    public void FinishRising()
    {
        animator.SetBool("rising", false);
    }

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
            chainslamWait = chainslamTentacles;
            animator.SetBool("chainslam", true);
        }
        else if (val == 0)
        {
            animator.SetBool("chainslam", false);

            //iterate through main tentacles, finishing the chainslam on each.
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
        int randNum;
        switch (this.GetComponent<BossManagerCore>().GetPhase()) {
            case 1:
                randNum = Random.Range(0, 1);
                break;
            case 2:
                randNum = Random.Range(0, 4);
                break;
            default:
            case 3:
                randNum = Random.Range(0, 5);
                break;
        }


        switch (randNum)
        {
            case 0:
                this.SetProjectileThrow(1);
                break;
            case 1:
                this.Screech();
                break;
            case 2:
                this.BeginLaser();
                break;
            case 3:
                this.BeginRotateLaser();
                break;
            case 4:
                this.SetChainslam(1);
                break;
        }
    }
    #endregion


    #region Public Methods
    public Element GetElement()
    {
        return this.currentElement;
    }

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
            stream.SendNext(this.laserTentaclesActive);
            stream.SendNext(this.lasersActive);
        }
        else
        {
            // Network kraken, receive data
            this.laserTentaclesActive = (bool)stream.ReceiveNext();
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
            PhotonNetwork.Instantiate(this.projectile.name, player.transform.position + Vector3.up * 30, Quaternion.Euler(90, 0, 0));
        }
    }

    private void BeginLaser()
    {
        if (!this.laserTentacles.activeSelf)
        {
            this.laserTentaclesActive = true;
            this.lasersActive = true;
            this.Invoke("EndLaser", laserTime);
        }
    }

    private void BeginRotateLaser()
    {
        if (!this.laserTentacles.activeSelf)
        {
            this.laserTentaclesActive = true;
            this.Invoke("EndLaser", laserTime);
        }

        this.rotateLasers = true;
    }

    private void EndLaser()
    {
        this.rotateLasers = false;
        this.laserTentaclesActive = false;
        this.lasersActive = false;
    }

    /* Screech only affects the head, so we tell it to screech and
     * do not delay the cooldown of the next attack */
    private void Screech()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        this.krakenHead.GetComponent<KrakenHeadAnimationManager>().SetScreech(1);
    }
    #endregion

    #region Simple Private Helpers
    // Returns if left <= toCheck <= right
    private bool BetweenTwoValues(float toCheck, float left, float right)
    {
        return left <= toCheck && toCheck <= right;
    }
    #endregion

    #region RPCs
    [PunRPC]
    public void SetAura(Element newAura)
    {
        if (this.currentElement != Element.None)
        {
            this.elementBuffs[(int)this.currentElement - 1].SetActive(false);
        }
        this.currentElement = newAura;
        if (this.currentElement != Element.None)
        {
            this.elementBuffs[(int)this.currentElement - 1].SetActive(true);
        }
    }

    #endregion
}
