using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Car;
using TMPro;

public class TestCarSteer : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private float steer;
    [SerializeField, Range(0.01f, 0.5f)] private float steerSensitivity = 0.01f;

    [SerializeField] private float acceleration = 0.2f;
    [SerializeField] private float brake = 0;

    private DriverSpeedFuzzy driverSpeedFuzzy;
    private DistanceAllowance distanceAllowance;
    private SpeedAllowance speedAllowance;
    [SerializeField] private SpeedAdjust speedAdjustment; 
    private CarEngine carEngine;
    public CircuitPathTracker pathTracker;
    public bool performAction = false;

    public Vector3 myForward;
    public Vector3 trackerForward;
    public float currentSpeedRb;
    public float topSpeed = 30.0f;

    private Rigidbody rb;
    public bool isParellel = false;
    public bool isInBoundary = false;

    public GameObject brakeLight;
    public bool passing = false;

    [Header("Debugger")]
    [SerializeField, Range(0.0f, 10.0f)] private float debugRange = 2.0f;
    [SerializeField] private TextMeshProUGUI parallel;
    [SerializeField] private TextMeshProUGUI inBoundary;
    public float valueX;
    public float currentTimeScale;
    public float hitLength;
    [SerializeField, Range(0.0f, 100.0f)] private float lengthTest;


    private void Start()
    {
        carEngine = GetComponent<CarEngine>();
        rb = GetComponent<Rigidbody>();

        driverSpeedFuzzy = GetComponent<DriverSpeedFuzzy>();

        speedAllowance.max = 30.0f;
        speedAllowance.min = 10.0f;
        distanceAllowance.max = 80.0f;
        distanceAllowance.min = 15.0f;

        driverSpeedFuzzy.InitFuzzySystem(distanceAllowance, speedAllowance);

    }


    private void UpdateSpeed()
    {
        float currentDistance = CheckDistance();
        float currentSpeed = rb.velocity.magnitude;

        //if(currentSpeed < speedAllowance.min) currentSpeed = speedAllowance.min;
        //if(currentDistance < distanceAllowance.min) currentDistance = distanceAllowance.min;

        lengthTest = currentDistance;

        currentSpeed = Mathf.Clamp(currentSpeed, speedAllowance.min, speedAllowance.max);   
        currentDistance = Mathf.Clamp(currentDistance, distanceAllowance.min, distanceAllowance.max);

        driverSpeedFuzzy.Process(ref speedAdjustment, currentSpeed, currentDistance);


        switch(speedAdjustment)
        {
            case SpeedAdjust.FloorIt:

                acceleration = 1.0f;
                brake = 0.0f;

                break;
            case SpeedAdjust.SpeedUp:

                acceleration = 0.5f;
                brake = 0.0f;

                break;
            case SpeedAdjust.MaintainSpeed:

                acceleration = 0.0f;
                brake = 0.0f;

                break;
            case SpeedAdjust.SlowDown:

                acceleration = 0.0f;
                brake = 0.5f;

                break;
            case SpeedAdjust.BrakeHard:

                acceleration = 0.0f;
                brake = 1.0f;

                break;
        }

    }


    private void Update()
    {
        passing = false;
        isParellel = DriverParalleToTracker(pathTracker.transform.right);
        isInBoundary = DriverIsWithinBoundary();

        DebuggingInfo();

        currentTimeScale = Time.timeScale;

        currentSpeedRb = rb.velocity.magnitude;

        UpdateSpeed();

        brakeLight.SetActive((brake > 0) ? true : false);   

        //if (currentSpeedRb > topSpeed)
        //{
        //    acceleration = 0.0f;
        //}
        //else
        //{
        //    acceleration += 0.5f;
        //}

        if (carEngine != null && performAction && pathTracker != null)
        {
            Vector3 localTarget = rb.transform.InverseTransformPoint(pathTracker.transform.position);
            float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

            if (!DriverParalleToTracker(pathTracker.transform.right))
            {
                //Steer(ref steer, steerSensitivity);
                steer = Mathf.Clamp(targetAngle * steerSensitivity * 0.6f, -1, 1) * Mathf.Sign(rb.velocity.magnitude);
            }
            else
            {
                if (DriverIsWithinBoundary())
                {

                    //Vector3 newTarget = new Vector3(pathTracker.transform.position.x,
                    //                                transform.position.y,
                    //                                pathTracker.transform.position.z);

                    //Vector3 newLocalTarget = rb.transform.InverseTransformPoint(newTarget);
                    //float angle = Mathf.Atan2(newTarget.x, newTarget.z) * Mathf.Rad2Deg;

                    //steer = Mathf.Clamp(angle * steerSensitivity, -1, 1) * Mathf.Sign(rb.velocity.magnitude);

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
            carEngine.Move(acceleration, brake, steer);
        }

        
    }


    private bool DriverParalleToTracker(Vector3 targetRight)
    {
        //TO-DO: calculate the angle of rotation
        //       then increase and decrease the sensitivtity 
        //       based on how off, and have like a damping effect(ease in to rotations)

        float dot = Vector3.Dot(transform.right, targetRight);

        return (dot > 0.95f);
    }



    private bool DriverIsWithinBoundary()
    {
        bool inBoundary = true;

        //Vector3 trackerX = transform.TransformPoint(pathTracker.transform.position);
        //Vector3 driverX = transform.TransformPoint(transform.position);

        ////float distanceOnX = transform.position.x - pathTracker.transform.position.x;
        //float distanceOnX = driverX.x - trackerX.x;

        //if(Mathf.Abs(distanceOnX) > pathTracker.GetPathHalfWidth)
        //{
        //    inBoundary = false;
        //}

        Vector3 relativePos = pathTracker.transform.InverseTransformPoint(transform.position);
        valueX = relativePos.x;

        if(Mathf.Abs(relativePos.x) > pathTracker.GetPathHalfWidth)
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


        Vector3 localTarget = rb.transform.InverseTransformPoint(pathTracker.transform.position);
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

        float ratio = Mathf.InverseLerp(30.0f, 60.0f, targetAngle);

        if (Vector3.Dot(right, vecToTarget) > 0)
        {
            steer += intensity * ratio;
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


        //CheckDistance();
        Debug.DrawRay(transform.position + (transform.up * 0.5f), transform.forward * lengthTest, Color.blue);
    }



    private void DebuggingInfo()
    {
        if (parallel != null && inBoundary != null)
        {
            string textIsParallel = (isParellel) ? "true" : "false";
            parallel.text = "Parallel: " + textIsParallel;

            string textIsInBound = (isInBoundary) ? "true" : "false";
            inBoundary.text = "InBoundary: " + textIsInBound;
        }
    }


    private float CheckDistance()
    {
        RaycastHit hit;
        float useDistance = 0.0f;
        float computeDistance = 0.0f;

        Vector3[] checkPoints = new Vector3[2];

        checkPoints[0] = transform.position + (transform.up * 0.5f);
        checkPoints[1] = transform.position + transform.up;

        foreach (var point in checkPoints)
        {
            if(Physics.Raycast(point, transform.forward, out hit, Mathf.Infinity))
            {
                passing = true;
                if(hit.transform.CompareTag("Wall"))
                {
                    Debug.DrawRay(point, transform.forward, Color.green);
                    computeDistance = hit.distance;
                }
                else if (hit.transform.CompareTag("Track"))
                {
                    Vector3 incomingVec = hit.point - point;
                    Vector3 reflectVec = Vector3.Reflect(incomingVec, hit.normal);

                    Debug.DrawLine(point, hit.point, Color.yellow);
                    Vector3 newPoint = hit.point;
                    if(Physics.Raycast(newPoint, reflectVec.normalized, out hit, Mathf.Infinity))
                    {
                        Debug.DrawLine(newPoint, hit.point, Color.magenta);
                        if(hit.transform.CompareTag("Wall"))
                        {
                            computeDistance = Vector3.Distance(transform.position, hit.point);
                        }
                    }
                    else
                    {
                        computeDistance = (hit.distance * 2f);
                    }
                }
            }
            
            if(computeDistance > useDistance)
                useDistance = computeDistance;
        }

        return useDistance;
    }

    private void Test(out RaycastHit hit, Vector3 start, Vector3 dir)
    {
        bool gotHit = false;

        if (Physics.Raycast(start, dir, out hit, Mathf.Infinity))
        {
            if (hit.transform.CompareTag("Wall"))
            {
                gotHit = true;
            }
        }

        Debug.DrawRay(start, dir * hit.distance, (gotHit) ? Color.red : Color.green);
    }
}
