using UnityEngine;
using CarAI.Vehicle;
using CarAI.FuzzySystem;
using CarAI.ObstacleSystem;
using CarAI.Pathfinding;
using static StateMachine;


[System.Serializable]
public class DriverData
{
    [Header("Properties")]
    public Circuit circuit;
    public Transform transform;
    public Rigidbody rb;
    public DriverStateInformation stateInfo;
    public WhiskeySearchType behindRayType = WhiskeySearchType.CentralRayWithWhiskey;


    [Header("Car")]
    public CarEngine engine;
    [SerializeField] private float maxTorque = 200f;
    [SerializeField] private float maxBreak = 897f;
    [SerializeField, Range(0.0f, 1.0f)] public float steeringSensitivity = 0.01f;

    [Header("Perception")]
    [SerializeField, Range(0.0f, 20.0f)] public float visionLength = 9.0f;
    [SerializeField, Range(0.0f, 90.0f)] public float visionAngle = 25.0f;
    [SerializeField, Range(0.0f, 5.0f)] public float raycastUpOffset = 1.0f;
    [SerializeField, Range(0.0f, 25.0f)] public float rayLength = 15.0f;

    [Header("Utilities Condition")]
    [SerializeField] public bool canUseBlock;
    [SerializeField] public bool start = false;
    public float blockingCooldown = 0.0f;
    public float overtakeCooldown = 5.0f;   
    public bool goBerserk = false;



    [Header("External Information")]
    [SerializeField] public FuzzinessUtilityData currentFuzzinessUtilityData;   //if having another create a sturct then split in Driver Data
    [SerializeField] public int currentWaypointIndex = 0;
    public Transform currentWaypoint = null;
    public SpeedAdjust speedAdjust = SpeedAdjust.BrakeHard;
    public Vector3 currentTarget;

    [Header("Debugger")]
    [SerializeField] public float currentIntensity;
    [SerializeField] public float rotRatio;
    [SerializeField] public float currentDirection;
    [SerializeField] public float currentSteer;
    [SerializeField] public float normalSteerIntensity;
    [SerializeField] public float brakeTest;


    public DriverData(CarEngine engine, DriverStateInformation driverStateInfo, Circuit circuit, Rigidbody rb,Transform transform)
    {
        this.engine = engine;
        this.circuit = circuit;
        this.rb = rb;
        this.transform = transform;
        stateInfo = driverStateInfo;    
    }
}

[System.Serializable]
public struct StateMachineData
{
    public string stateName;
    public SM_Event stateEvent;
    public float stateDuration;
}



public class Ghost
{
    public GameObject obj;

    public Ghost(GameObject obj)
    {
        this.obj = obj;
    }

    public void Destroy()
    {
        Object.Destroy(obj);
    }
}


public class StateMachine
{


    public enum SM_Event
    {
        Enter,
        Update,
        Exit
    }
    



    private StateMachineData sm_stateData;
    protected StateMachine sm_transitTo;  //next state
    protected bool sm_transitionTriggered = false;  //triggered for transition
    protected SM_Event sm_event;   //current event in state 
    protected string sm_name;             // state name 
    protected float sm_duration;
    
    protected DriverData sm_driver;

    protected float currentSteerSensitity;
    private float lastObtacleTime = Mathf.Infinity;

    protected float accelerate;
    protected float brake;
    protected float steer;

    protected SpeedAllowance speedAllowance;
    protected DistanceAllowance distanceAllowance;
    protected DriverSpeedFuzzy driverSpeedFuzzy;
    protected FuzzinessUtilityDataCollection fuzzyUtilityCollection;
    protected bool useFuzzySystem = false;

    private CarGrounded carGrounded;
    protected bool canDrive = false;

    protected ObstacleAvoidance obstacleAvoidance;

