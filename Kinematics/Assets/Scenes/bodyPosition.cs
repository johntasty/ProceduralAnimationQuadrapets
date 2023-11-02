using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bodyPosition : MonoBehaviour
{
    [SerializeField] Transform frontLeftLegStepper;
    [SerializeField] Transform frontRightLegStepper;
    [SerializeField] Transform backLeftLegStepper;
    [SerializeField] Transform backRightLegStepper;
    [SerializeField] Transform boxPosition;
    [SerializeField] float offset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 bodyPosition = (frontLeftLegStepper.position + frontRightLegStepper.position + backLeftLegStepper.position + backRightLegStepper.position)/4;
        boxPosition.position = bodyPosition + (Vector3.up* offset);
    }
}
