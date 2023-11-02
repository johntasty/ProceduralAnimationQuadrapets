using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BirdSettings : ScriptableObject
{
    // Settings
    public float minSpeed = 2;
    public float maxSpeed = 5;
    public float maxSteerForce = 3;
}
