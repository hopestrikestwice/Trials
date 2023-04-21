using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class GiveElement : MonoBehaviour, IPunObservable
{
    private Element mostRecentElement = Element.Fire;

    public Element getElement()
    {
        return this.mostRecentElement;
    }

    public void setElement(Element nextElement)
    {
        this.mostRecentElement = nextElement;
        Debug.Log("Element set to: " + this.mostRecentElement);
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
        }
    }
    #endregion
}
