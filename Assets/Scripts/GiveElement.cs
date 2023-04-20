using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveElement : MonoBehaviour
{
    public Element mostRecentElement = Element.Fire;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Element getElement()
    {
        return mostRecentElement;
    }

    public void changeElement()
    {
        mostRecentElement = ElementFunctions.NextElement(mostRecentElement);
    }
}
