using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class GiveElement : MonoBehaviour, IPunObservable
{
    private Element mostRecentElement = Element.Fire;

    [SerializeField]
    private GameObject fireVfx;
    [SerializeField]
    private GameObject waterVfx;
    [SerializeField]
    private GameObject earthVfx;

    public Element getElement()
    {
        return this.mostRecentElement;
    }

    public void setElement(Element nextElement)
    {
        this.mostRecentElement = nextElement;
        Debug.Log("Element set to: " + this.mostRecentElement);

        fireVfx.gameObject.SetActive(nextElement == Element.Fire);
        waterVfx.gameObject.SetActive(nextElement == Element.Water);
        earthVfx.gameObject.SetActive(nextElement == Element.Earth);
    }

    #region IPunObservable Implementation
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send others our data
            stream.SendNext(this.mostRecentElement);
        }
        else 
        {
            // Network player, receive data
            mostRecentElement = (Element)stream.ReceiveNext();
            Debug.Log("Element sent through stream: " + this.mostRecentElement);
            setElement(mostRecentElement);
        }
    }
    #endregion
}