    public StateMachine(DriverData driver)
    {
        sm_name = "Base State";
        sm_event = SM_Event.Enter;
        sm_driver = driver;
        sm_duration = 0.0f;

        fuzzyUtilityCollection = sm_driver.transform.GetComponent<FuzzinessUtilityDataCollection>();
        driverSpeedFuzzy = driver.transform.GetComponent<DriverSpeedFuzzy>();
        carGrounded = driver.transform.GetComponent<CarGrounded>();
        obstacleAvoidance = driver.transform.GetComponent<ObstacleAvoidance>();
    }
    
    
    public StateMachine Process()
    {
    
        switch (sm_event)
        {
            case SM_Event.Enter:
                Enter();
                break;
            case SM_Event.Update:
                Update();
                break;
            case SM_Event.Exit:
                Debug.Log("Prepare to exit");
                Exit();
                return sm_transitTo;
        }

        sm_stateData.stateName = sm_name;
        sm_stateData.stateDuration = sm_duration;
        sm_stateData.stateEvent = sm_event;
        return this;
    }
    
    
    /// <summary>
    /// special logic of thing to do -
    /// when enter new state, 
    /// if some parameter needs to be checked - configurations
    /// </summary>
    protected virtual void Enter()
    {
        sm_duration = 0.0f;

        useFuzzySystem = driverSpeedFuzzy.InitFuzzySystem(distanceAllowance, speedAllowance, sm_driver.currentFuzzinessUtilityData);
        //after all logic as be performed change state event to update
        sm_event = SM_Event.Update;
    }
    
    /// <summary>
    /// what specific state do on it event update every frame
    /// </summary>
    protected virtual void Update()
    {
        sm_duration += Time.deltaTime;
        sm_driver.blockingCooldown -= Time.deltaTime;
        sm_driver.blockingCooldown = Mathf.Clamp(sm_driver.blockingCooldown, -10.0f, 10.0f);

        Movement();

        if(canDrive)
        {
            if (useFuzzySystem)
                UpdateSpeed();

            if (carGrounded.NeedToFlip)
                carGrounded.Flip(sm_driver.currentTarget);
        }

        

        sm_driver.brakeTest = brake;

        sm_driver.currentSteer = steer;

        UpdateWaypointGoal();
    
        if (sm_transitionTriggered)
            return;
    
        sm_event = SM_Event.Update;
    }
    
    
    /// <summary>
    /// what state should do before transiting to next state 
    /// </summary>
    protected virtual void Exit()
    {
        sm_event = SM_Event.Exit;
    }
    
    
    protected virtual void TriggerExit(StateMachine transition)
    {
        sm_transitTo = transition;
        sm_transitionTriggered = true;
        sm_event = SM_Event.Exit;
    }
    
    
    public string GetSM_Name() => sm_name;
    public SM_Event GetSM_Event() => sm_event;

    public StateMachineData GetStateMachineData() => sm_stateData;

    private StateMachine GetStateMachine() => this; 


