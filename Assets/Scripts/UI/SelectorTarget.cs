using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectorTarget : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    private Selector selector;

    public void OnPointerEnter(PointerEventData eventData)
    {
        selector.setTarget(this.gameObject.GetComponent<RectTransform>().position);
    }
}
