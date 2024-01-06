using CarAI.Pathfinding;
using UnityEngine;

public class CircuitPathTracker : MonoBehaviour
{
    public Circuit circuit;
    public int currentWaypointIndex = 0;
    private Vector3 target = Vector3.zero;

    [Header("Behaviour")]
    [SerializeField, Range(0.0f, 300.0f)] private float currentSpeed = 5.0f;
    [SerializeField, Range(0.0f, 300.0f)] private float maxSpeed = 100.0f;
    [SerializeField, Range(0.0f, 100.0f)] private float rotSpeed = 2.0f;
    [SerializeField, Range(0.0f, 10.0f)] private float reachedTargetTreshold = 2.0f;

    [Header("Path Behaviour")]
    [SerializeField, Range(0.0f, 10.0f)] private float pathHalfWidth = 5.0f;
    [SerializeField, Range(0.0f, 1.0f)] private float pathWidthDangerRatio = 0.2f;
    [SerializeField] private Transform user;
    [SerializeField, Range(0.0f, 20.0f)] private float maxDistance = 2.0f;

    [Header("Debugger")]
    [SerializeField] private Color arrowColour = Color.cyan;
    [SerializeField] private bool drawPathWidth = true;
    [SerializeField, Range(1.0f, 5.0f)] private float drawWidthThickness = 2.5f;
    [SerializeField] private Color pathWidthColour = Color.blue;
    [SerializeField] private Color pathWidthDangerColour = Color.red;
    [SerializeField] private bool testing = false;


    public float GetPathHalfWidth
    {
        get { return pathHalfWidth; }   
    }

    public float GetPathWidthDangerRatio
    {
        get { return pathWidthDangerRatio; }
    }


    private void Start()
    {
        if(circuit != null)
            target = circuit.waypoints[currentWaypointIndex].position;   
    }

    private void Update()
    {
        if(!testing)
        {

            if (circuit != null)
            {
                Movement();


                float distance = Mathf.Abs(Vector3.Distance(target, transform.position));
                if (distance < reachedTargetTreshold)
                {
                    currentWaypointIndex = (currentWaypointIndex + 1) % circuit.waypoints.Count;
                    target = circuit.waypoints[currentWaypointIndex].position;
                }
            }



            //TO-DO: later when intergated with drivers
            //       dynamic speed increse and decrease
            //       if too close to driver increase speed gradually 
            //       if too far away from driver gradually decrese
            //       the rate of change, will vary based on this distance 
            //       something like 

            //float rateOfChange = 0.0f;     //alter based on the state of the game
            //float currentSpeed = 5.0f;     // currentSpeed would be the current tracker speed;
            //currentSpeed += rateOfChange;

            if (currentSpeed < maxSpeed)
            {
                currentSpeed += 5.0f /** 4 * Time.deltaTime*/;
            }




            if(Vector3.Distance(transform.position, user.position) > maxDistance)
            {
                currentSpeed -= 10.0f;
                currentSpeed = Mathf.Clamp(currentSpeed, 0.0f, Mathf.Infinity);
            }
        }
 
    }


    private void Movement()
    {
        Vector3 vecToTarget = target - transform.position;
        vecToTarget.Normalize();

        transform.position += vecToTarget * Time.deltaTime * currentSpeed;


        //TO-DO: rotate towards target
        //float angle = Vector3.Angle(transform.forward, vecToTarget);

        Quaternion rot = Quaternion.LookRotation(vecToTarget);

        if(Quaternion.Angle(transform.rotation, rot) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotSpeed * Time.deltaTime);
        }
    }




    private void OnDrawGizmos()
    {
        DirectVisual();

        if(circuit != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target, reachedTargetTreshold);   
        }

        if(drawPathWidth)
        {
            PathSize();

            if(user != null)
            {
                Vector3 vecToUser = ((user.position - transform.position).normalized);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, transform.position + (vecToUser * maxDistance));
            }

        }
    }

    private void DirectVisual()
    {
        UnityEditor.Handles.color = arrowColour;
        UnityEditor.Handles.ArrowHandleCap(0, transform.position, transform.rotation, 1.5f, EventType.Repaint);
    }


    private void PathSize()
    {
        UnityEditor.Handles.color = pathWidthColour;

        Vector3 safeLeftPoint = transform.position - (transform.right * (pathHalfWidth * (1 - pathWidthDangerRatio)));
        Vector3 safeRightPoint = transform.position + (transform.right * (pathHalfWidth * (1 - pathWidthDangerRatio)));


        UnityEditor.Handles.DrawLine(safeLeftPoint, safeRightPoint, drawWidthThickness);

        UnityEditor.Handles.color = pathWidthDangerColour;

        //Vector3 leftPoint = transform.position - (transform.right * pathHalfWidth);
        //Vector3 rightPoint = transform.position + (transform.right * pathHalfWidth);

        Vector3 leftSidePL = safeLeftPoint - (transform.right * (pathHalfWidth * pathWidthDangerRatio));
        Vector3 leftSidePR = safeLeftPoint;

        UnityEditor.Handles.DrawLine(leftSidePL, leftSidePR, drawWidthThickness);

        Vector3 rightSidePL = safeRightPoint;
        Vector3 rightSidePR = safeRightPoint + (transform.right * (pathHalfWidth * pathWidthDangerRatio));

        UnityEditor.Handles.DrawLine(rightSidePL, rightSidePR, drawWidthThickness);
    }

}
