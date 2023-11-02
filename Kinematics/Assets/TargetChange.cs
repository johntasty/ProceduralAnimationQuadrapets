using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetChange : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform[] DirectionPoints;
    public CreatureController[] targeting;
    public float changeTarget = 0f, timeSinceChange = 0f;
    public float[] delays;
    private int delaysCreatures;
    private void Start()
    {
        delays = new float[targeting.Length];
    }
    private void FixedUpdate()
    {
        delaysCreatures = 0;
        foreach (CreatureController animal in targeting)
        {
           
            if (delays[delaysCreatures] < 0f && !animal.Moving)
            {
                
                StartCoroutine(targetChanges(animal));
                delays[delaysCreatures] = changeTarget;
            }
            if (!animal.Moving)
            {
                delays[delaysCreatures] -= Time.fixedDeltaTime;
            }
            delaysCreatures += 1;
        }
        
    }
    private void changeDirection (CreatureController animal)
    {
        int lengthPoints = DirectionPoints.Length;
        Debug.Log(lengthPoints);
        int randomDirection = Random.Range(0, lengthPoints - 1);
        if(animal.target != DirectionPoints[randomDirection])
        {
            animal.target = DirectionPoints[randomDirection];
        }
        else
        {
            if(DirectionPoints[randomDirection + 1] != null)
            {
                animal.target = DirectionPoints[randomDirection + 1];
            }
            else
            {
                animal.target = DirectionPoints[randomDirection - 1];
            }
            
        }        
    }
    IEnumerator targetChanges(CreatureController animal)
    {
        changeDirection(animal);     
            
         yield return new WaitForSeconds(1.5f);

    }
}
