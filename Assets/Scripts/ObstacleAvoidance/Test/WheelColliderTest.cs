using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WheelColliderTest : MonoBehaviour
{
    [SerializeField] private WheelCollider[] wheelColliders = new WheelCollider[4];
    [SerializeField] private GameObject[] wheelMeshes = new GameObject[4];

    [SerializeField, Range(0.0f, 90.0f)] private float maxRot;
    [SerializeField, Range(-1.0f, 1.0f)] private float dir;

    private Vector3 position;
    private Quaternion quat;



    private void Update()
    {

        //foreach (var wheel in wheelColliders)
        //{
        //    wheel.steerAngle = dir * maxRot;
        //}

        for(int i = 0; i < 4; i++)
        {
            wheelColliders[i].GetWorldPose(out position, out quat);
            wheelMeshes[i].transform.position = position;
            wheelMeshes[i].transform.rotation = quat;
        }

        print(wheelColliders[0].steerAngle);

    }



    public float SteeringAngle()
    {
        return wheelColliders[0].steerAngle;
    }

    public void SetSteeringAngle(float rate)
    {
        foreach(var wheel in wheelColliders)
        {
            wheel.steerAngle = rate * maxRot;
        }
    }
}
