using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//[ExecuteInEditMode]
public class BirdControl : MonoBehaviour
{
    public LayerMask IgnoreMe;
    public float heightCap = 25f;
    public bool FlyingBird = true;
    //how fast we turn and move
    [SerializeField] float turnSpeed;
    [SerializeField] float moveSpeed;
    //how fast we reach max velocity
    [SerializeField] float turnAccelaration;
    [SerializeField] float moveAccelaration;
    //stay within this range from target
    [SerializeField] float minDistToTarget;
    [SerializeField] float maxDistToTarget;
    //target we are sticking close too
    [SerializeField] Transform agent;
    public NavMeshAgent navMeshagent;
    public GameObject agentBox;
    //max angle before begin turning
    [SerializeField] float maxAngToTarget;

    Vector3 currentVelocity;
    float currentAngularVelocity;
  
    //target we are tracking
    [SerializeField] Transform target;
    //reference to neck
    [SerializeField] Transform headBone;
    // Start is called before the first frame update
    [SerializeField] float headMaxTurnAngle;
    [SerializeField] float BodyMaxTurnAngle;
    [SerializeField] float headTrackingSpeed;
    //get foot steppers
    [SerializeField] WingStep frontLeftLegStepper;
    [SerializeField] WingStep frontRightLegStepper;
    //[SerializeField] LegStepper backLeftLegStepper;
    //[SerializeField] LegStepper backRightLegStepper;
    //get height of foots placements
    [SerializeField] Transform frontLeft;
    [SerializeField] Transform frontRight;
    //[SerializeField] Transform backLeft;
    //[SerializeField] Transform backRight;    
    [SerializeField] Transform bodyRotate;    
    [SerializeField] Transform BodyRotationClamped;    
    [SerializeField] float offset;
    private Quaternion currentBodyLocalRotation;
    private Quaternion targetLocalRotationBody;

    IEnumerator LegUpdateCoroutine()
    {
        // Run continuously
        while (true)
        {
            
            do
            {
                frontLeftLegStepper.TryFly();
                frontRightLegStepper.TryFly();
                //backRightLegStepper.TryMove();
                // Wait a frame
                yield return null;

                // Stay in this loop while either leg is moving.
                // If only one leg in the pair is moving, the calls to TryMove() will let
                // the other leg move if it wants to.
            } while (frontLeftLegStepper.Moving );

            // Do the same thing for the other diagonal pair
            
           
     
        }
    }

