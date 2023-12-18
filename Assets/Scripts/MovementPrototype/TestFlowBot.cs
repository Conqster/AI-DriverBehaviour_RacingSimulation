using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFlowBot : MonoBehaviour
{
    public Vector3 moveDir;
    private Vector3 desiredDir;
    [SerializeField, Range(0.0f, 10.0f)] private float moveSpeed;
    private float steerValue = 0.0f;    

    public FlowFieldGrid flowField;

    private void Update()
    {



        //if(flowField.GetFlowDir(transform.position, out desiredDir))
        //{
        //    //desiredDir.Normalize();
        //    //float desiredSteer = Vector3.Cross(transform.forward, desiredDir).y > 0 ? 1 : -1;

        //    //steerValue = Mathf.Lerp(steerValue, desiredSteer, Time.deltaTime * 20.0f);
        //    //transform.Rotate(0, steerValue * 50.0f * Time.deltaTime, 0);

        //    Vector3 localTarget = transform.InverseTransformPoint(desiredDir);
        //    float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

        //    transform.Rotate(0, targetAngle * Time.deltaTime * 5.0f, 0);
        //}

        if(flowField.GetFlowDir(transform.position, out desiredDir))
        {
            Vector3 localTarget = transform.InverseTransformDirection(desiredDir);
            float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

            transform.Rotate(0, targetAngle * Time.deltaTime * 5.0f, 0);
        }


        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }


    

}
