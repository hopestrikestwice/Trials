using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class BerserkerShootSlash : MonoBehaviour
{
    [SerializeField]
    private GameObject projectile;
    private const float speed = 10;

    private int damage;

    private GameObject currentProjectile;

    /* Tune to make charged attack feel better when it hits the boss and doesn't immediately disappear */
    private float destroyDelay  = 0.1f;

    public void InstantiateProjectile(int damage, float lifetime)
    {
        this.damage = damage;

        currentProjectile = PhotonNetwork.Instantiate(projectile.name, this.gameObject.transform.position, this.gameObject.transform.rotation);
        currentProjectile.GetComponent<BerserkerChargeCollision>().SetSlashScript(this);
        StartCoroutine(SlowDown(currentProjectile, lifetime));
    }

    public void ReportHitBoss(Collider other)
    {
        Debug.Log("ChargedAttack hit Boss!");
        /* Call destroy a few frames later so the effect still pops up at close range. */
        StartCoroutine(DelayedDestroyProjectile(other));
    }

    IEnumerator DelayedDestroyProjectile(Collider other)
    {
        yield return new WaitForSeconds(destroyDelay);

        /* Also time the damage with the destroy so it's synced */
        other.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.MasterClient, damage, this.GetComponent<PlayerManagerCore>().GetElement());

        PhotonNetwork.Destroy(currentProjectile);
    }

    IEnumerator SlowDown(GameObject projectileObj, float projectileLifetime)
    {
        Rigidbody rb = projectileObj.GetComponent<Rigidbody>();
        Vector3 start_velocity = transform.forward * speed;
        float t = projectileLifetime;
        
        while(t > 0)
        {
            if (projectileObj == null)
            {
                yield break;
            }

            rb.velocity = Vector3.Lerp(Vector3.zero, start_velocity, t);
            t -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        /* Need to duplicate here for race conditions feelsbadman */
        if (projectileObj == null)
        {
            yield break;
        }

        if (GetComponent<PhotonView>().IsMine) {
            PhotonNetwork.Destroy(projectileObj);
        }
    }
}
