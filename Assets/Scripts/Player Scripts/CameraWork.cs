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
    private float height = 5.0f;

    [Tooltip("Allow the camera to be offset vertically from the target, for example giving more view of the scenery and less ground.")]
    private Vector3 centerOffset = Vector3.up * 3.6f;

    [Tooltip("Set this as false if a component of a prefab being instantiated by Photon Network. Manually call OnStartFollowing() if needed.")]
    [SerializeField]
    private bool followOnStart = false;

    // cached transform of the target
    Transform cameraTransform;

    // maintain a flag internally to reconnect if target is lost or camera is switched
    bool isFollowing;

    // cache for camera offset
    Vector3 cameraOffset = Vector3.zero;

    #endregion

    #region Monobehaviour Callbacks

    // Start is called before the first frame update
    void Start()
    {
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

    #endregion

    #region Private Methods

    ///<summary>
    /// Follow the target smoothly
    /// </summary>
    void Follow()
    {
        cameraOffset.z = -distance;
        cameraOffset.y = height;

        Vector3 newPosition = this.transform.position + new Vector3(distance, height, -distance);
        cameraTransform.position = newPosition;

        cameraTransform.LookAt(this.transform.position + centerOffset);
    }

    #endregion
}
