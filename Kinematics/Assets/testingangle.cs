using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class testingangle : MonoBehaviour
{
    public Transform cubetwo;
    public Transform cubethree;
    public Transform cubeFour;
    public Transform cubeFive;
    public Vector3 stepNormalone;
    public Vector3 stepNormaltwo;
    public Vector3 stepNormalthree;
    public Vector3 stepNormalfour;
    [Range(0, 1f)]
    public float distanceGround;
 
    //public Transform cubethree;
    public LayerMask hitMe;
    // Update is called once per frame
    void Update()
    {
        var left45 = ( transform.forward - transform.right).normalized;
        var right15offset = (transform.forward + transform.right).normalized;
        RaycastHit hit;
        
        Ray ray = new Ray(transform.position, (transform.up.normalized + (left45) / 2 + transform.forward.normalized / 2));
        if (Physics.Raycast(ray, out hit, distanceGround + 2.5F))
        {
            var rayPos = hit.point;
            stepNormalone = hit.normal;
            rayPos += hit.normal * distanceGround;
            Vector3 homePoint = new Vector3(rayPos.x, rayPos.y, rayPos.z);
            cubetwo.position = homePoint;
        }

        RaycastHit hitTwo;
        Ray rayTwo = new Ray(transform.position, (transform.up.normalized + (right15offset / 2) + transform.forward.normalized / 2));
        if (Physics.Raycast(rayTwo, out hitTwo, distanceGround + 2.5F))
        {
            var rayPosTwo = hitTwo.point;
            stepNormaltwo = hitTwo.normal;
            rayPosTwo += hitTwo.normal * distanceGround;
            Vector3 homePointTwo = new Vector3(rayPosTwo.x, rayPosTwo.y, rayPosTwo.z);
            cubethree.position = homePointTwo;
        }
        RaycastHit hitThree;
        Ray rayThree = new Ray(transform.position, (transform.up.normalized - (right15offset) / 2 - transform.forward.normalized / 2));
        if (Physics.Raycast(rayThree, out hitThree, distanceGround + 2F))
        {
            var rayPosThree = hitThree.point;
            stepNormalthree = hitThree.normal;
            rayPosThree += hitThree.normal * distanceGround;
            Vector3 homePointThree = new Vector3(rayPosThree.x, rayPosThree.y, rayPosThree.z);
            cubeFour.position = homePointThree;
        }
        RaycastHit hitFour;
        Ray rayFour = new Ray(transform.position, (transform.up.normalized - (left45 / 2) - transform.forward.normalized / 2));
        if (Physics.Raycast(rayFour, out hitFour, distanceGround + 2F))
        {
            var rayPosFour = hitFour.point;
            stepNormalfour = hitFour.normal;
            rayPosFour += hitFour.normal * distanceGround;
            Vector3 homePointFour = new Vector3(rayPosFour.x, rayPosFour.y, rayPosFour.z);
            cubeFive.position = homePointFour;
        }
        Debug.DrawRay(transform.position, (transform.up.normalized + (left45)/2 + transform.forward.normalized/2) * 1.5F, Color.red);
        Debug.DrawRay(transform.position, (transform.up.normalized + (right15offset / 2) + transform.forward.normalized /2) * 1.5F, Color.green);
        Debug.DrawRay(transform.position, (transform.up.normalized - (right15offset) / 2 - transform.forward.normalized / 2) * 1.5F, Color.blue);
        Debug.DrawRay(transform.position, (transform.up.normalized - (left45 / 2) - transform.forward.normalized /2) * 1.5F, Color.black);
        
    }
  
}
