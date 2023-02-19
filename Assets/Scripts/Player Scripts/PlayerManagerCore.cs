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

    #region Private Fields
    private GameObject playerUI;

    [Tooltip("The current Health of our player")]
    private float Health = 1f;

    private bool isShielded = false;
    private Element currentElement = Element.None;

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
            if (this.Health <= 0f)
            {
                Debug.Log("Player Died");
            }
            // Debug.Log("Shielded? "+this.isShielded);
            // Debug.Log("Player's element "+this.currentElement);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        //If object is a boss projectile, decrement health
        if (other.CompareTag("BossProjectile") && !isShielded)
        {
            Debug.Log("player hit!");
            this.Health -= 0.25f;
        }

        if (other.CompareTag("Shield"))
        {
            Debug.Log("Player is now shielded");
            this.isShielded = true;
        }

        if (other.CompareTag("Heal"))
        {
            Debug.Log("Player got healed");
            this.Health += 0.25f;
        }

        if (other.CompareTag("ElementBuff"))
        {
            Element newElement = other.GetComponent<GiveElement>().getElement();
            Debug.Log("New Element "+newElement);
            // this.gameObject.GetComponent<PlayerActionCore>().setElement(newElement);
            this.gameObject.GetComponent<PlayerManagerCore>().SetElement(newElement);
            Debug.Log("Player's element is now "+this.currentElement);
            other.GetComponent<GiveElement>().changeElement();
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
            this.isShielded = false;
        }

        //Removes element buff?
        // if (other.CompareTag("ElementBuff"))
        // {
        //     this.gameObject.GetComponent<PlayerActionCore>().setElement(Element.None);
        // }
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
            stream.SendNext(this.Health);
            stream.SendNext(this.isShielded);
            stream.SendNext(this.currentElement);
        }
        else
        {
            //Network player, receive data
            this.Health = (float)stream.ReceiveNext();
            this.isShielded = (bool)stream.ReceiveNext();
            
            // if ((bool)stream.ReceiveNext())
            this.currentElement = (Element)stream.ReceiveNext();
            // Debug.Log("Player's element "+this.currentElement);
            // Debug.Log("Shielded? "+this.isShielded);
        }
    }

    #endregion

    #region Public Getters/Setters

    public float GetHealth()
    {
        return this.Health;
    }

    public Element GetElement()
    {
        return this.currentElement;
    }

    public void SetElement(Element newElement)
    {
        this.currentElement = newElement;
        playerUI.GetComponent<PlayerUI>().UpdateElement();
        Debug.Log("Changing Element (manager): "+newElement);
    }

    /* Gets the script PlayerUI attached to the playerUI for this player */
    public PlayerUI getPlayerUI()
    {
        return playerUI.GetComponent<PlayerUI>();
    }

    #endregion

    #region Private Methods

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
    {
        this.CalledOnLevelWasLoaded(scene.buildIndex);
    }

    #endregion
}
