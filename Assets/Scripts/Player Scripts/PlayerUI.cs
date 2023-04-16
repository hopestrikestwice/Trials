/// PlayerUI.cs
/// 
/// Updates UI with player stats.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SkillUI { PRIMARY, SECONDARY, ULTIMATE }

public class PlayerUI : MonoBehaviour
{

    #region Private Fields

    [Tooltip("Pixel offset  from the player target")]
    [SerializeField]
    private Vector3 screenOffset = new Vector3(0f, 30f, 0f);

    [Tooltip("UI Text to display Player's Name")]
    [SerializeField]
    private Text playerNameText;

    [Tooltip("UI Slider to display Player's Health")]
    [SerializeField]
    private Slider playerHealthSlider;

    [SerializeField]
    private GameObject[] skillUI;

    private PlayerManagerCore target;

    float characterControllerHeight = 0f;
    Transform targetTransform;
    Renderer targetRenderer;
    CanvasGroup _canvasGroup;
    Vector3 targetPosition;

    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        this.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);

        _canvasGroup = this.GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        // Reflect the Player Health
        if (playerHealthSlider != null)
        {
            playerHealthSlider.value = target.GetHealth();
        }

        // Destroy itself i the target is null, It's a fail safe when Photon is destroying Instances of a Player over the network
        if (target == null)
        {
            Destroy(this.gameObject);
            return;
        }
    }

    private void LateUpdate()
    {
        // Do not show UI if we are not visible to the camera, thus avoiding potential bugs with seeing the UI, but not the player itself.
        if (targetRenderer != null)
        {
            this._canvasGroup.alpha = targetRenderer.isVisible ? 1f : 0f;
        }

        // #Critical
        // Follow the Target GameObject on the screen.
        if (targetTransform != null)
        {
            targetPosition = targetTransform.position;
            targetPosition.y = characterControllerHeight;
            //playerNameText.transform.position = Camera.main.WorldToScreenPoint(targetPosition) + screenOffset;
        }
    }

    #endregion

    #region Public Methods

    public void SetTarget(PlayerManagerCore _target)
    {
        if (_target == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
            return;
        }

        //Cache references for efficiency
        target = _target;
        if (playerNameText != null)
        {
            playerNameText.text = target.photonView.Owner.NickName;
        }

        targetTransform = this.target.GetComponent<Transform>();
        targetRenderer = this.target.GetComponent<Renderer>();
        CharacterController characterController = _target.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterControllerHeight = characterController.height;
        }
    }

    /* Darken the attack icon for feedback while attacking. */
    public void ShadeIcon(SkillUI skill) {
         /* Get the "Outer ring" object, which represents the attack icon */
        GameObject icon = skillUI[(int)skill].transform.GetChild(1).gameObject;
        icon.GetComponent<Image>().color = icon.GetComponent<Image>().color * new Color(0.5f, 0.5f, 0.5f);
    }

    public void UnshadeIcon(SkillUI skill) {
         /* Get the "Outer ring" object, which represents the attack icon */
        GameObject icon = skillUI[(int)skill].transform.GetChild(1).gameObject;
        icon.GetComponent<Image>().color = icon.GetComponent<Image>().color * new Color(2f, 2f, 2f);
    }

    //timePassed is a float from 0 to 1 of how much cooldown time has passed
    public void SkillCooldown(SkillUI skill, float timePassed)
    {
        Slider icon = skillUI[(int)skill].GetComponent<Slider>();
        if (icon != null)
        {
            if (timePassed <= 1)
            {
                icon.value = timePassed;
            }
            if (timePassed > 1)
            {
                icon.value = 0;
            }
        }
    }

    public void ResetCooldown(SkillUI skill)
    {
        Slider icon = skillUI[(int)skill].GetComponent<Slider>();
        icon.value = 1f;
    }

    public void UpdateElement()
    {
        // Display the element enhancing player
        foreach(GameObject icon in skillUI) {
            if(icon != null) {
                switch (target.GetElement())
                {
                    case Element.Fire:
                        icon.transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.red;
                        break;
                    case Element.Water:
                        icon.transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.blue;
                        break;
                    case Element.Earth:
                        icon.transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.yellow;
                        break;
                    default:
                        icon.transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.white;
                        break;
                }
            }
        }
    }

    #endregion
}
