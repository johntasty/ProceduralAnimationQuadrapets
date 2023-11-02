using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class raycastposition : MonoBehaviour
{   //the object we want to raycast

    public Transform ikPoleTarget;
    public Transform ikTarget;
    public float stepRadius = 0.25f;

    public Vector3 optimalRestingPosition = Vector3.forward;

    public Vector3 restingPosition
    {
        get
        {
            return transform.TransformPoint(optimalRestingPosition);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        ikTarget.position = restingPosition;
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 direction = transform.position - ikPoleTarget.position;
        RaycastHit hit;
        if (Physics.SphereCast(ikPoleTarget.position, stepRadius, direction, out hit, direction.magnitude * 2f))
        {
            Debug.DrawLine(ikPoleTarget.position, hit.point, Color.green, 0f);
            //position = hit.point;
            //stepNormal = hit.normal;
            //legGrounded = true;
        }
        else
        {
            Debug.DrawLine(ikPoleTarget.position, restingPosition, Color.red, 0f);
            //position = restingPosition;
            //stepNormal = Vector3.zero;
            //legGrounded = false;
        }

    }
}
