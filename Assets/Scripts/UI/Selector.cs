using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Assertions;

public class Selector : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;

    private float targetX, targetY;

    private void Start()
    {
        Assert.IsTrue(moveSpeed > 0);

        this.targetX = this.transform.position.x;
        this.targetY = this.transform.position.y;
    }

    private void Update()
    {
        if (this.transform.position.x != targetX || this.transform.position.y != targetY)
        {
            Vector3 targetPos = new Vector3(this.targetX, this.targetY, this.transform.position.z);

            if (Vector3.Distance(this.transform.position, targetPos) < moveSpeed * Time.deltaTime)
            {
                this.transform.position = targetPos;
            } else
            {
                this.transform.position = Vector3.Lerp(this.transform.position,
                                                       targetPos,
                                                       Time.deltaTime * moveSpeed);
            }
        }
    }

    public void setTarget(Vector3 newTarget)
    {
        this.targetX = newTarget.x;
        this.targetY = newTarget.y;
    }
}
