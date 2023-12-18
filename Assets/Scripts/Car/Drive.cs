using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Drive : MonoBehaviour
{
    public WheelCollider wheelCollider;
    public float torque = 200.0f;
   

    public GameObject wheelMesh;
    [SerializeField, Range(0.0f, 60.0f)] private float maxSteerAngle = 30.0f;
    [SerializeField, Range(0.0f, 1000.0f)] private float maxBreakTorque = 500f;
    [SerializeField] private bool canSteer = false;


    private void Start()
    {
        wheelCollider = GetComponent<WheelCollider>();
    }




    private void Update()
    {

        //ApplyRotation(accelerate, steer);
    }


    public void ApplyRotation(float accelerationDir, float steer, float brake)
    {
        if(canSteer)
        {
            steer = Mathf.Clamp(steer, -1, 1);
            float steerRate = steer * maxSteerAngle;
            wheelCollider.steerAngle = steerRate;
        }
        else
        {
            brake = Mathf.Clamp(brake, -1, 1) * maxBreakTorque;
            wheelCollider.brakeTorque = brake;  
        }
   

        accelerationDir = Mathf.Clamp(accelerationDir, -1, 1);
        float thrustTorque = accelerationDir * torque;
        //if(brake <= 0)
            wheelCollider.motorTorque = thrustTorque;



        //Add physical rotation and displacement to wheel gameObject
        Quaternion quat;
        Vector3 position;
        wheelCollider.GetWorldPose(out position, out quat);
        wheelMesh.transform.position = position;
        wheelMesh.transform.rotation = quat;  
    }


    public void UpdateParameters(float torque, float breakTorque)
    {
        this.torque = torque;
        maxBreakTorque = breakTorque;   
    }
}
