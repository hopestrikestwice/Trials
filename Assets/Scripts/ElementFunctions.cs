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

    /* Used to generate random element for boss buffs */
    public static Element RandomElementNotGiven(Element given)
    {
        int randNum = Random.Range(0, 4);

        switch (randNum)
        {
            case 0:
                return given == Element.None ? Element.Fire : Element.None;
            case 1:
                return given == Element.Fire ? Element.Water : Element.Fire;
            case 2:
                return given == Element.Water ? Element.Earth : Element.Water;
            case 3:
                return given == Element.Earth ? Element.None : Element.Earth;
            default:
                /* TODO: should probably error here? */
                return Element.None;
        }
    }
}
