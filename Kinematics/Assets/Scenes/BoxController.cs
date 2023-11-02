using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[ExecuteInEditMode]
public class BoxController : MonoBehaviour
{
    //how fast we turn and move
    public float dist;
    public float desiredSufaceDist = -1f;
    public testingangle legs;
    public List<Vector3> legss;
    [SerializeField]
    public List<Transform> legpoints;
    public AnimationCurve sensitivityCurve;
    [SerializeField] float turnSpeed;
    [SerializeField] float moveSpeed;
    [Range(0, 1f)]
    public float distanceGround;
    public float bounceAmplitude;
    //how fast we reach max velocity
    [SerializeField] float turnAccelaration;
    [SerializeField] float moveAccelaration;
    //stay within this range from target
    [SerializeField] float minDistToTarget;
    [SerializeField] float maxDistToTarget;
    //max angle before begin turning
    [SerializeField] float maxAngToTarget;

    public Vector3 currentVelocity;
    public float currentAngularVelocity;
    //target we are tracking
    [SerializeField] Transform target;
    
    //reference to neck
    [SerializeField] Transform headBone;
    // Start is called before the first frame update
    [SerializeField] float headMaxTurnAngle;
    [SerializeField] float headTrackingSpeed;
    //get foot steppers
    [SerializeField] LegStepper frontLeftLegStepper;
    [SerializeField] LegStepper frontRightLegStepper;
    [SerializeField] LegStepper backLeftLegStepper;
    [SerializeField] LegStepper backRightLegStepper;
    ////get height of foots placements
    [SerializeField] Transform frontLeft;
    [SerializeField] Transform frontRight;
    [SerializeField] Transform backLeft;
    [SerializeField] Transform backRight;
    private Vector3 initBodyPos;
    [SerializeField] Transform body;
    [SerializeField] Transform bodyXandZ;
    [SerializeField] Transform render;

    [SerializeField] float bodyMaxTurnAngle;
    [SerializeField] float offset;
    private Vector3 bodyUp;

    IEnumerator LegUpdateCoroutine()
    {
        // Run continuously
        while (true)
        {

            // Try moving one diagonal pair of legs
            do
            {
                frontLeftLegStepper.TryMove();
                backRightLegStepper.TryMove();

                frontLeftLegStepper.stopMotion();
                backRightLegStepper.stopMotion();
                // Wait a frame
                yield return null;

                // Stay in this loop while either leg is moving.
                // If only one leg in the pair is moving, the calls to TryMove() will let
                // the other leg move if it wants to.
            } while (frontLeftLegStepper.Moving);

            // Do the same thing for the other diagonal pair

            do
            {
                frontRightLegStepper.TryMove();
                backLeftLegStepper.TryMove();
                frontRightLegStepper.stopMotion();
                backLeftLegStepper.stopMotion();
                yield return null;
            } while (backLeftLegStepper.Moving );

            

        }
    }