    /// <summary>
    /// Obstacle Avoidance function,
    /// Needed to be called by any state that requires it,
    /// To modify there use of it.
    /// </summary>
    /// <param name="visionLength"></param>
    /// <param name="visionAngle"></param>
    /// <param name="sensititity"></param>
    /// <param name="steer"></param>
    protected virtual void ObstacleAviodance(float visionLength, float visionAngle, float sensititity, ref float steer)
    {
        //CALCULATE ANGLE TO TARGET
        Vector3 localTarget = sm_driver.rb.transform.InverseTransformPoint(sm_driver.currentTarget);
        float distanceToTarget = Vector3.Distance(sm_driver.currentTarget, sm_driver.rb.transform.position);
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;


        //obstacleAhead = obstacleAvoidance.DangerAhead(visionLength, visionAngle);
        //Should ignore steering towards target if obstacle ahead
        if (obstacleAvoidance.DangerAhead(visionLength, visionAngle))
        {
            lastObtacleTime = 0.0f;
        }
        else
        {
            //steer based on the difference to target angle and use sensitivity.
            //steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(sm_driver.rb.velocity.magnitude);    //problem sets the angle directly, so it creates a snapping pos rotation


            AvoidedObstacle(ref currentSteerSensitity, sm_driver.steeringSensitivity);
            //SteerToTarget(targetAngle, normalSteerIntensity * 10f, ref steer);
            steer = Mathf.Clamp(targetAngle * currentSteerSensitity, -1, 1) * Mathf.Sign(sm_driver.rb.velocity.magnitude);    //problem sets the angle directly, so it creates a snapping pos rotation
            //float steer = 0.0f;
        }


        //float lookDistance = 0.0f;

        float maxAngle = 20.0f;
        float minAngle = 4.0f;
        
        float lookAngle = maxAngle * 0.5f;
        if(speedAllowance.min != 0 && speedAllowance.max != 0)
        {
            float speedRatio = Mathf.InverseLerp(speedAllowance.min, speedAllowance.max, sm_driver.rb.velocity.magnitude);
            lookAngle = Mathf.Lerp(minAngle,maxAngle, speedRatio);  
        }


        if(obstacleAvoidance.VehicleSidePerception(out int targetSide, out Rigidbody opponent, 7.0f, lookAngle))
        {
            //opposite side of target side hence (-targetSide) //hmm if 0;
            //steer += -targetSide * (sm_driver.steeringSensitivity * 100.0f);

            float possibleCornerAngle = CustomMath.NextThreePointIsAPossibleCorner(sm_driver.circuit.waypoints, (sm_driver.currentWaypointIndex + 1));
            float maxCornerAngle = 20.0f;

            int cornerIndex = (sm_driver.currentWaypointIndex + 1) % sm_driver.circuit.waypoints.Count;
            float distanceToCorner = Vector3.Distance(sm_driver.transform.position, sm_driver.circuit.waypoints[cornerIndex].position);
            float maxDistance = 2.0f;

            if(possibleCornerAngle > maxCornerAngle && distanceToCorner > maxDistance)
            {
                if (opponent != null)
                {
                    Vector3 vecToOpponent = (opponent.position - sm_driver.transform.position).normalized;

                    float dot = Vector3.Dot(vecToOpponent, sm_driver.transform.forward);
                    float angle = (Mathf.Acos(dot / vecToOpponent.magnitude * sm_driver.transform.forward.magnitude)) * Mathf.Rad2Deg;

                    float smallAngle = 4.0f;
                    float bigAngle = 20.0f;


                    float angleRatio = Mathf.InverseLerp(smallAngle, bigAngle, angle);
                    Debug.Log("Current Angle: " + angle + ", Ratio would be: " + angleRatio);


                    if (angle < 10)
                        brake = 1.0f;

                    if (angle < bigAngle)
                    {

                        steer += -targetSide * (sm_driver.steeringSensitivity * 100.0f); steer += -targetSide * (sm_driver.steeringSensitivity * 100.0f);
                        steer = steer * (angleRatio);
                    }


                }


                lastObtacleTime = 0.0f;
            }
            else
            {
                //use brake instead 
                sm_driver.engine.Move(accelerate, 1.0f, steer);
            }


        }
        else
        {
            AvoidedObstacle(ref currentSteerSensitity, sm_driver.steeringSensitivity);
            //SteerToTarget(targetAngle, normalSteerIntensity * 10f, ref steer);
            steer = Mathf.Clamp(targetAngle * currentSteerSensitity, -1, 1) * Mathf.Sign(sm_driver.rb.velocity.magnitude);    //problem sets the angle directly, so it creates a snapping pos rotation
            //float steer = 0.0f;
        }
        sm_driver.normalSteerIntensity = currentSteerSensitity;

        obstacleAvoidance.Perception(visionLength, visionAngle, sm_driver.steeringSensitivity * 100f, ref steer);

        obstacleAvoidance.Braking(ref brake, visionLength, visionAngle, 0.5f);

    }


