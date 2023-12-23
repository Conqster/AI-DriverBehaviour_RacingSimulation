using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WaypointTestBot : MonoBehaviour
{
    [SerializeField, Range(0.0f, 10.0f)] private float moveSpeed;
    [SerializeField, Range(0.0f, 5.0f)] private float reachedWaypointThreshold = 2.0f;
    [SerializeField, Range(0.0f, 10.0f)] private float waypointBoundary = 3.0f;

    private Circuit circuit;
    public int waypointIndex = 0;
    private Vector3 target;
    private float steerValue = 0;

    [Header("Debugger")]
    [SerializeField, Range(1, 50)] private float length = 5;

    private void Start()
    {
        circuit = GameObject.FindGameObjectWithTag("Circuit").GetComponent<Circuit>();
    }


    // Update is called once per frame
    void Update()
    {
        //get and assign target
        target = circuit.waypoints[waypointIndex].position;

        if(Vector3.Distance(transform.position, target) < reachedWaypointThreshold)
        {
            waypointIndex = (waypointIndex + 1) % circuit.waypoints.Count;
        }


        Vector3 vectorToTarget = target - transform.position;
        Vector3 direction = vectorToTarget.normalized;

        float desiredSteer = Vector3.Cross(transform.forward, direction).y > 0 ? 1 : -1;

        if (Mathf.Abs(transform.position.x - circuit.waypoints[waypointIndex].position.x) <= waypointBoundary ||
    Mathf.Abs(transform.position.z - circuit.waypoints[waypointIndex].position.z) <= waypointBoundary)
        {
            // The driver is within the boundary, so it doesn't need to steer towards the waypoint
            steerValue = 0;
        }
        else
        {
            steerValue = Mathf.Lerp(steerValue, desiredSteer, Time.deltaTime * 20.0f);

        }


        transform.Rotate(0, steerValue * 50.0f * Time.deltaTime, 0);

        //Vector3 localTarget = transform.InverseTransformPoint(target);
        //float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

        //transform.Rotate(0, targetAngle * Time.deltaTime * 5.0f, 0);



        //Move Bot
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

    }





    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(target, reachedWaypointThreshold);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target, waypointBoundary);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * length);
    }
}
