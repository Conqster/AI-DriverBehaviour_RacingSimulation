using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class MoveAgent : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField, Range(0.0f, 50.0f)] private float speed;
    [SerializeField, Range(0.0f, 10.0f)] private float distanceToWaypoint;
    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    private int waypointIndex = 0;


    [Space][Header("Debug")]
    [SerializeField, Range(0.0f, 50.0f)] private float acceleration = 8.0f;
    [SerializeField, Range(0.0f, 100.0f)] private float maxSpeed = 40.0f;
    [SerializeField, Range(0.0f, 50.0f)] private float minSpeed = 8.0f;
    [SerializeField, Range(0.0f, 50.0f)] private float approachingDist = 2.0f;
    public TextMeshProUGUI displayDistance;
    public TextMeshProUGUI displaySpeed;
    public bool canChangeSpeed;



    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();   
        waypointIndex = 0;
    }


    private void Update()
    {
        agent.acceleration = acceleration;  
        if(agent.remainingDistance < distanceToWaypoint)
        {
            MoveTowardWayPoint();
        }

        if (canChangeSpeed)
            SpeedChange();
        else
            agent.speed = speed;

        DisplayDistanceToWaypoint();
    }


    private void SpeedChange()
    {
        agent.speed = (agent.remainingDistance < approachingDist) ? minSpeed : maxSpeed;


    }


    private void DisplayDistanceToWaypoint()
    {

        string distanceInfo = "N/A";
        string speedInfo = "N/A";
        
        if (displayDistance || agent)
        {
            float distance = agent.remainingDistance;
            distanceInfo = distance.ToString("0.0") + " meters";

        }

        if(agent)
        {
            //speedInfo = agent.speed.ToString("0.0") + " speed";
            speedInfo = agent.velocity.magnitude.ToString("0.0") + " speed";
        }

        displaySpeed.text = speedInfo;
        displayDistance.text = distanceInfo;
    }


    private void MoveTowardWayPoint()
    {
        if (waypointIndex >= waypoints.Count)
            waypointIndex = 0;

        Vector3 position = waypoints[waypointIndex].position;
        agent.SetDestination(position); 
        waypointIndex++;
    }



    private void OnDrawGizmos()
    {
        if(waypoints != null)
        {
             Gizmos.color = Color.red;
            foreach(var waypoint in waypoints)
            {
                Gizmos.DrawWireSphere(waypoint.position, distanceToWaypoint);

                
                Vector3 directionToward = transform.position - waypoint.position;
                float distanceRatio = approachingDist / directionToward.magnitude;
                Gizmos.DrawLine(waypoint.position, waypoint.position + (directionToward * distanceRatio));
            }
        }
    }

}
