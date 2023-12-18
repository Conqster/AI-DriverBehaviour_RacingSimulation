using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidance : MonoBehaviour
{

    [SerializeField] private bool showDebugLine = true;
    [SerializeField] private bool showDeepSweep = true;
    [SerializeField, Range(0.0f, 10.0f)] private float posOffset = 2.0f;
    private Vector3 position = Vector3.zero;


    /// <summary>
    /// Used for the distance to vision
    /// What it see 
    /// 
    /// </summary>
    public void Perception(float visionLength, float visionAngle, float aviodanceStrength, ref float steerDirRatio)
    {

        Vector3 rightPoint = (Quaternion.Euler(0, visionAngle, 0) * transform.forward) * visionLength;
        Vector3 leftPoint = (Quaternion.Euler(0, -visionAngle, 0) * transform.forward) * visionLength;

        RaycastHit hit;
        bool midVision = ObstacleDetection(transform.forward, out hit, visionLength);
        bool leftWhisker = ObstacleDetection(leftPoint, out hit, visionLength);
        bool rightWhisker = ObstacleDetection(rightPoint, out hit, visionLength);


        if (leftWhisker)
            steerDirRatio += aviodanceStrength;

        if (rightWhisker)
            steerDirRatio -= aviodanceStrength;

        if(midVision)
        {
            int dir = SweepCheck(visionLength, visionAngle);
            steerDirRatio += aviodanceStrength * dir;
        }

        steerDirRatio = Mathf.Clamp(steerDirRatio, -1, 1);

        if(showDebugLine)
        {
            Debug.DrawRay(transform.position, transform.forward * visionLength, (midVision) ? Color.red : Color.blue);
            Debug.DrawRay(transform.position, leftPoint, (leftWhisker) ? Color.red : Color.blue);
            Debug.DrawRay(transform.position, rightPoint, (rightWhisker) ? Color.red : Color.blue);
        }

    }


    public bool DangerAhead(float visionLength, float visionAngle)
    {
        bool danger = false;

        RaycastHit hit;
        Vector3 rightPoint = (Quaternion.Euler(0, visionAngle, 0) * transform.forward) * visionLength;
        Vector3 leftPoint = (Quaternion.Euler(0, -visionAngle, 0) * transform.forward) * visionLength;
        danger = ObstacleDetection(transform.forward, out hit, visionLength) ||
                 ObstacleDetection(leftPoint, out hit, visionLength) ||
                 ObstacleDetection(rightPoint, out hit, visionLength);

        //if (danger)
        //    print("There is an obstcale");

        return danger;
    }



    private bool ObstacleDetection(Vector3 dir, out RaycastHit hit, float range)
    {
        bool detect = false;
        if (Physics.Raycast(transform.position, dir, out hit, range))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                detect = true;
            }
        }
        return detect;
    }


    private int SweepCheck(float range, float angle)
    {
        int countOnLeft = 0;
        int countOnRight = 0;
        int desiredDir = 0;

        for (float angleOffset = -angle; angleOffset <= angle; angleOffset += 5.0f)
        {
            //print(angleOffset);
            Vector3 whiskerCheck = (Quaternion.Euler(0.0f, angleOffset, 0.0f) * transform.forward) * range;
            RaycastHit hit;
            bool whiskerHit = ObstacleDetection(whiskerCheck, out hit, range);

            if(whiskerHit)
            {
                if(angleOffset < 0)
                {
                    countOnLeft++;
                }
                else if(angleOffset > 0)
                {
                    countOnRight++; 
                }
            }

            if(showDeepSweep)
                Debug.DrawRay(transform.position, whiskerCheck, (whiskerHit) ? Color.red : Color.yellow);
        }
 
        desiredDir = (countOnRight > countOnLeft) ? -1 : 1;
        return desiredDir;
    }


    public void Braking(ref float brakingIntensity,float visionLength, float visionAngle, float brakeStrength)
    {
        //Need to add Distance to apply brake 
        //current Speed if Brake is need 


        RaycastHit hit;
        bool midVision = ObstacleDetection(transform.forward, out hit, visionLength);

        if (midVision)
        {
            brakingIntensity += brakeStrength;
            Debug.DrawRay(transform.position, transform.forward * visionLength, Color.magenta);
        }

        brakingIntensity = Mathf.Clamp01(brakingIntensity);
    }
}
