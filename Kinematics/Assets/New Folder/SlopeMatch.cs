using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class SlopeMatch : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] legTest backLeftLegStepper;
    [SerializeField] legTest backRightLegStepper;
    public LayerMask ground;
    public float DistanceToGround;
    public float offset;
    public Transform backLeft;
    public Transform backRight;
    public Transform frontLeft;
    public Transform frontRight;
    public Transform bodyXandZ;
    public Transform body;
    public Transform ass;
    public Transform[] legs;
    public int[] nextLegTri;
    public AnimationCurve sensitivityCurve;
    public float desiredSufaceDist = -1f;
   
    Quaternion rot;
    public int smooth;
    public Transform shpereHere;
    public RaycastHit lr;
    public RaycastHit rr;
    public RaycastHit lf;
    public RaycastHit rf;
    public Vector3 upDir;

    private void Awake()
    {
        //StartCoroutine(LegUpdateCoroutine());
    }
    void Update()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position + (Vector3.up), Vector3.down);
        Vector3 bodymove = Vector3.zero;
        Vector3 sphereHead = Vector3.zero;

        if (Physics.Raycast(ray, out hit, DistanceToGround, ground))
        {
            bodymove = hit.point;
            
            //transform.position = tmep;
        }       
        Vector3 tmep = new Vector3(transform.localPosition.x, bodymove.y + offset, transform.localPosition.z);
        tmep.z = shpereHere.localPosition.z;
        //transform.localPosition = Vector3.Lerp(transform.localPosition, tmep, 0.1f);
        //transform.Translate(0, (-body.position.y), 0, Space.Self);
        Debug.DrawRay(this.transform.position + (Vector3.up), Vector3.down * DistanceToGround, Color.green);

        var backLegs = (backLeft.transform.position + backRight.transform.position) / 2;
        var frontLegs = (frontLeft.transform.position + frontRight.transform.position) / 2;
        Vector3 upDir = Vector3.up;
        Vector3 diffHei = backLegs - frontLegs;
        float riseMag = Vector3.Dot(diffHei, upDir);
        Vector3 riseVec = riseMag * upDir;
        float runMang = (diffHei - riseVec).magnitude;
        float slope = Mathf.Rad2Deg * Mathf.Atan2(riseMag, runMang);

        var newRot = Quaternion.Euler(-(slope) + 25f, 180f, 0);
        Quaternion currentLocalRotation = bodyXandZ.localRotation;
        bodyXandZ.localRotation = Quaternion.Slerp(currentLocalRotation, newRot, 2f);
        OrientHeight();
        
    }
    void OnDrawGizmosSelected()
    {
        // Display the explosion radius when selected
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, 0.5f);
    }

    private void OrientHeight()
    {

        Physics.Raycast(backLeft.position + Vector3.up , Vector3.down, out lr,ground);
        Physics.Raycast(backRight.position + Vector3.up , Vector3.down, out rr,ground);
        Physics.Raycast(frontLeft.position + Vector3.up , Vector3.down, out lf,ground);
        Physics.Raycast(frontRight.position + Vector3.up, Vector3.down, out rf,ground);
        upDir = (Vector3.Cross(rr.point - Vector3.up, lr.point - Vector3.up) +
              Vector3.Cross(lr.point - Vector3.up, lf.point - Vector3.up) +
              Vector3.Cross(lf.point - Vector3.up, rf.point - Vector3.up) +
              Vector3.Cross(rf.point - Vector3.up, rr.point - Vector3.up)
             ).normalized;
        body.up = upDir;
        //body.up = Vector3.Slerp(body.up, upDir, 0.1f);
        
        Vector3 bodyUp = Vector3.zero;
        RaycastHit hit;
        Ray ray = new Ray(body.position + Vector3.up, Vector3.down);
        if (Physics.Raycast(ray, out hit, DistanceToGround, ground))
        {
            bodyUp += hit.point;            
        }
        Vector3 height = new Vector3(body.transform.position.x, bodyUp.y, body.transform.position.z);
        body.transform.position = Vector3.Lerp(body.transform.position, height, 0.5f);               
    }
    
}
