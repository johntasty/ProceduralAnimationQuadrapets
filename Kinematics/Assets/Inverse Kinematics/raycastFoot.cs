using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class raycastFoot : MonoBehaviour
{
    [Range(-1f, 1f)]
    public float distanceGround;
    public float len;
    public LayerMask ground;
    public Transform rootBone;
    public Vector3 targetPos;
    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        //Ray ray = new Ray(this.transform.position + Vector3.up, Vector3.down * 10);
        Vector3 left45 = (Vector3.up * -len);

        if (Physics.Raycast(rootBone.position + rootBone.right * distanceGround, left45, out hit, 3f, ground))
        {
            targetPos = hit.point;
            
        }
       
        Debug.DrawRay(rootBone.position + rootBone.right * distanceGround, left45, Color.red);
    }
}