    /// <summary>
    /// Updates waypoints,
    /// based on the distance to current waypoint
    /// And the circuit of the driver
    /// </summary>
    /// <returns></returns>
    protected Vector3 UpdateWaypointGoal()
    {
        if(sm_driver.currentWaypoint == null)
        {
            sm_driver.currentWaypoint = sm_driver.circuit.waypoints[sm_driver.currentWaypointIndex];
        }

        if(sm_driver.currentWaypoint)
        {
            float distanceToTarget = Vector3.Distance(sm_driver.currentWaypoint.position, sm_driver.rb.transform.position);

            float minDistance = 20.0f; //MIGTH CHANGE WAS 15.0F

            if (distanceToTarget < minDistance)
            {
                sm_driver.currentWaypointIndex++;
                if (sm_driver.currentWaypointIndex >= sm_driver.circuit.waypoints.Count)
                    sm_driver.currentWaypointIndex = 0;

                sm_driver.currentWaypoint = sm_driver.circuit.waypoints[sm_driver.currentWaypointIndex];
                //sm_driver.currentWaypointIndex = sm_driver.currentWaypointIndex; //debug info
            }
        }
        else
            return Vector3.zero;

        return sm_driver.currentWaypoint.position;
    }


    /// <summary>
    /// Updates the current target
    /// </summary>
    /// <param name="goal"></param>
    protected virtual void UpdateCurrentGoal(Vector3 goal)
    {
        sm_driver.currentTarget = goal;
    }


    /// <summary>
    /// Applys accelaration, 
    /// brake and steers the car
    /// </summary>
    private void Movement()
    {
        //MOVES THE CAR
        sm_driver.engine.Move(accelerate, brake, steer);
    }


    /// <summary>
    /// Ease normal Steering
    /// Back to full strength,
    /// After just avioded obstacle
    /// </summary>
    /// <param name="normalSteerIntensity"></param>
    /// <param name="steerIntensity"></param>
    private void AvoidedObstacle(ref float normalSteerIntensity, float steerIntensity)
    {
        lastObtacleTime += Time.deltaTime * 0.7f;        // increases overtime last obstacle avioded 
        float minIntensity = steerIntensity * 0.2f;

        normalSteerIntensity = Mathf.Lerp(minIntensity, steerIntensity, lastObtacleTime);
    }


