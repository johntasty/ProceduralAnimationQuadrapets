using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteInEditMode]
public class CreatureController : MonoBehaviour {
    [SerializeField] public float turnAccelaration;
    [SerializeField] float moveAccelaration;
    //stay within this range from target
    [SerializeField] float minDistToTarget;
    [SerializeField] float maxDistToTarget;
    public float DistanceToGround;
    public LayerMask ground;
    public float offset;
    //max angle before begin turning
    [SerializeField] public float maxAngToTarget;

    public Vector3 currentVelocity;
    public Vector3 test;
    public float currentAngularVelocity;
    public float singSwitch = 1f;

    public float TailMaxTurn;
    public float TailMinTurn;
    public float TailDefaultHeight = 24.266f;

    [SerializeField] public float turnSpeed;
    [SerializeField] float moveSpeed;

    [SerializeField] float bodySpeedX;
    [SerializeField] float bodyOffset;
    [SerializeField] float TailOffset;
    //reference to neck
    [SerializeField] Transform headBone;

    [SerializeField] float headMaxTurnAngle;
    [SerializeField] float headTrackingSpeed;
    //target we are tracking
    [SerializeField] public Transform target;
    [SerializeField] Transform bone;
    [SerializeField] Transform TailBone = null;
    [SerializeField] Transform armature;
    [SerializeField] Transform body;


    [SerializeField] public Transform[] legs;
    [SerializeField] public legTest[] legsMove;
    private float angleAverage;
    public float lastStep = 0;
    public int index;
    public float timeBetweenSteps = 0.25f;
    public bool MoveTail;
    public bool TailShake;
    public float TailSpeed;
    public Vector3 upDir;
    public bool Moving = false;
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
        if(currentVelocity.magnitude > 0.5f && Mathf.Abs(angToTarget) > 1f)
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
    void FixedUpdate()
    {        
        RootMotionUpdate();
        //OrientHeight();
        if (MoveTail)
        {
            tryShake();
        }
        if(currentVelocity != Vector3.zero)
        {
            Moving = true;
        }
        else { Moving = false; }

            // Store the current head rotation since we will be resetting it
            Quaternion currentLocalRotation = headBone.localRotation;

        // Reset the head rotation so our world to local space transformation will use the head's zero rotation. 
        // Note: Quaternion.Identity is the quaternion equivalent of "zero"
        headBone.localRotation = Quaternion.identity;
        //bodyXandZ.localRotation = Quaternion.identity;

        Vector3 targetWorldLookDir = target.position - headBone.position;
        Vector3 targetLocalLookDir = headBone.InverseTransformDirection(targetWorldLookDir - (Vector3.up * bodyOffset));

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
              
        
    }
    private void OrientHeight()
    {
        //get cross products of all legs
        Vector3 a = (legs[0].position ) - (legs[1].position );
        Vector3 b = (legs[2].position ) - (legs[0].position );
        Vector3 c = (legs[3].position ) - (legs[2].position );
        Vector3 d = (legs[0].position ) - (legs[3].position );

        //normal of each Vector
        Vector3 crossBA = Vector3.Cross(b, a);
        Vector3 crossCB = Vector3.Cross(c, b);
        Vector3 crossDC = Vector3.Cross(d, c);
        Vector3 crossAD = Vector3.Cross(a, d);
               
        //clamp the y since the legs sometimes go below surface
        Vector3 newUp = (crossBA + crossCB + crossDC + crossAD);
        newUp.y = Mathf.Clamp(newUp.y , 0, newUp.y );
        //rotate the armature on the z and x set y to transforms as to not interfere with direction of movement
        armature.up = Vector3.Lerp(armature.up, newUp * offset, Time.deltaTime * bodySpeedX);
        armature.localRotation = armature.localRotation * Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        
        Vector3 bodyUp = Vector3.zero;
        RaycastHit hit;
        Ray ray = new Ray(transform.position + Vector3.up, Vector3.down * DistanceToGround);
        if (Physics.Raycast(ray, out hit, DistanceToGround, ground))
        {
            bodyUp += hit.point;
        }
        Debug.DrawRay(transform.position + Vector3.up, Vector3.down * DistanceToGround, Color.red);
       //just a second fail safe for slope on the transform to be completely snug on the surface
        var backLegs = (legs[1].transform.position + legs[0].transform.position) / 2;
        var frontLegs = (legs[3].transform.position + legs[2].transform.position) / 2;
        Vector3 upDir = Vector3.up;
        Vector3 diffHei = backLegs - frontLegs;
        float riseMag = Vector3.Dot(diffHei, upDir);
        Vector3 riseVec = riseMag * upDir;
        float runMang = (diffHei - riseVec).magnitude;
        float slope = Mathf.Rad2Deg * Mathf.Atan2(riseMag, runMang);
        //raise the height of the entire object so legs dont fold up
       
        Vector3 height = new Vector3(transform.position.x, bodyUp.y, transform.position.z);
       
        transform.rotation =  Quaternion.Euler( slope * singSwitch , transform.rotation.eulerAngles.y, 0);
        transform.position = height;
    }
    
    private void Start()
    {        
        //StartCoroutine(LegUpdateCoroutine());
    }

   
    void tryShake()
    {
        
        var dirRight = (legs[0].position - bone.position);
        var dirLeft = ( legs[1].position - bone.position);
        float AngleRadSinged = Vector3.SignedAngle( dirRight, bone.forward, Vector3.up);
        float AngleRadSingedLeft = Vector3.SignedAngle( dirLeft, bone.forward, Vector3.up);
      //get the angle average of the 2 back feet in relation to the root bone
        angleAverage = ((AngleRadSinged) + (AngleRadSingedLeft)) / 2;
        float assClampedAverage = Mathf.Clamp(angleAverage, TailMinTurn, TailMaxTurn);

        //Vector to rotate towards        
        Quaternion rotatedLineTest = Quaternion.AngleAxis(angleAverage, Vector3.up);
               
        //create a vector at a position and rotation of the angles, creates the shake ass effect
        Vector3 result = rotatedLineTest * (bone.forward * TailOffset);
        result = result + bone.position ;

        Quaternion targetEyeRotation = Quaternion.LookRotation(
           bone.position - result, // toward target
           bone.up
         );
       
         
        if(TailBone != null)
        {
            TailBone.position = result;
        }
        //bone.rotation = Quaternion.Lerp(
        //    bone.rotation,
        //    targetEyeRotation,
        //    1 - Mathf.Exp(-TailSpeed * Time.deltaTime)
        //);


        float assClampROtation = bone.localEulerAngles.y;
      if(assClampROtation >= TailMaxTurn)
        {
            assClampROtation -= 360f;
        }
        float assClampedY = Mathf.Clamp(assClampROtation, TailMinTurn, TailMaxTurn);
        bone.localEulerAngles = new Vector3(
        TailDefaultHeight,
            assClampedY,
            0f);
        
    }
    IEnumerator LegUpdateCoroutine()
    {
        // Run continuously
        while (true)
        {
            //yield return null;
            do
            {
                legsMove[0].FootMove();                
                legsMove[1].FootMove();
                // Wait a frame
                yield return null;
            } while (legsMove[0].Moving);
            do
            {
                legsMove[2].FootMove();              
                legsMove[3].FootMove();
                // Wait a frame
                yield return null;
            } while (legsMove[2].Moving);



        }
    }
   
}

