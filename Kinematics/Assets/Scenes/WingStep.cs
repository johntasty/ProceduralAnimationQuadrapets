using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//[ExecuteInEditMode]
public class WingStep : MonoBehaviour
{
    //The position and rotation we want
    [SerializeField] Transform homeTransform;
    [SerializeField] Transform StartTransform;
    //stay within this distance
    [SerializeField] float wantStepAtDistance;
    //duration of step
    [SerializeField] float moveDuration;
    [SerializeField] float wingFlap;
    public NavMeshAgent navMeshagent;
    //distance to overshoot
    [SerializeField] float stepOvershootFraction;
    //is leg moving
    public bool Moving;
    // Start is called before the first frame update
   IEnumerator MoveWing()
    {
        Moving = true;
        float wingFlapping = Mathf.CeilToInt(navMeshagent.velocity.z);
        wingFlap = 11f - wingFlapping;


        // Directional vector from the foot to the home position
        Vector3 towardHome = (homeTransform.position - StartTransform.position );
        // Total distnace to overshoot by   
        float overshootDistance = wantStepAtDistance * stepOvershootFraction;
        Vector3 overshootVector = towardHome * overshootDistance;
        Vector3 startPoint = StartTransform.position ;
        Quaternion startRot = StartTransform.rotation;

        Quaternion endRot = StartTransform.rotation;
        
        // Apply the overshoot to make it seem more natural
        Vector3 endPoint = homeTransform.position - overshootVector;


        float time = Mathf.PingPong(Time.time * wingFlap, 1);
       
        
        transform.position = Vector3.Slerp(startPoint, endPoint, time);
        
        transform.rotation = Quaternion.Slerp(startRot, endRot, time);

        yield return null; 

        Moving = false;
    }

    // Update is called once per frame
    public void TryFly()
    {
        // If we are already moving, don't start another move
        if (Moving) return;

        float distFromHome = Vector3.Distance(transform.position, homeTransform.position);
        float distFromStart = Vector3.Distance(transform.position, StartTransform.position);
        

        // If we are too far off in position or rotation
        if (distFromStart > wantStepAtDistance || distFromHome > wantStepAtDistance )
        {
            // Start the step coroutine
            StartCoroutine(MoveWing());
        }
    }
}
