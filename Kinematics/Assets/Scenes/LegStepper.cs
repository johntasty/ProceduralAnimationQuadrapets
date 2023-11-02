using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class LegStepper : MonoBehaviour
{
    //The position and rotation we want
    [SerializeField] Transform homeTransform;
    [SerializeField] Transform body;
    [SerializeField] Transform targetTransform;
    [SerializeField] float legMaxAngle;
    [SerializeField] float headTrackingSpeed;
    //stay within this distance
    [SerializeField] float wantStepAtDistance;
    private float distLeft;
    //duration of step
    [SerializeField] float moveDuration;
    //distance to overshoot
    [SerializeField] float stepOvershootFraction;
    //is leg moving
    public bool Moving;
    public BoxController temp;
    // Start is called before the first frame update
   IEnumerator MoveToHome()
    {
        Moving = true;

        Vector3 startPoint = transform.position;
        Quaternion startRot = transform.rotation;

        Quaternion endRot = homeTransform.rotation;
               

        // Directional vector from the foot to the home position
        Vector3 towardHome = (homeTransform.position - transform.position);
        // Total distnace to overshoot by   
        float overshootDistance = wantStepAtDistance * stepOvershootFraction;
        Vector3 overshootVector = towardHome * overshootDistance;
        // Since we don't ground the point in this simplified implementation,
        // we restrict the overshoot vector to be level with the ground
        // by projecting it on the world XZ plane.
        overshootVector = Vector3.ProjectOnPlane(overshootVector, Vector3.up);

        // Apply the overshoot
        Vector3 endPoint = homeTransform.position + overshootVector;

        // We want to pass through the center point
        Vector3 centerPoint = (startPoint + endPoint) / 2;
        // But also lift off, so we move it up by half the step distance (arbitrarily)
        centerPoint += homeTransform.up * Vector3.Distance(startPoint, endPoint) / 2f;

        float timeElapsed = 0;
        
        do
        {
            timeElapsed += Time.deltaTime;
            float normalizedTime = timeElapsed / moveDuration;
            normalizedTime = Easing.Cubic.InOut(normalizedTime);
            
            // Quadratic bezier curve
            transform.position =
              Vector3.Lerp(
                Vector3.Lerp(startPoint, centerPoint, normalizedTime),
                Vector3.Lerp(centerPoint, endPoint, normalizedTime),
                normalizedTime
              );
                  
            
            transform.rotation = Quaternion.Slerp(startRot, endRot, normalizedTime);

            yield return null;
        }
        while (timeElapsed < moveDuration);
        
        Moving = false;
    }

    public void stopMotion()
    {
        Vector3 speed = temp.currentVelocity;
        if (speed == Vector3.zero)
        {
            distLeft = 0.5f;           
        }
        else
        {
            distLeft = wantStepAtDistance;
        }
    }
    public void TryMove()
    {
        
        // If we are already moving, don't start another move
        if (Moving) return;

        float distFromHome = Vector3.Distance(transform.position, homeTransform.position);
        //float stepDist = wantStepAtDistance;
        // If we are too far off in position or rotation
        if (distFromHome > distLeft)
        {
            // Start the step coroutine
            StartCoroutine(MoveToHome());
        }
        
       
    }
}
