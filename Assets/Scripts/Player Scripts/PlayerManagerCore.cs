/// PlayerManagerCore.cs
/// 
/// Manages player stats (health, etc.) and sync them across the network.
/// Sets up camera and UI upon character initialization for the local player.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class PlayerManagerCore : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Public Fields

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene.")]
    public static GameObject LocalPlayerInstance;

    [Tooltip("The Player's UI GameObject Prefab")]
    [SerializeField]
    public GameObject PlayerUiPrefab;

    #endregion

    [SerializeField]
    private int maxHealth;

    #region Private Fields
    private GameObject playerUI;

    [Tooltip("The current Health of our player")]
    private int health;

    private bool isProtected = false; //Used for tank's abilities
    private bool isShielded = false; //Used for berserker's ability
    private bool isFogged = false; //Used for support's ability

    private Element currentElement = Element.None;

    private float timeSinceHit = 0f;
    // How many seconds to make character immune after taking damage
    private float immunitySinceHit = 0.5f;

    #endregion

    #region MonoBehaviour CallBacks

    private void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            PlayerManagerCore.LocalPlayerInstance = this.gameObject;
        }
        // #Critical
        //  we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        health = maxHealth;

        CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();

        if (_cameraWork != null)
        {
            if (photonView.IsMine)
            {
                _cameraWork.OnStartFollowing();
            }
        }
        else
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

        if (PlayerUiPrefab != null && photonView.IsMine)
        {
            playerUI = Instantiate(PlayerUiPrefab);
            playerUI.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }
        else
        {
            Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
        }
    }

    public void Update()
    {
        if (photonView.IsMine)
        {
            if (this.health <= 0)
            {
                //Debug.Log("Player Died");
            }

            this.timeSinceHit = Mathf.Min(this.timeSinceHit + Time.deltaTime, this.immunitySinceHit);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        //If object is a boss projectile, decrement health
        if (other.CompareTag("BossTentacle") && this.timeSinceHit >= this.immunitySinceHit && !(isShielded || isProtected))
        {
            Debug.Log("Player " + photonView.Owner + " hit!");
            this.health -= 20;

            this.timeSinceHit = 0f;
        }

        if (other.CompareTag("BossProjectile") && !(isShielded || isProtected))
        {
            Debug.Log("Player " + photonView.Owner + " hit!");
            this.health -= 20;

            this.timeSinceHit = 0f;
        }

        if (other.CompareTag("Shield"))
        {
            Debug.Log("Player is now shielded");
            this.isProtected = true;
        }

        if (other.CompareTag("Fog"))
        {
            Debug.Log("Player entered fog");
            this.isFogged = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (other.CompareTag("Fog"))
        {
            SetElement(other.GetComponent<GiveElement>().getElement());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (other.CompareTag("Shield"))
        {
            Debug.Log("Player no longer shielded");
            this.isProtected = false;
        }

        if (other.CompareTag("Fog"))
        {
            Debug.Log("Player left fog");
            this.isFogged = false;
        }
    }

    void OnLevelWasLoaded(int level)
    {
        this.CalledOnLevelWasLoaded(level);
    }

    void CalledOnLevelWasLoaded(int level)
    {
        //check if we are outside the Arena and if it's the case, spawn around the center of the arena.
        if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
        {
            transform.position = new Vector3(0f, 5f, 0f);
        }

        GameObject _uiGo = Instantiate(this.PlayerUiPrefab);
        _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion

    #region IPunObservable Implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send others our data
            stream.SendNext(this.health);
            stream.SendNext(this.isShielded);
            stream.SendNext(this.isProtected);
            stream.SendNext(this.isFogged);
            stream.SendNext(this.currentElement);
        }
        else
        {
            //Network player, receive data
            this.health = (int)stream.ReceiveNext();
            this.isShielded = (bool)stream.ReceiveNext();
            this.isProtected = (bool)stream.ReceiveNext();
            this.isFogged = (bool)stream.ReceiveNext();
            this.currentElement = (Element)stream.ReceiveNext();
        }
    }

    #endregion

    #region Public Getters/Setters

    public int GetMaxHealth()
    {
        return this.maxHealth;
    }

    public int GetHealth()
    {
        return this.health;
    }

    public Element GetElement()
    {
        return this.currentElement;
    }

    public void SetElement(Element newElement)
    {
        this.currentElement = newElement;
        playerUI.GetComponent<PlayerUI>().UpdateElement();
    }

    /* Gets the script PlayerUI attached to the playerUI for this player */
    public PlayerUI getPlayerUI()
    {
        return playerUI.GetComponent<PlayerUI>();
    }

    public void SetShielded(bool shielded)
    {
        this.isShielded = shielded;
        Debug.Log("Shielded value set to: " + shielded);
    }

    public bool getIsFogged()
    {
        return this.isFogged;
    }

    #endregion

    #region RPCs
    [PunRPC]
    public void HealPlayer(int amount)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        Debug.Log("Healing "+this.gameObject+"by "+amount);
        this.health = Mathf.Min(this.health + amount, this.maxHealth);
    }
    #endregion

    #region Private Methods

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
    {
        this.CalledOnLevelWasLoaded(scene.buildIndex);
    }

    #endregion
}
