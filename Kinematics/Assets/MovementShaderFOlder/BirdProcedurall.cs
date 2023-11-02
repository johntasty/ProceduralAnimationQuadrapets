using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdProcedurall : MonoBehaviour
{
    public int instanceCount = 100000;
    public Mesh instanceMesh;
    public Material instanceMaterial;

    public ComputeShader positionComputeShader;
    private int positionComputeKernelId;

    private ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;
    // Start is called before the first frame update
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    // Update is called once per frame

    void Start()
    {
        argsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
        CreateBuffers();
    }
    void Update()
    {
        UpdateBuffers();

        // Render
        Bounds ounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
        Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, instanceMesh.bounds, argsBuffer, 0, null);

    }
    void UpdateBuffers()
    {
        positionComputeShader.SetFloat("_Time", Time.time);

        /// TODO this only works with POT, integral sqrt vals
        int bs = instanceCount / 64;
        positionComputeShader.Dispatch(positionComputeKernelId, bs, 1, 1);
    }
    void CreateBuffers()
    {
        if (instanceCount < 1) instanceCount = 1;

        instanceCount = Mathf.ClosestPowerOfTwo(instanceCount);

        positionComputeKernelId = positionComputeShader.FindKernel("CSMainIndirect");
        instanceMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);

        // Positions & Colors
        if (positionBuffer != null) positionBuffer.Release();

        positionBuffer = new ComputeBuffer(instanceCount, 16);
      

        instanceMaterial.SetBuffer("positionBuffer", positionBuffer);

        // indirect args
        uint numIndices = (instanceMesh != null) ? (uint)instanceMesh.GetIndexCount(0) : 0;
        args[0] = numIndices;
        args[1] = (uint)instanceCount;
        argsBuffer.SetData(args);

        positionComputeShader.SetBuffer(positionComputeKernelId, "positionBuffer", positionBuffer);
        positionComputeShader.SetFloat("_Dim", Mathf.Sqrt(instanceCount));
    }
    void OnDisable()
    {
        if (positionBuffer != null) positionBuffer.Release();
        positionBuffer = null;


        if (argsBuffer != null) argsBuffer.Release();
        argsBuffer = null;
    }


}
