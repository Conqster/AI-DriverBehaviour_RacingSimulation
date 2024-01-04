using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWhiskeyAngles : MonoBehaviour
{
    [SerializeField, Range(0.0f, 15.0f)] private float length = 5.0f;
    [SerializeField, Range(0.0f, 30.0f)] private float angle = 20.0f;
    public Transform carRear;


    private void OnDrawGizmos()
    {
        SideObstacle(length, angle);
    }


    private bool SideObstacle(float visionLength, float visionAngle ,WhiskeySearchType whiskeyType = (WhiskeySearchType)1)
    {

        //Vector3 rightPoint = (Quaternion.Euler(0, visionAngle, 0) *  transform.forward) * visionLength;
        //Vector3 leftPoint = (Quaternion.Euler(0, -visionAngle, 0) *  transform.forward) * visionLength;


        Vector3 rightPoint = (Quaternion.Euler(0, visionAngle, 0) * transform.forward);
        Vector3 leftPoint = (Quaternion.Euler(0, -visionAngle, 0) * transform.forward);

        RaycastHit hit;
        //bool midVision = ObstacleDetection(transform.forward, out hit, visionLength);
        bool leftWhisker = ObstacleDetection(carRear.position, leftPoint, out hit, visionLength, "Vehicle");
        bool rightWhisker = ObstacleDetection(carRear.position, rightPoint, out hit, visionLength, "Vehicle");

        Debug.DrawRay(carRear.position, leftPoint * visionLength, (leftWhisker) ? Color.red : Color.blue);
        Debug.DrawRay(carRear.position, rightPoint * visionLength, (rightWhisker) ? Color.red : Color.blue);


        return leftWhisker || rightWhisker;
    }

    private bool ObstacleDetection(Vector3 start, Vector3 dir, out RaycastHit hit, float range, string targetObstacleTag = "Obstacle")
    {
        bool detect = false;
        if (Physics.Raycast(start, dir, out hit, range))
        {
            if (hit.collider.CompareTag(targetObstacleTag))
            {
                detect = true;
            }
        }
        return detect;
    }
}
