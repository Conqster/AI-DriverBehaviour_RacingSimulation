using UnityEngine;
using CarAI.Vehicle;
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
    public float currentDistance = 0;
    [Space]
    public int functionCounter = 0;
    public int functionEndCounter = 0;
    public float gameTimeScale;

    private void Start()
    {
        //Time.timeScale = 0.5f;

        carEngine = GetComponent<CarEngine>();
        rb = GetComponent<Rigidbody>();

        driverSpeedFuzzy = GetComponent<DriverSpeedFuzzy>();

        speedAllowance.max = 30.0f;
        speedAllowance.min = 10.0f;
        distanceAllowance.max = 80.0f;
        distanceAllowance.min = 0.0f;

        driverSpeedFuzzy.InitFuzzySystem(distanceAllowance, speedAllowance);

    }


    private void UpdateSpeed()
    {

        gameTimeScale = Time.timeScale;

        CheckDistance(ref currentDistance);
        float currentSpeed = rb.velocity.magnitude;

        //if(currentSpeed < speedAllowance.min) currentSpeed = speedAllowance.min;
        //if(currentDistance < distanceAllowance.min) currentDistance = distanceAllowance.min;

        currentSpeed = Mathf.Clamp(currentSpeed, speedAllowance.min, speedAllowance.max);   
        currentDistance = Mathf.Clamp(currentDistance, distanceAllowance.min, distanceAllowance.max);

        driverSpeedFuzzy.Process(ref speedAdjustment, currentSpeed, currentDistance);

        acceleration = 1.0f;


        switch (speedAdjustment)
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

            //steer = Mathf.Clamp(targetAngle * steerSensitivity * 0.6f, -1, 1) * Mathf.Sign(rb.velocity.magnitude);

            if(!DriverParalleToTracker(pathTracker.transform.right) || !DriverIsWithinBoundary())
            {
                steer = Mathf.Clamp(targetAngle * steerSensitivity * 0.6f, -1, 1) * Mathf.Sign(rb.velocity.magnitude);
            }
            else
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

            #region Old Method
            //if (!DriverParalleToTracker(pathTracker.transform.right))
            //{
            //    //Steer(ref steer, steerSensitivity);
            //    steer = Mathf.Clamp(targetAngle * steerSensitivity * 0.6f, -1, 1) * Mathf.Sign(rb.velocity.magnitude);
            //}
            //else
            //{
            //    if (DriverIsWithinBoundary())
            //    {

            //        //Vector3 newTarget = new Vector3(pathTracker.transform.position.x,
            //        //                                transform.position.y,
            //        //                                pathTracker.transform.position.z);

            //        //Vector3 newLocalTarget = rb.transform.InverseTransformPoint(newTarget);
            //        //float angle = Mathf.Atan2(newTarget.x, newTarget.z) * Mathf.Rad2Deg;

            //        //steer = Mathf.Clamp(angle * steerSensitivity, -1, 1) * Mathf.Sign(rb.velocity.magnitude);

            //        if (Mathf.Abs(steer) > 0.1)
            //        {
            //            if (steer > 0)
            //            {
            //                steer -= steerSensitivity;
            //            }
            //            else if (steer < 0)
            //            {
            //                steer += steerSensitivity;
            //            }
            //        }
            //        else
            //        {
            //            steer = 0.0f;
            //        }
            //    }
            //    else
            //    {
            //        //Steer(ref steer, steerSensitivity);
            //        steer = Mathf.Clamp(targetAngle * steerSensitivity, -1, 1) * Mathf.Sign(rb.velocity.magnitude);
            //    }


            //}
            #endregion

            carEngine.Move(acceleration, brake, steer);
        }

        
    }


    private bool DriverParalleToTracker(Vector3 targetRight)
    {
        //TO-DO: calculate the angle of rotation
        //       then increase and decrease the sensitivtity 
        //       based on how off, and have like a damping effect(ease in to rotations)

        float dot = Vector3.Dot(transform.right, targetRight);

        return (dot > 0.98f);
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
        //fDebug.DrawRay(transform.position + (transform.up * 0.5f), transform.forward * lengthTest, Color.blue);
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


    private float CheckDistance(ref float useDistance)
    {
        //RaycastHit hit;
        //float useDistance = 0.0f;
        float computeDistance1 = 0.0f;
        int computeLevel1 = 0;

        //float computeDistance2 = 0.0f;
        //int computeLevel2 = 0;

        Vector3[] checkPoints = new Vector3[2];

        //checkPoints[0] = transform.position + (transform.up * 0.5f);
        checkPoints[0] = transform.position + (transform.up * 2.0f);

        Vector3 directionToTarget = pathTracker.transform.position - transform.position;
        directionToTarget.Normalize();

        Vector3 useDirection = pathTracker.transform.forward;

        computeDistance1 = ComputeDistance(out computeLevel1, checkPoints[0], useDirection);
        //computeDistance2 = ComputeDistance(out computeLevel2, checkPoints[1], transform.forward);

        //if(computeLevel1 < computeLevel2)
        //    useDistance = computeDistance1;

        //if(computeLevel2 < computeLevel1)
        //    useDistance = computeDistance2;

        //if(computeLevel1 ==  computeLevel2)
        //{
        //    useDistance = computeDistance1;
        //    if (useDistance < computeDistance2)
        //        useDistance = computeDistance2;
        //}

        useDistance = computeDistance1;
         
        return useDistance;
    }

    private float ComputeDistance(out int objLevel, Vector3 start, Vector3 dir)
    {
        RaycastHit hit;


        //TO-Do: should be directionn to the target / tracker 
        // But problem  will arise when the car is travelling too fats than the tracker 


        if(Physics.Raycast(start, dir, out hit, Mathf.Infinity))
        {
            Debug.DrawRay(start, dir * hit.distance, Color.green);
            if(hit.transform.CompareTag("Wall"))
            {
                objLevel = 0;
                return hit.distance;
            }

            if(hit.transform.CompareTag("Track"))
            {
                Vector3 incomingVec = hit.point - start;
                Vector3 reflectVec = Vector3.Reflect(incomingVec, hit.normal);

                float caputureDistance = hit.distance;
                Debug.DrawRay(hit.point, reflectVec.normalized * hit.distance, Color.magenta);
                if(Physics.Raycast(hit.point, reflectVec.normalized, out hit, Mathf.Infinity))
                {
                    objLevel = 1;

                    if(hit.transform.CompareTag("Wall"))
                    {
                        return Vector3.Distance(transform.position, hit.point); 
                    }
                    else 
                        return (caputureDistance * 2.0f);
                }
                else
                {
                    objLevel = 3;
                    return (caputureDistance * 2.0f);
                }
            }
        }

        objLevel = 4;
        return 0;   
    }
}