    void RootMotionUpdate()
    {
       
        //get direction toward target
        Vector3 towardTarget = target.position - transform.position ;
        //vector toward target on local plane
        Vector3 towardTargetProjected = Vector3.ProjectOnPlane(towardTarget, transform.up);

        //get direction toward target we are sticking close
        Vector3 towardTargetAgent = agent.position - transform.position;
        //vector toward target on local plane
        Vector3 towardTargetProjectedAgent = Vector3.ProjectOnPlane(towardTargetAgent, transform.up);
        //get the angle from the box
        float angToTarget = Vector3.SignedAngle(transform.forward, towardTargetProjected, transform.up);

        float targetAngularVelocity = 0;

        //if withing the max angle leave velocity zero
        if (Mathf.Abs(angToTarget) > maxAngToTarget)
        {
            if (angToTarget > 0)
            {
                targetAngularVelocity = turnSpeed;
            }
            //invert angular speed if to our left
            else
            {
                targetAngularVelocity = -turnSpeed;
            }
        }
        //use smoothing to gradually change speed
        currentAngularVelocity = Mathf.SmoothStep(
            currentAngularVelocity,
            targetAngularVelocity,
            1 - Mathf.Exp(-turnAccelaration * Time.deltaTime)
            );

        //rotate transform around y axis
        bodyRotate.Rotate(0, Time.deltaTime * currentAngularVelocity, 0, Space.World);

        Vector3 targetVelocity = Vector3.zero;

        // Don't move if we're facing away from the target, just rotate in place
        if (Mathf.Abs(angToTarget) < 90)
        {
            float distToTarget = Vector3.Distance(transform.position, new Vector3(agent.position.x, agent.position.y + transform.position.y, agent.position.z));
            
            // If we're too far away, approach the target
            if (distToTarget > maxDistToTarget || distToTarget > minDistToTarget)
            {
                targetVelocity = moveSpeed * towardTargetProjectedAgent.normalized;
            }
            // If we're too close, reverse the direction and move away
            else if (distToTarget < minDistToTarget)
            {
                
                targetVelocity = moveSpeed * -towardTargetProjectedAgent.normalized;
            }
        }

        currentVelocity = Vector3.Lerp(
          currentVelocity,
          targetVelocity,
          1 - Mathf.Exp(-moveAccelaration * Time.deltaTime)
        );

        // Apply the velocity
        transform.position += currentVelocity * Time.deltaTime;
    }
    void LateUpdate()
    {
        RootMotionUpdate();
        //create ray so the bird can  interact with all colliders and have the ability to land on them
        Ray ray = new Ray(this.transform.position + Vector3.up * 1, Vector3.down * 50);
        RaycastHit hit;
        ////adjust height based on targets position

        if (Physics.Raycast(ray, out hit, 50f, ~IgnoreMe))
        {
            var posFly = Vector3.Magnitude(transform.position - (navMeshagent.destination + Vector3.up * 15));
            Vector3 speed = currentVelocity;
            speed = navMeshagent.velocity;



            if (speed == Vector3.zero)
            {

                //if idle land down to the point of the ray
                var positioning = hit.point;

                float time = 2.5f * Time.deltaTime;
                float heights = transform.position.y - time;
                var height = Mathf.Clamp(heights, hit.point.y, hit.point.y + heightCap);
                transform.position = new Vector3(positioning.x, height, positioning.z);
                if (height <= hit.point.y + 1f)
                {
                    float offsetAnglez = 0.65f;
                    FlyingBird = false;
                    BodyRotationClamped.localRotation = Quaternion.Slerp(
                      currentBodyLocalRotation,
                     //create an inverse value for the body anglesince we want it to be upright at close and flat at far
                     new Quaternion(offsetAnglez - targetLocalRotationBody.x, targetLocalRotationBody.y, targetLocalRotationBody.z, targetLocalRotationBody.w),
                      1 - Mathf.Exp(-headTrackingSpeed * Time.deltaTime)
                    );
                    speed = Vector3.zero;
                }


            }
            //if moving adjust speed and height slowly
            if (speed != Vector3.zero)
            {
                FlyingBird = true;
                
                var positioning = hit.point;

                float time = 5f * Time.deltaTime;
                float heights = transform.position.y + time;
                var height = Mathf.Clamp(heights, hit.point.y, hit.point.y + heightCap);

                transform.position = new Vector3(positioning.x, height, positioning.z);

            }
        }
            // Store the current head rotation since we will be resetting it

            Quaternion currentLocalRotation = headBone.localRotation;
        currentBodyLocalRotation = BodyRotationClamped.localRotation;
                
        // Reset the head rotation so our world to local space transformation will use the head's zero rotation. 
        // Note: Quaternion.Identity is the quaternion equivalent of "zero"
        headBone.localRotation = Quaternion.identity;
        BodyRotationClamped.localRotation = Quaternion.identity;


        Vector3 targetWorldLookDir = target.position - headBone.position;
      
        Vector3 targetLocalLookDir = headBone.InverseTransformDirection(targetWorldLookDir);

        Vector3 targetWorldLookDirBody = (target.position - BodyRotationClamped.position) ;
        
        Vector3 targetLocalLookDirBody = BodyRotationClamped.InverseTransformDirection(targetWorldLookDirBody);

        // Apply angle limit
        targetLocalLookDir = Vector3.RotateTowards(
          Vector3.forward,
          targetLocalLookDir,
          Mathf.Deg2Rad * headMaxTurnAngle, // Note we multiply by Mathf.Deg2Rad here to convert degrees to radians
          0 // We don't care about the length here, so we leave it at zero
        );
        targetLocalLookDirBody = Vector3.RotateTowards(
          Vector3.forward,
          targetLocalLookDirBody,
          Mathf.Deg2Rad * BodyMaxTurnAngle, // Note we multiply by Mathf.Deg2Rad here to convert degrees to radians
          0 // We don't care about the length here, so we leave it at zero
        );
        

        // Get the local rotation by using LookRotation on a local directional vector
        Quaternion targetLocalRotation = Quaternion.LookRotation(targetLocalLookDir, Vector3.up);
        targetLocalRotationBody = Quaternion.LookRotation(targetLocalLookDirBody, Vector3.up);

        //local body rotation
        
        // Apply smoothing
        headBone.localRotation = Quaternion.Slerp(
          currentLocalRotation,
          targetLocalRotation,
          1 - Mathf.Exp(-headTrackingSpeed * Time.deltaTime)
        );
        // Apply smoothing
        BodyRotationClamped.localRotation = Quaternion.Slerp(
          currentBodyLocalRotation,
         new Quaternion( 0.70f - targetLocalRotationBody.x , targetLocalRotationBody.y, targetLocalRotationBody.z, targetLocalRotationBody.w),
          1 - Mathf.Exp(-headTrackingSpeed * Time.deltaTime)
        );
        

    }
    void Awake()
    {
        StartCoroutine(LegUpdateCoroutine());
    }

}