    void RootMotionUpdate()
    {
       
        //get direction toward target
        Vector3 towardTarget = target.position - transform.position;
        //vector toward target on local plane
        Vector3 towardTargetProjected = Vector3.ProjectOnPlane(towardTarget, transform.up);
        //get the angle from the box
        float angToTarget = Vector3.SignedAngle(transform.forward, towardTargetProjected, transform.up);

        float targetAngularVelocity = 0;

        //if withing the max angle leave velocity zero
        if (Mathf.Abs(angToTarget) > maxAngToTarget)
        {
            if(angToTarget > 0)
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
        currentAngularVelocity = Mathf.Lerp(
            currentAngularVelocity,
            targetAngularVelocity,
            1 - Mathf.Exp(-turnAccelaration * Time.deltaTime)
            );

        //rotate transform around y axis
        transform.Rotate(0, Time.deltaTime * currentAngularVelocity, 0, Space.World);      
       

        Vector3 targetVelocity = Vector3.zero;

        // Don't move if we're facing away from the target, just rotate in place
        if (Mathf.Abs(angToTarget) < 90)
        {
            float distToTarget = Vector3.Distance(transform.position, target.position);

            // If we're too far away, approach the target
            if (distToTarget > maxDistToTarget)
            {
                targetVelocity = moveSpeed * towardTargetProjected.normalized;
            }
            // If we're too close, reverse the direction and move away
            else if (distToTarget < minDistToTarget)
            {
                targetVelocity = moveSpeed * -towardTargetProjected.normalized;
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
  
    void Update()
    {
        RootMotionUpdate();
        //get height of body
        Vector3 up = Vector3.zero;
        Vector3 point, a, b, c;
        for (int i = 0; i < legpoints.Count; i++)
        {
            point = legpoints[i].position;
            distanceGround += transform.InverseTransformPoint(point).y;
            a = (transform.position - point).normalized;
            b = ((legpoints[i].position) - point).normalized;
            c = Vector3.Cross(a, b);
            up += c * sensitivityCurve.Evaluate(c.magnitude) + (legss[i] == Vector3.zero ? transform.forward : legss[i]);
            //grounded |= legs[i].legGrounded;

            Debug.DrawRay(point, a, Color.red, 0);

            Debug.DrawRay(point, b, Color.green, 0);

            Debug.DrawRay(point, c, Color.blue, 0);
        }
        up /= legpoints.Count;
        distanceGround /= legpoints.Count;
        dist = distanceGround;
        Debug.DrawRay(transform.position, up, Color.red, 0);        
        render.Translate(0, -(-distanceGround + desiredSufaceDist) * 0.5f, 0, Space.Self);
        var frontLeftlegMove = frontLeftLegStepper.transform.position.y;
        var frontRightLegMove = frontRightLegStepper.transform.position.y;
        var backLeftLegMove = backLeftLegStepper.transform.position.y;
        var backRightLegMove = backRightLegStepper.transform.position.y;
        var heightBody = (frontLeftlegMove + frontRightLegMove + backLeftLegMove + backRightLegMove) / 4;
       
        var backLegs = (backLeft.transform.position + backRight.transform.position) / 2;
        var frontLegs = (frontLeft.transform.position + frontRight.transform.position) / 2;
        Vector3 upDir = Vector3.up;
        Vector3 diffHei = backLegs - frontLegs;
        float riseMag = Vector3.Dot(diffHei, upDir);
        Vector3 riseVec = riseMag * upDir;
        float runMang = (diffHei - riseVec).magnitude;
        float slope = Mathf.Rad2Deg * Mathf.Atan2(riseMag, runMang);
     
        bodyXandZ.localRotation = Quaternion.Euler(-slope, 0, 0);
        //rotate transform around x axis
               
        // Store the current head rotation since we will be resetting it
        Quaternion currentLocalRotation = headBone.localRotation;
       
        // Reset the head rotation so our world to local space transformation will use the head's zero rotation. 
        // Note: Quaternion.Identity is the quaternion equivalent of "zero"
        headBone.localRotation = Quaternion.identity;
        //bodyXandZ.localRotation = Quaternion.identity;

        Vector3 targetWorldLookDir = target.position - headBone.position;
        Vector3 targetLocalLookDir = headBone.InverseTransformDirection(targetWorldLookDir - (Vector3.up * 5));
        
        // Apply angle limit
        targetLocalLookDir = Vector3.RotateTowards(
          Vector3.forward,
          targetLocalLookDir,
          Mathf.Deg2Rad * headMaxTurnAngle, // Note we multiply by Mathf.Deg2Rad here to convert degrees to radians
          0 // We don't care about the length here, so we leave it at zero
        );

       
        // Get the local rotation by using LookRotation on a local directional vector
        Quaternion targetLocalRotation = Quaternion.LookRotation(targetLocalLookDir, Vector3.up);
        
        //local body rotation

        // Apply smoothing
        headBone.localRotation = Quaternion.Slerp(
          currentLocalRotation,
         targetLocalRotation,
          1 - Mathf.Exp(-headTrackingSpeed * Time.deltaTime)
        );
        
        //float time = Mathf.PingPong(Time.time * offset, 0.1f);

        //body.localPosition = new Vector3(transform.position.x, time, transform.position.z);



    }
    void Awake()
    {
        legss.Add(legs.stepNormalone);
        legss.Add(legs.stepNormaltwo);
        legss.Add(legs.stepNormalthree);
        legss.Add(legs.stepNormalfour);
        initBodyPos = transform.localPosition;
        StartCoroutine(LegUpdateCoroutine());
    }

}
