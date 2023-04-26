using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class BerserkerShootSlash : MonoBehaviour
{
    [SerializeField]
    private GameObject projectile;
    private const float speed = 10;

    public void InstantiateProjectile(float lifetime)
    {
        GameObject projectileObj = PhotonNetwork.Instantiate(projectile.name, this.gameObject.transform.position, this.gameObject.transform.rotation);
        StartCoroutine(SlowDown(projectileObj, lifetime));
    }

    IEnumerator SlowDown(GameObject projectileObj, float projectileLifetime)
    {
        Rigidbody rb = projectileObj.GetComponent<Rigidbody>();
        Vector3 start_velocity = transform.forward * speed;
        float t = projectileLifetime;
        
        while(t > 0)
        {
            rb.velocity = Vector3.Lerp(Vector3.zero, start_velocity, t);
            t -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        
        if (GetComponent<PhotonView>().IsMine) {
            PhotonNetwork.Destroy(projectileObj);
        }
    }
}
