using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public class BirdManager : MonoBehaviour
{
    [SerializeField]
    ComputeShader computeShader;
   
    const int threadGroupSize = 1024;
    public BirdSettings settings;
    BirdController[] birds;
    int birdAmount;
    private int positionComputeKernelId;
    public float changeTarget = 0f, timeSinceChange = 0f;
    
    void Awake()
    {
        birds = FindObjectsOfType<BirdController>();
        foreach (BirdController bir in birds)
        {
            bir.Initialize(settings, Vector3.zero);
        }
        birdAmount = birds.Length;
    }
   
    private void FixedUpdate()
    {
        for (int i = 0; i < birdAmount; i++)
        {
            birds[i].UpdateBird();
        }

        if (changeTarget < 0f)
        {
            for (int z = 0; z < birdAmount; z++)
            {
                birds[z].target = DirectionChange();

            }
            changeTarget = timeSinceChange;
        }
        changeTarget -= Time.fixedDeltaTime;
    }
    Vector3  DirectionChange()
    {
        Vector3 newDir;
        float angleXZ = Random.Range(0, Mathf.PI);
        // Limited max steepness of ascent/descent in the vertical direction
        float angleY = Random.Range(-Mathf.PI / 48f, Mathf.PI / 48f);
        // Calculate direction
        newDir = new Vector3(Mathf.Sin(angleXZ) * ( 500), Mathf.Sin(angleY)  * 100, Mathf.Cos(angleXZ) * 500);
        
        return newDir;
    }

    public struct BirdData
    {
        public Vector3 position;
        public Vector3 target;
        public Vector3 velocity;
        public Vector3 forward;
        public static int Size
        {
            get
            {
                return sizeof(float) * 3 * 4;
            }
        }

    }
    
    }
