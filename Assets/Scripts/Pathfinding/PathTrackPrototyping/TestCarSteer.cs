using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Car;

public class TestCarSteer : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private float steer;
    [SerializeField, Range(0.01f, 0.5f)] private float steerSensitivity = 0.01f;
    [SerializeField, Range(0f, 1f)] private float acceleration = 0.2f;
    private CarEngine carEngine;
    public CircuitPathTracker pathTracker;
    public bool performAction = false;

    public Vector3 myForward;
    public Vector3 trackerForward;
    public float currentSpeedRb;
    public float topSpeed = 30.0f;

    private Rigidbody rb;

    [Header("Debugger")]
    [SerializeField, Range(0.0f, 10.0f)] private float debugRange = 2.0f;


    private void Start()
    {
        carEngine = GetComponent<CarEngine>();
        rb = GetComponent<Rigidbody>();

    }


    private void Update()
    {
        currentSpeedRb = rb.velocity.magnitude;

        if(currentSpeedRb > topSpeed)
        {
            acceleration = 0.0f;
        }
        else
        {
            acceleration += 0.5f;
        }

        if (carEngine != null && performAction && pathTracker != null)
        {
            Vector3 localTarget = rb.transform.InverseTransformPoint(pathTracker.transform.position);
            float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

            if (!DriverParalleToTracker(pathTracker.transform.right))
            {
                //Steer(ref steer, steerSensitivity);
                steer = Mathf.Clamp(targetAngle * steerSensitivity, -1, 1) * Mathf.Sign(rb.velocity.magnitude);
            }
            else
            {
                if (DriverIsWithinBoundary())
                {
                    if (Mathf.Abs(steer) > 0.1)
                    {
                        if (steer > 0)
                        {
                            steer -= steerSensitivity;
                        }
                        else if (steer < 0)
                        {
                            steer += steerSensitivity;
                        }
                    }
                    else
                    {
                        steer = 0.0f;
                    }
                }
                else
                {
                    //Steer(ref steer, steerSensitivity);
                    steer = Mathf.Clamp(targetAngle * steerSensitivity, -1, 1) * Mathf.Sign(rb.velocity.magnitude);
                }


            }
            carEngine.Move(acceleration, 0.0f, steer);
        }

        
    }


    private bool DriverParalleToTracker(Vector3 targetRight)
    {
        //TO-DO: calculate the angle of rotation
        //       then increase and decrease the sensitivtity 
        //       based on how off, and have like a damping effect(ease in to rotations)

        float dot = Vector3.Dot(transform.right, targetRight);

        return (dot > 0.92f);
    }



    private bool DriverIsWithinBoundary()
    {
        bool inBoundary = true;


        float distanceOnX = transform.position.x - pathTracker.transform.position.x;

        if(Mathf.Abs(distanceOnX) > pathTracker.GetPathHalfWidth)
        {
            inBoundary = false;
        }


        return inBoundary; 
    }


    private void Steer(ref float steer, float intensity)
    {
        Vector3 vecToTarget = pathTracker.transform.position - transform.position;
        //Vector3 targetPos = pathTracker.transform.position;
        //Vector3 right = transform.TransformDirection(transform.right);
        Vector3 right = transform.right;
        
        if(Vector3.Dot(right, vecToTarget) > 0)
        {
            steer += intensity;
        }
        else if(Vector3.Dot(right, vecToTarget) < 0)
        {
            steer -= intensity;
        }

        steer = Mathf.Clamp(steer, -1.0f, 1.0f);
    }


    private void OnDrawGizmos()
    {

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (transform.forward * debugRange));

    }
}
