using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace Car
{

    public enum DriveType
    {
        FourWheel,
        TwoWheel
    }



    public class CarEngine : MonoBehaviour
    {

        [SerializeField] private DriveType driveType = DriveType.TwoWheel;
        [SerializeField] private WheelCollider[] wheelCollider = new WheelCollider[4];
        [SerializeField] private GameObject[] wheelMesh = new GameObject[4];
        [SerializeField, Range(0.0f, 10000.0f)] private float fullTorqueAcrossAllWheel = 2000.0f;
        [SerializeField, Range(50.0f, 1000.0f)] private float downForce = 100.0f;
        [SerializeField, Range(0.0f, 1.0f)] private float tractionControl = 0.5f;
        [SerializeField, Range(5.0f, 90.0f)] private float maxSteerAngle = 25.0f;
        [SerializeField, Range(50.0f, 2000.0f)] private float reverseTorque = 150.0f;
        [SerializeField, Range(0.0f, 2.0f)] private float slipLimit = 0.4f;

        public float driveNow;
        public float testBrake = 0.0f;
        public float currentLtFrontBrake = 0.0f;
        public float currentVelocity;

        private Rigidbody rb;
        private float steerAngle;
        private float currentMotorTorque;

        private void Start()
        {
            InitWheelColliderAndMesh();
            rb = GetComponent<Rigidbody>();
            currentMotorTorque = fullTorqueAcrossAllWheel - (tractionControl * fullTorqueAcrossAllWheel);   
        }



        private void Update()
        {
            currentLtFrontBrake = wheelCollider[0].brakeTorque;

            currentVelocity = rb.velocity.magnitude;

            float v = Input.GetAxis("Vertical");
            float h = Input.GetAxis("Horizontal");
            float b = (Input.GetKey(KeyCode.Space)) ? 1.0f : 0.0f;
            driveNow = v;
            
            Move(v, b, h);
            if(Mathf.Abs(v) > 0 || Mathf.Abs(h) > 0)
            {
            }

           if(rb.velocity.magnitude > 0)
            {
                WheelVisualRot();
            }





        }

        public void Move(float acceleration, float brake, float steer)
        {


            //Clamping values 
            acceleration = Mathf.Clamp(acceleration, -1, 1);
            //brake = -1 * Mathf.Clamp(brake, -1, 0);
            brake = Mathf.Clamp(brake, -1, 1);
            //Set the steering for the front wheel 
            steerAngle = steer * maxSteerAngle;
            wheelCollider[0].steerAngle = steerAngle;
            wheelCollider[1].steerAngle = steerAngle;

            ApplyRotation(acceleration, brake);
            AddDownForce();

            TractionControl();
            WheelVisualRot();

        }

        public void UpdateParameter(float maxBrake)
        {
            reverseTorque = maxBrake;
        }


        private void ApplyRotation(float acceleration, float brake)
        {

            testBrake = brake;
            float thrustTorque;
            switch(driveType)
            {
                case DriveType.TwoWheel:

                    thrustTorque = acceleration * (currentMotorTorque / 2f);
                    wheelCollider[2].motorTorque = wheelCollider[3].motorTorque = thrustTorque;

                    break;

                case DriveType.FourWheel:

                    thrustTorque = acceleration * (currentMotorTorque / 4f);
                    foreach(var wheel in wheelCollider)
                    {
                        wheel.motorTorque = thrustTorque;
                    }

                    break;
            }

            //brake application

            if(brake > 0)
            {
                foreach (var wheel in wheelCollider)
                {
                    wheel.brakeTorque = brake * reverseTorque; // GOING TO NEED TO CHANGE THIS
                    //wheel.motorTorque = -reverseTorque * brake;
                }
            }

            foreach (var wheel in wheelCollider)
            {
                wheel.brakeTorque = brake * reverseTorque;
            }

        }



        private void TractionControl()
        {
            WheelHit hit;

            switch(driveType)
            {
                case DriveType.TwoWheel:

                    wheelCollider[2].GetGroundHit(out hit);
                    AdjustTorque(hit.forwardSlip);

                    wheelCollider[3].GetGroundHit(out hit);
                    AdjustTorque(hit.forwardSlip);

                    break;

                case DriveType.FourWheel:

                    foreach(var wheel in wheelCollider)
                    {
                        wheel.GetGroundHit(out hit);
                        AdjustTorque(hit.forwardSlip);
                    }

                    break;
            }
        }


        private void AdjustTorque(float forwardSlip)
        {
            if (forwardSlip >= slipLimit && currentMotorTorque >= 0)
            {
                currentMotorTorque -= 10 * tractionControl;
            }
            else
            {
                currentMotorTorque += 10 * tractionControl;
                if (currentMotorTorque > fullTorqueAcrossAllWheel)
                {
                    currentMotorTorque = fullTorqueAcrossAllWheel;
                }
            }
        }

        private void AddDownForce()
        {
            wheelCollider[0].attachedRigidbody.AddForce(-transform.up * downForce *
                                                         wheelCollider[0].attachedRigidbody.velocity.magnitude);
        }


        /// <summary>
        /// Adds visual rotation to wheel
        /// </summary>
        private void WheelVisualRot()
        {

            for (int i = 0; i < 4; i++)
            {
                Vector3 position;
                Quaternion quat;
                wheelCollider[i].GetWorldPose(out position, out quat);
                wheelMesh[i].transform.position = position;
                wheelMesh[i].transform.rotation = quat;
            }
        }

        private void InitWheelColliderAndMesh()
        {

            wheelCollider = GetComponentsInChildren<WheelCollider>();
            
            int childCount = transform.childCount;
            int childInChild = 0;   
            List<GameObject> temp = new List<GameObject>();
            for (int i = 0; i < childCount; i++)
            {

                childInChild = transform.GetChild(i).childCount;
                if(childInChild > 0)
                {
                    for(int j = 0; j < childInChild; j++)
                    {
                        if(transform.GetChild(i).GetChild(j).CompareTag("WheelMesh"))
                            temp.Add(transform.GetChild(i).GetChild(j).gameObject);
                    }
                }

                if (transform.GetChild(i).CompareTag("WheelMesh"))
                    temp.Add(transform.GetChild(i).gameObject);
            }
            wheelMesh = temp.ToArray(); 


        }

        public float GetWheelSteerAngle()
        {
            return wheelCollider[0].steerAngle;
        }

    }

}


