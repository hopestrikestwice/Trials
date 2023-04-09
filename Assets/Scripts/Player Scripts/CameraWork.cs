/// CameraWork.cs
/// 
/// Attach to GameObject to have the local camera follow that object.
/// For example, the local player character.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraWork : MonoBehaviour
{
    #region Private Fields

    [Tooltip("The distance in the local x-z plane to the target")]
    private float distance = 8.0f;

    [Tooltip("The height we want the camera to be above the target")]
    private float height = 3.0f;

    [Tooltip("Set this as false if a component of a prefab being instantiated by Photon Network. Manually call OnStartFollowing() if needed.")]
    [SerializeField]
    private bool followOnStart = false;

    // cached transform of the camera gameobject
    Transform cameraTransform;

    // maintain a flag internally to reconnect if target is lost or camera is switched
    bool isFollowing;

    Vector2 cameraTurn;
    #endregion

    #region Monobehaviour Callbacks

    // Start is called before the first frame update
    void Start()
    {
        // Make cursor disappear
        Cursor.lockState = CursorLockMode.Locked;

        if (followOnStart)
        {
            OnStartFollowing();
        }
    }

    void LateUpdate()
    {
        //  The transform target may not destroy on level load, so we need to cover corner cases where the Main Camera is different everytime we load a new scene,
        //  and reconnect when that happens.
        if (cameraTransform == null & isFollowing)
        {
            OnStartFollowing();
        }

        // only follow if explicitly declared
        if (isFollowing)
        {
            Follow();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Raises the start following event.
    /// Use this when you don't know at the time of editing what to follow, typically instances managed by the photon network.
    /// </summary>
    public void OnStartFollowing()
    {
        cameraTransform = Camera.main.transform;
        isFollowing = true;
    }

    /// <summary>
    /// Returns the main camera's current transform component.
    /// </summary>
    public Transform GetCameraTransform()
    {
        return cameraTransform;
    }

    #endregion

    #region Private Methods

    ///<summary>
    /// Follow the target smoothly
    /// </summary>
    void Follow()
    {
        // Calculate which way the camera is facing using mouse.
        cameraTurn.x += Input.GetAxis("Mouse X") * 12.0f;
        cameraTurn.y += Input.GetAxis("Mouse Y") * 1.0f;
        cameraTransform.localRotation = Quaternion.Euler(8.0f - cameraTurn.y, cameraTurn.x, 0);

        // Start calculating camera position from the character's position
        Vector3 newPosition = this.transform.position;
        // Add some height
        newPosition += Vector3.up * height;
        // Now shift it back some distance, so it's behind the player and looking at them.
        newPosition += -1 * cameraTransform.forward * distance;
        // Set this as the camera's new position
        cameraTransform.position = newPosition;
    }

    #endregion
}