    protected bool CheckBehind(out Rigidbody opponentRb, float distance, WhiskeySearchType rayType = (WhiskeySearchType)4, float checkAngle = 10.0f)
    {
        opponentRb = null;

        Vector3 rightWhiskey = Vector3.zero;
        Vector3 leftWhiskey = Vector3.zero;

        Vector3 origin = sm_driver.transform.position +
                 new Vector3(0.0f, sm_driver.raycastUpOffset, 0.0f);



        bool midCheck = false;
        bool rightCheck = false;
        bool leftCheck = false;


        switch (rayType)
        {
            case WhiskeySearchType.CentralRayWithWhiskey:
                rightWhiskey = (Quaternion.Euler(0, checkAngle, 0) * -sm_driver.transform.forward);
                leftWhiskey = (Quaternion.Euler(0, -checkAngle, 0) * -sm_driver.transform.forward);


                rightCheck = DetectVehicle(out opponentRb, origin, rightWhiskey, distance);
                leftCheck = DetectVehicle(out opponentRb, origin, leftWhiskey, distance);
                midCheck = DetectVehicle(out opponentRb, origin, -sm_driver.transform.forward, distance);

                Debug.DrawRay(origin, -sm_driver.transform.forward * distance, (midCheck) ? Color.red : Color.green);
                Debug.DrawRay(origin, rightWhiskey * distance, (rightCheck) ? Color.red : Color.green);
                Debug.DrawRay(origin, leftWhiskey * distance, (leftCheck) ? Color.red : Color.green);
                break;
            case WhiskeySearchType.WhiskeysOnly:
                rightWhiskey = (Quaternion.Euler(0, checkAngle, 0) * -sm_driver.transform.forward);
                leftWhiskey = (Quaternion.Euler(0, -checkAngle, 0) * -sm_driver.transform.forward);

                rightCheck = DetectVehicle(out opponentRb, origin, rightWhiskey, distance);
                leftCheck = DetectVehicle(out opponentRb, origin, leftWhiskey, distance);

                Debug.DrawRay(rightWhiskey, -sm_driver.transform.forward * distance, (rightCheck) ? Color.red : Color.green);
                Debug.DrawRay(leftWhiskey, -sm_driver.transform.forward * distance, (leftCheck) ? Color.red : Color.green);
                break;
            case WhiskeySearchType.SingleOnly:

                midCheck = DetectVehicle(out opponentRb, origin, -sm_driver.transform.forward, distance);

                Debug.DrawRay(origin, -sm_driver.transform.forward * distance, (midCheck) ? Color.red : Color.green);
                break;
            case WhiskeySearchType.ParallelSide:
                rightWhiskey = origin + (sm_driver.transform.right);
                leftWhiskey = origin - sm_driver.transform.right;

                rightCheck = DetectVehicle(out opponentRb, rightWhiskey, -sm_driver.transform.forward, distance);
                leftCheck = DetectVehicle(out opponentRb, leftWhiskey, -sm_driver.transform.forward, distance);

                Debug.DrawRay(rightWhiskey, -sm_driver.transform.forward * distance, (rightCheck) ? Color.red : Color.green);
                Debug.DrawRay(leftWhiskey, -sm_driver.transform.forward * distance, (leftCheck) ? Color.red : Color.green);
                break;
            case WhiskeySearchType.CentralWithParallel:
                rightWhiskey = origin + (sm_driver.transform.right);
                leftWhiskey = origin - sm_driver.transform.right;

                rightCheck = DetectVehicle(out opponentRb, rightWhiskey, -sm_driver.transform.forward, distance);
                leftCheck = DetectVehicle(out opponentRb, leftWhiskey, -sm_driver.transform.forward, distance);
                midCheck = DetectVehicle(out opponentRb, origin, -sm_driver.transform.forward, distance);

                Debug.DrawRay(origin, -sm_driver.transform.forward * distance, (midCheck) ? Color.red : Color.green);
                Debug.DrawRay(rightWhiskey, -sm_driver.transform.forward * distance, (rightCheck) ? Color.red : Color.green);
                Debug.DrawRay(leftWhiskey, -sm_driver.transform.forward * distance, (leftCheck) ? Color.red : Color.green);
                break;
        }

        if (opponentRb == null)
            return false;

        return midCheck || rightCheck || leftCheck;
    }


    private bool DetectVehicle(out Rigidbody target, Vector3 start, Vector3 dir, float distance)
    {
        RaycastHit hit; 
        target = null;  

        if(Physics.Raycast(start, dir, out hit, distance))
        {
            if(!hit.transform.CompareTag("Vehicle"))
            {
                target = hit.rigidbody;
                return true;
            }
        }
        return false;
    }



    private void UpdateSpeed()
    {
        float currentDistance = 0;

        Vector3 checkFrom = sm_driver.transform.position + (sm_driver.transform.up * 2.0f);

        Vector3 directionToTarget = sm_driver.currentTarget - sm_driver.transform.position;
        directionToTarget.Normalize();


        UpdateDistanceToCorner(ref currentDistance, checkFrom, directionToTarget);
        Debug.Log("The new Distance: " +  currentDistance); 

        float currentSpeed = sm_driver.rb.velocity.magnitude;

        currentSpeed = Mathf.Clamp(currentSpeed, speedAllowance.min, speedAllowance.max);
        currentDistance = Mathf.Clamp(currentDistance, distanceAllowance.min, distanceAllowance.max);

        driverSpeedFuzzy.Process(ref sm_driver.speedAdjust, currentSpeed, currentDistance);


        switch (sm_driver.speedAdjust)
        {
            case SpeedAdjust.FloorIt:

                accelerate = 1.0f;
                brake = 0.0f;

                break;
            case SpeedAdjust.SpeedUp:

                accelerate = 0.5f;
                brake = 0.0f;

                break;
            case SpeedAdjust.MaintainSpeed:

                accelerate = 0.0f;
                brake = 0.0f;

                break;
            case SpeedAdjust.SlowDown:

                accelerate = 0.0f;
                brake = 0.5f;

                break;
            case SpeedAdjust.BrakeHard:

                accelerate = 0.0f;
                brake = 1.0f;

                break;
        }

    }

