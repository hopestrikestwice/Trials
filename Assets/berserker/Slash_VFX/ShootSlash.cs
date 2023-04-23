using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootSlash : MonoBehaviour
{
    public GameObject projectile;
    public float fireRate = 4;

    private Ground_Slash groundSlashScript;

    public void InstantiateProjectile()
    {
        var projectileObj = Instantiate(projectile, this.gameObject.transform.position, this.gameObject.transform.rotation) as GameObject;

        groundSlashScript = projectile.GetComponent<Ground_Slash>();
        projectileObj.GetComponent<Rigidbody>().velocity = transform.forward * groundSlashScript.speed;
    }
}
