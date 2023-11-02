using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class IKScript : MonoBehaviour
{
    /// <summary>
    /// chain lenght
    /// </summary>

    public int ChainLength = 2;
    /// <summary>
    /// Target where the chain should bent
    /// </summary>

    public Transform Target;
    public Transform Pole;

    /// <summary>
    /// Iterations per update
    /// </summary>

    [Header("Solver Parameters")]
    public int Iterations = 10;
    /// <summary>
    /// Distance when solver stops
    /// </summary>
 
    public float Delta = 0.001f;

    [Range(0,1)]
    public float SnapBackStrength = 1f;
    /// <summary>
    /// Start is called before the first frame update
    /// </summary>    
    void Start()
    {
       
    } 
    // Update is called once per frame
    void Update()
    {

    }
    void OnDrawGizmos()
    {
        var current = this.transform;
        for(int i = 0; i < ChainLength && current != null && current.parent != null; i++)
        {
            var scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
            Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
            Handles.color = Color.green;
            Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
            current = current.parent;
        }
    }
   
}
