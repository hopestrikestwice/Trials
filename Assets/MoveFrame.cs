using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MoveFrame : MonoBehaviour, IPointerEnterHandler
{
    public Image selector;
    private float posX;
    private float posY;
    private float aposY;
    private float posZ;
    private float moveSpeed;
    private bool moving;
    private bool greater;
  
    // Start is called before the first frame update
    void Start()
    {
        greater = false;
        moving = false;
        moveSpeed = 300;
        posX = selector.transform.position.x;
        posY = selector.transform.position.y;
        posX = 0;
        aposY = 0;
    
    }
    private void Update()
    {
        if (moving)
        {
            print("moving apose" + aposY);
            if (aposY < 0)
            {
                greater = true;
                posY -= moveSpeed * Time.deltaTime;
                print(posY);
            }
            else
            {
                greater = false;
                posY += moveSpeed * Time.deltaTime;
                print(posY);
            }
            selector.transform.position = new Vector3(gameObject.GetComponent<RectTransform>().position.x, posY, posZ);
            if ((greater && posY <= gameObject.GetComponent<RectTransform>().position.y) || (!greater && posY >= gameObject.GetComponent<RectTransform>().position.y))
            {
                posY = gameObject.GetComponent<RectTransform>().position.y;
                moving = false;
            }
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        selector.transform.position = gameObject.GetComponent<RectTransform>().position;
    }

}
