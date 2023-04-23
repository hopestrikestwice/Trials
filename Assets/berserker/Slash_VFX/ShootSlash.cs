using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootSlash : MonoBehaviour
{
    public Camera cam;
    public GameObject projectile;
    public Transform firePoint;
    public float fireRate = 4;

    private Vector3 destination;
    private float timeToFire;
    private Ground_Slash groundSlashSCript;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Pressing space currently fires the slash. change this.
        if (Input.GetKeyDown("space"))
        {
            timeToFire = Time.time + 1/fireRate;
            ShootProjectile();
        }
    }

    //Destination marks where the slash is fired from. In this case it is always 0. Change this.
    void ShootProjectile()
    {
        destination = new Vector3(0, 0, 0);
        InstantiateProjectile();
    }
    void InstantiateProjectile()
    {
        var projectileObj = Instantiate(projectile, firePoint.position, Quaternion.identity) as GameObject;

        groundSlashSCript = projectile.GetComponent<Ground_Slash>();
        RotateToDestination(projectileObj, destination, true);
        projectileObj.GetComponent<Rigidbody>().velocity = transform.forward * groundSlashSCript.speed;
    }

    //Rotates slash depending on projected destination and direction of fire. Currently 0. Change this. 
    void RotateToDestination( GameObject obj, Vector3 destination, bool onlyY)
    {
        var direction = destination - obj.transform.position;
        var rotation = Quaternion.LookRotation(direction);

        if (onlyY)
        {
            rotation.x = 0;
            rotation.z = 0;
        }

        obj.transform.localRotation = Quaternion.Lerp(obj.transform.rotation, rotation, 1);

    }
}
