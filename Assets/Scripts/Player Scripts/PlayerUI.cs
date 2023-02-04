using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private GameObject primarySkillUI;
    [SerializeField]
    private GameObject secondarySkillUI;
    [SerializeField]
    private GameObject ultimateSkillUI;

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
    public void ShadeBasicAttackIcon()
    {
        /* Get the "Outer ring" object, which represents the attack icon */
        GameObject primarySkillIcon = primarySkillUI.transform.GetChild(0).gameObject;
        primarySkillIcon.GetComponent<Image>().color = primarySkillIcon.GetComponent<Image>().color * new Color(0.5f, 0.5f, 0.5f);
    }

    public void UnshadeBasicAttackIcon()
    {
        /* Get the "Outer ring" object, which represents the attack icon */
        GameObject primarySkillIcon = primarySkillUI.transform.GetChild(0).gameObject;
        primarySkillIcon.GetComponent<Image>().color = primarySkillIcon.GetComponent<Image>().color * new Color(2f, 2f, 2f);
    }

    public void ShadeSkillIcon()
    {
        /* Get the "Outer ring" object, which represents the attack icon */
        GameObject secondarySkillIcon = secondarySkillUI.transform.GetChild(0).gameObject;
        secondarySkillIcon.GetComponent<Image>().color = secondarySkillIcon.GetComponent<Image>().color * new Color(0.5f, 0.5f, 0.5f);
    }

    public void UnshadeSkillIcon()
    {
        /* Get the "Outer ring" object, which represents the attack icon */
        GameObject secondarySkillIcon = secondarySkillUI.transform.GetChild(0).gameObject;
        secondarySkillIcon.GetComponent<Image>().color = secondarySkillIcon.GetComponent<Image>().color * new Color(2f, 2f, 2f);
    }
    public void ShadeUltimateIcon()
    {
        /* Get the "Outer ring" object, which represents the attack icon */
        GameObject ultimateSkillIcon = ultimateSkillUI.transform.GetChild(0).gameObject;
        ultimateSkillIcon.GetComponent<Image>().color = ultimateSkillIcon.GetComponent<Image>().color * new Color(0.5f, 0.5f, 0.5f);
    }

    public void UnshadeUltimateIcon()
    {
        /* Get the "Outer ring" object, which represents the attack icon */
        GameObject ultimateSkillIcon = ultimateSkillUI.transform.GetChild(0).gameObject;
        ultimateSkillIcon.GetComponent<Image>().color = ultimateSkillIcon.GetComponent<Image>().color * new Color(2f, 2f, 2f);
    }

    public void UpdateElement()
    {
        // Display the element enhancing player
        if (primarySkillUI != null)
        {
            switch (target.GetElement())
            {
                case Element.Fire:
                    primarySkillUI.transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.red;
                    break;
                case Element.Water:
                    primarySkillUI.transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.blue;
                    break;
                case Element.Earth:
                    primarySkillUI.transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.yellow;
                    break;
                default:
                    primarySkillUI.transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.white;
                    break;
            }
        }
    }

    #endregion
}
