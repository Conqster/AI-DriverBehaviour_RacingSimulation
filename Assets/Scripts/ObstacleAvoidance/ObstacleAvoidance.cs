using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CarAI.ObstacleSystem
{
    public enum WhiskeySearchType
    {
        SingleOnly,
        WhiskeysOnly,
        ParallelSide,
        CentralWithParallel,
        CentralRayWithWhiskey
    }


    public class ObstacleAvoidance : MonoBehaviour
    {

        [SerializeField] private bool showDebugLine = true;
        [SerializeField] private bool showDeepSweep = true;
        [SerializeField, Range(0.0f, 10.0f)] private float posOffset = 2.0f;
        private Vector3 position = Vector3.zero;
        public Transform carRear;




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
            bool midVision = ObstacleDetection(transform.position, transform.forward, out hit, visionLength);
            bool leftWhisker = ObstacleDetection(transform.position, leftPoint, out hit, visionLength);
            bool rightWhisker = ObstacleDetection(transform.position, rightPoint, out hit, visionLength);


            if (leftWhisker)
                steerDirRatio += aviodanceStrength;

            if (rightWhisker)
                steerDirRatio -= aviodanceStrength;

            if (midVision)
            {
                int dir = SweepCheck(visionLength, visionAngle);
                steerDirRatio += aviodanceStrength * dir;
            }

            steerDirRatio = Mathf.Clamp(steerDirRatio, -1, 1);

            if (showDebugLine)
            {
                Debug.DrawRay(transform.position, transform.forward * visionLength, (midVision) ? Color.red : Color.blue);
                Debug.DrawRay(transform.position, leftPoint, (leftWhisker) ? Color.red : Color.blue);
                Debug.DrawRay(transform.position, rightPoint, (rightWhisker) ? Color.red : Color.blue);
            }

        }


        public bool VehicleSidePerception(out int side, out Rigidbody opponent, float visionLength, float visionAngle)
        {
            side = 1;
            opponent = null;

            if (carRear == null)
            {
                print("Get Car Rear Game Object");
                return false;
            }

            Vector3 rightPoint = (Quaternion.Euler(0, visionAngle, 0) * transform.forward);
            Vector3 leftPoint = (Quaternion.Euler(0, -visionAngle, 0) * transform.forward);

            RaycastHit hit;
            bool debug = true;
            if (debug)
            {
                bool midVision = ObstacleDetection(carRear.position, transform.forward, out hit, visionLength * 1.5f, "Vehicle");
                bool leftWhisker = ObstacleDetection(carRear.position, leftPoint, out hit, visionLength, "Vehicle");
                bool rightWhisker = ObstacleDetection(carRear.position, rightPoint, out hit, visionLength, "Vehicle");

                Debug.DrawRay(carRear.position, transform.forward * (visionLength * 1.5f), (midVision) ? Color.red : Color.cyan);
                Debug.DrawRay(carRear.position, leftPoint * visionLength, (leftWhisker) ? Color.red : Color.blue);
                Debug.DrawRay(carRear.position, rightPoint * visionLength, (rightWhisker) ? Color.red : Color.blue);
            }


            if (ObstacleDetection(carRear.position, transform.forward, out hit, visionLength * 1.5f, "Vehicle"))
            {
                side = SweepCheck(visionLength * 1.5f, visionAngle);
                opponent = hit.rigidbody;
                return true;
            }
            else if (ObstacleDetection(carRear.position, leftPoint, out hit, visionLength, "Vehicle"))
            {
                side = -1;
                opponent = hit.rigidbody;
                return true;
            }
            else if (ObstacleDetection(carRear.position, rightPoint, out hit, visionLength, "Vehicle"))
            {
                side = 1;
                opponent = hit.rigidbody;
                return true;
            }

            return false;
        }

        public bool DangerAhead(float visionLength, float visionAngle)
        {
            bool danger = false;

            RaycastHit hit;
            Vector3 rightPoint = (Quaternion.Euler(0, visionAngle, 0) * transform.forward) * visionLength;
            Vector3 leftPoint = (Quaternion.Euler(0, -visionAngle, 0) * transform.forward) * visionLength;
            danger = ObstacleDetection(transform.position, transform.forward, out hit, visionLength) ||
                     ObstacleDetection(transform.position, leftPoint, out hit, visionLength) ||
                     ObstacleDetection(transform.position, rightPoint, out hit, visionLength);

            //if (danger)
            //    print("There is an obstcale");

            return danger;
        }



        public bool ObstacleDetection(Vector3 start, Vector3 dir, out RaycastHit hit, float range, string targetObstacleTag = "Obstacle")
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

        private int SweepCheck(float range, float angle)
        {
            int countOnLeft = 0;
            int countOnRight = 0;

            for (float angleOffset = -angle; angleOffset <= angle; angleOffset += 5.0f)
            {
                //print(angleOffset);
                Vector3 whiskerCheck = (Quaternion.Euler(0.0f, angleOffset, 0.0f) * transform.forward) * range;
                RaycastHit hit;
                bool whiskerHit = ObstacleDetection(transform.position, whiskerCheck, out hit, range);

                if (whiskerHit)
                {
                    if (angleOffset < 0)
                    {
                        countOnLeft++;
                    }
                    else if (angleOffset > 0)
                    {
                        countOnRight++;
                    }
                }

                if (showDeepSweep)
                    Debug.DrawRay(transform.position, whiskerCheck, (whiskerHit) ? Color.red : Color.yellow);
            }

            return (countOnRight > countOnLeft) ? -1 : 1;
        }


        public void Braking(ref float brakingIntensity, float visionLength, float visionAngle, float brakeStrength)
        {
            //Need to add Distance to apply brake 
            //current Speed if Brake is need 


            RaycastHit hit;
            bool midVision = ObstacleDetection(transform.position, transform.forward, out hit, visionLength);

            if (midVision)
            {
                brakingIntensity += brakeStrength;
                Debug.DrawRay(transform.position, transform.forward * visionLength, Color.magenta);
            }

            brakingIntensity = Mathf.Clamp01(brakingIntensity);
        }

    }
}


