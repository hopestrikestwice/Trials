using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealerProjectile : MonoBehaviour
{
    int sigCharge;
    private GameObject playerSource = null;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int getCharge()
    {
        return sigCharge;
    }

    public void setCharge(int charge)
    {
        sigCharge = charge;
    }

}