    protected void UpdateDistanceToCorner(ref float useDistance, Vector3 start, Vector3 dir, bool debugRay = false)
    {
        #region Old Method with Raycast
        RaycastHit hit;

        if (Physics.Raycast(start, dir, out hit, Mathf.Infinity))
        {
            Debug.DrawRay(start, dir * hit.distance, Color.green);
            if (hit.transform.CompareTag("Wall"))
            {
                useDistance = hit.distance;
                if (debugRay)
                    Debug.DrawRay(start, dir * hit.distance, Color.magenta, 10.0f);
                return;
            }

            if (hit.transform.CompareTag("Track"))
            {
                Vector3 incomingVec = hit.point - start;
                Vector3 reflectVec = Vector3.Reflect(incomingVec, hit.normal);

                float caputureDistance = hit.distance;
                Debug.DrawRay(hit.point, reflectVec.normalized * hit.distance, Color.magenta);

                if (debugRay)
                    Debug.DrawRay(hit.point, reflectVec.normalized * hit.distance, Color.red, 10.0f);

                if (Physics.Raycast(hit.point, reflectVec.normalized, out hit, Mathf.Infinity))
                {
                    if (hit.transform.CompareTag("Wall"))
                    {
                        useDistance = Vector3.Distance(sm_driver.transform.position, hit.point);
                        return;
                    }
                    else
                    {
                        useDistance = caputureDistance * 2.0f;
                        return;
                    }
                }
                else
                {
                    useDistance = caputureDistance * 2.0f;
                    return;
                }
            }
        }
        #endregion

        #region New Method With NextThreeWayPoint

        //float possibleCornerAngle = CustomMath.NextThreePointIsAPossibleCorner(sm_driver.circuit.waypoints, (sm_driver.currentWaypointIndex + 1));
        //float maxCornerAngle = 10.0f;

   

        //if(possibleCornerAngle > maxCornerAngle)
        //{
        //    int cornerIndex = (sm_driver.currentWaypointIndex + 2) % sm_driver.circuit.waypoints.Count;
        //    float distanceToCorner = Vector3.Distance(sm_driver.transform.position, sm_driver.circuit.waypoints[cornerIndex].position);

        //    useDistance = distanceToCorner;
        //}
        //else
        //{
        //    RaycastHit hit;

        //    if (Physics.Raycast(start, dir, out hit, Mathf.Infinity))
        //    {
        //        Debug.DrawRay(start, dir * hit.distance, Color.green);
        //        if (hit.transform.CompareTag("Wall"))
        //        {
        //            useDistance = hit.distance;
        //            if (debugRay)
        //                Debug.DrawRay(start, dir * hit.distance, Color.magenta, 10.0f);
        //            return;
        //        }

        //        if (hit.transform.CompareTag("Track"))
        //        {
        //            Vector3 incomingVec = hit.point - start;
        //            Vector3 reflectVec = Vector3.Reflect(incomingVec, hit.normal);

        //            float caputureDistance = hit.distance;
        //            Debug.DrawRay(hit.point, reflectVec.normalized * hit.distance, Color.magenta);

        //            if (debugRay)
        //                Debug.DrawRay(hit.point, reflectVec.normalized * hit.distance, Color.red, 10.0f);

        //            if (Physics.Raycast(hit.point, reflectVec.normalized, out hit, Mathf.Infinity))
        //            {
        //                if (hit.transform.CompareTag("Wall"))
        //                {
        //                    useDistance = Vector3.Distance(sm_driver.transform.position, hit.point);
        //                    return;
        //                }
        //                else
        //                {
        //                    useDistance = caputureDistance * 2.0f;
        //                    return;
        //                }
        //            }
        //            else
        //            {
        //                useDistance = caputureDistance * 2.0f;
        //                return;
        //            }
        //        }
        //    }
        //}
        
        #endregion

    }
}

