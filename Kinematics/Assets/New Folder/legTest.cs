using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteInEditMode]
public class legTest : MonoBehaviour
{
    public LayerMask ground;
    [Range(-1f, 1f)]
    public float distanceGround;
    public float DIstFromBone;
    public float len;

    public float speed;
    public float overShootStep;
    public float offStepVertical = 0f;


    public Transform rootBone;
    public Transform body;
 
    public AnimationCurve maxDist;
    public AnimationCurve maxHeight;
    public AnimationCurve testProgress;

    // Start is called before the first frame update
    Vector3 targetPos;

    Vector3 targetNormal;

    public Vector3 velocity;

    public bool Moving;
    public float maxSpeed = 3f;
    public float minSpeed = 1f;
    public float maxAllowedPrediction;

    public float lastStep = 0f;
    private float linearDistance;
    private float jump;

    public Vector3 startDistance;
    public Vector3 forwardVector;
    public Vector3 direction;
    public Vector3 endpoint;
    public Vector3 testingMark;

 
    public void FootMove()
    {
      
        velocity = body.GetComponent<CreatureController>().currentVelocity;

        Vector3 horizontalVelocity = velocity;
        horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        float horizontalSpeed = velocity.magnitude;
       
        speed = Mathf.Clamp(horizontalSpeed, minSpeed, maxSpeed);
               
        RaycastHit hit;
        Vector3 left45 = (rootBone.up*-len);

        if (Physics.Raycast(rootBone.position + (rootBone.right * distanceGround) + (rootBone.forward * DIstFromBone), left45, out hit, 3f, ground))
        {
            targetPos = hit.point;
            targetPos.y += offStepVertical;
            targetNormal = hit.normal;
        }
        Debug.DrawRay(rootBone.position + (rootBone.right * distanceGround) + (rootBone.forward * DIstFromBone), left45, Color.green);
        testingMark = targetPos;
        direction = targetPos - transform.position;
       
        //get the dot of the foot position to judge if its infront or behind the target
        float directionToTargetDot = Vector3.Dot(direction.normalized, Vector3.forward);
        //set max distance it can move on -1 and 1 on the graph
        float maxDistAllowed = maxDist.Evaluate(directionToTargetDot);


        float over = maxAllowedPrediction * overShootStep;
        //adding overshoot to look more natural
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        forwardVector = lookRotation * Vector3.forward * over;
        //project on a plane since we dont rlly care about y
        forwardVector = Vector3.ProjectOnPlane(forwardVector, Vector3.up);
        endpoint = targetPos + forwardVector;
        //2 ways of moving, eith lerp which i do below or by percentage of the distance each frame, went with lerp       
        startDistance = ((targetPos + velocity * maxAllowedPrediction) - transform.position);
        linearDistance = startDistance.magnitude;
        startDistance.Normalize();
        startDistance *= (linearDistance - (maxAllowedPrediction * maxAllowedPrediction));
        
        if (direction.sqrMagnitude >= maxDistAllowed * maxDistAllowed)
        {
            //evaluate for overshoot
            float directionToPredictedTarget = Vector3.Dot(velocity.normalized, transform.forward);            
            maxAllowedPrediction = maxDist.Evaluate(directionToPredictedTarget);
                      
           
            TryMove();

        }
        

    }
 
    public void TryMove()
    {
        if (Moving) return;
        StartCoroutine(TestRoutine());
    }
    //public void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(testingMark, 0.1f);
    //}
    IEnumerator TestRoutine()
    {
        Moving = true;
        // Run continuously
        
              
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(0, body.rotation.eulerAngles.y, 0);

        Vector3 newPos = transform.position;
        
        float over = maxAllowedPrediction * overShootStep;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Vector3 forwardVectorMove = lookRotation * Vector3.forward * over;
        forwardVectorMove = Vector3.ProjectOnPlane(forwardVectorMove, Vector3.up);
        var i = 0f;

        do
        {
            //update target position while moving so the leg doesnt fall behind
                                        
            Vector3 newEnd = targetPos + forwardVectorMove;
            //using percentage instead of just deltatime * speed makes for much smoother transitions of animation
            float move = i / lastStep;
            //evaluating graph position based on percentage of the move
            jump = maxHeight.Evaluate(move);
            
            transform.position = Vector3.Lerp(newPos, newEnd, move);
          
            transform.position += transform.up * jump;
     
            transform.rotation = Quaternion.Slerp(startRot, endRot, 10f);
         
            i += Time.deltaTime * speed;
            
            yield return null;
        } while (i < 1f);
        transform.position = targetPos + forwardVectorMove;
        Moving = false;
    }
   
    public Vector3 SuperSmoothVector3Lerp(Vector3 pastPosition, Vector3 pastTargetPosition, Vector3 targetPosition, float time, float speed)
    {
        Vector3 f = pastPosition - pastTargetPosition + (targetPosition - pastTargetPosition) / (speed * time);
        return targetPosition - (targetPosition - pastTargetPosition) / (speed * time) + f * (1 -Mathf.Exp(-speed * time));
    }
}
