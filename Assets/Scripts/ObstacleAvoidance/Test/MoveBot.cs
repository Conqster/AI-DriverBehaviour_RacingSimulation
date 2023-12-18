using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


    public class MoveBot : MonoBehaviour
    {
        [SerializeField, Range(0.0f, 10.0f)] private float speed = 5.0f;
        [SerializeField] private Vector3 moveDir = Vector3.zero;
        [SerializeField, Range(25.0f, 90.0f)] private float coneAngle = 45.0f;

        public Transform target;
        public Vector3 currentVelocity = Vector3.zero;
        public Vector3 desiredVel = Vector3.zero;

        public bool obstacleInSight = false;
        public float steerTo = 0;

        private float steerInput = 0.0f;
        private Vector3 move;

        [Header("Part2")]
        [SerializeField, Range(0.0f, 90.0f)] private float angleOfVision = 25.0f;
        [SerializeField, Range(0.0f, 20.0f)] private float lengthOfVision = 5.0f;
        public float myWheelRot = 0.0f;
        public float useRot = 0;
        [SerializeField, Range(0.0f, 0.2f)] private float steerIntensity = 0.01f;

        private void Start()
        {
            currentVelocity = moveDir;
        }


        private void Update()
        {

            if (TryGetComponent<WheelColliderTest>(out WheelColliderTest myWheel))
            {
                myWheelRot = myWheel.SteeringAngle();
            }


            Perception(lengthOfVision, angleOfVision, steerIntensity, ref useRot);

            if(TryGetComponent<WheelColliderTest>(out WheelColliderTest wheel))
            {
                wheel.SetSteeringAngle(useRot);
            }


            move = moveDir * speed * Time.deltaTime;
            transform.Translate(move);

        }



        /// <summary>
        /// Used for the distance to vision
        /// What it see 
        /// 
        /// </summary>
        private void Perception(float visionLength, float visionAngle, float aviodanceStrength, ref float steerDirRatio)
        {

            Vector3 rightPoint = (Quaternion.Euler(0, visionAngle, 0) * transform.forward) * visionLength;
            Vector3 leftPoint = (Quaternion.Euler(0, -visionAngle, 0) * transform.forward) * visionLength;

            RaycastHit hit;
            bool midVision = ObstacleDetection(transform.forward, out hit, visionLength);
            bool leftWhisker = ObstacleDetection(leftPoint, out hit, visionLength);
            bool rightWhisker = ObstacleDetection(rightPoint, out hit, visionLength);


            if (leftWhisker)
                steerDirRatio += aviodanceStrength;

            if(rightWhisker)
                steerDirRatio -= aviodanceStrength;



            steerDirRatio = Mathf.Clamp(steerDirRatio, -1, 1);

            Debug.DrawRay(transform.position, transform.forward * visionLength, (midVision) ? Color.red : Color.blue);
            Debug.DrawRay(transform.position, leftPoint, (leftWhisker) ? Color.red : Color.blue);
            Debug.DrawRay(transform.position, rightPoint, (rightWhisker) ? Color.red : Color.blue);

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

    }


