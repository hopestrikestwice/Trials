using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ChangeInfo : MonoBehaviour
{
    public GameObject[] infos;
    private int currentInfo;
    private GameObject currentInfoImage;
    // Start is called before the first frame update
    void Start()
    {
        currentInfo = 0;
        currentInfoImage = infos[0];
        currentInfoImage.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void changeInfo()
    {
        print("changing");
        currentInfo += 1;
        currentInfoImage.GetComponent<Animator>().SetBool("close", true);
        currentInfoImage.SetActive(false);
        currentInfoImage = infos[currentInfo];
        currentInfoImage.SetActive(true);

    }

}
