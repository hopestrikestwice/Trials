/// ElementFunctions.cs
/// 
/// Enumerator for Elements.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Element {None, Fire, Water, Earth}

public class ElementFunctions
{
    public static Element NextElement(Element old)
    {
        switch (old)
        {
            case Element.Fire:
                return Element.Water;
            case Element.Water:
                return Element.Earth;
            case Element.Earth:
                return Element.Fire;
            default:
                /* old == Element.NONE */
                return Element.None;
        }
    }
}
