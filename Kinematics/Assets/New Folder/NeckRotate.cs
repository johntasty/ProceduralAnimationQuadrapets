using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class NeckRotate : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform target;
    public Transform bodyMove;
    public CreatureController body;
    private Vector3 currentVelocity;
    private float currentAngularVelocity;
    // Update is called once per frame
    void Update()
    {
        //get direction toward target
        Vector3 towardTarget = target.position - transform.position;
        //vector toward target on local plane
        Vector3 towardTargetProjected = Vector3.ProjectOnPlane(towardTarget, transform.up);
        //get the angle from the box
        float angToTarget = Vector3.SignedAngle(transform.forward, towardTargetProjected, transform.up);

        float targetAngularVelocity = 0;

        //if withing the max angle leave velocity zero
        if (Mathf.Abs(angToTarget) > body.maxAngToTarget)
        {
            if (angToTarget > 0)
            {
                targetAngularVelocity = body.turnSpeed;
            }
            //invert angular speed if to our left
            else
            {
                targetAngularVelocity = -body.turnSpeed;
            }
        }
        //use smoothing to gradually change speed
        currentAngularVelocity = Mathf.Lerp(
            currentAngularVelocity,
            targetAngularVelocity,
            1 - Mathf.Exp(-body.turnAccelaration * Time.deltaTime)
            );

        //rotate transform around y axis
        //transform.Rotate(0, Time.deltaTime * currentAngularVelocity, 0, Space.World);
        var direction = Vector3.MoveTowards(transform.position,towardTarget,2f);
        var height = Mathf.Clamp(direction.y, 1.3f, 1.35f);
        var horizontal = bodyMove.position.x;
        this.transform.position = new Vector3(horizontal, 1.35f, 1.65f);
       
    }
}
