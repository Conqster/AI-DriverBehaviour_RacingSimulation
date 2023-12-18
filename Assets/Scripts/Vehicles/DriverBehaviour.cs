using Car;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CarEngine))]
public class DriverBehaviour : MonoBehaviour
{
    private CarEngine engine;
    private Circuit circuit;
    private Rigidbody rb;
    private ObstacleAvoidance obstacleAvoidance;

    public GameObject brakeLight;

    private StateMachine driverSM;
    public StateMachineData driverSMData;

    private int currentWaypointIndex = 0;
    //[SerializeField] private float maxTorque = 200f;
    [SerializeField] private float maxBreak = 500f;


    [SerializeField, Range(0.0f, 1.0f)] private float steeringSensitivity = 0.01f;
    [SerializeField, Range(0.0f, 20.0f)] private float visionLength = 3.4f;
    [SerializeField, Range(0.0f, 90.0f)] private float visionAngle = 25.0f;

    public DriverData driverData;


    private Vector3 target;
    public float currentSteer;
    private float steer = 0.0f;

    [Header("Debugger")]
    [SerializeField] private bool obstacleAhead = false;

    private void Start()
    {
        engine = GetComponent<CarEngine>();
        obstacleAvoidance = GetComponent<ObstacleAvoidance>();  
        engine.UpdateParameter(maxBreak);
        circuit = GameObject.FindGameObjectWithTag("Circuit").GetComponent<Circuit>();
        target = circuit.waypoints[currentWaypointIndex].position;
        rb = GetComponent<Rigidbody>();
        driverData = new DriverData(engine, circuit, rb, obstacleAvoidance, brakeLight);
        driverSM = new SM_DefaultState(driverData);
    }

    private void FixedUpdate()
    {
        //OldScript();
        driverSM = driverSM.Process();
        driverSMData = driverSM.GetStateMachineData();
    }





    private void OldScript()
    {
        currentSteer = engine.GetWheelSteerAngle();
        Vector3 localTarget = transform.InverseTransformPoint(target);
        float distanceToTarget = Vector3.Distance(target, transform.position);
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;


        //constant acceleration
        float accelerate = 1f;
        float brake = 0.0f;

        obstacleAhead = obstacleAvoidance.DangerAhead(visionLength, visionAngle);
        //Should ignore steering towards target if obstacle ahead
        if (!obstacleAvoidance.DangerAhead(visionLength, visionAngle))
        {
            //steer based on the difference to target angle and use sensitivity.
            steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(rb.velocity.magnitude);
            //float steer = 0.0f;
        }

        obstacleAvoidance.Perception(visionLength, visionAngle, steeringSensitivity * 100f, ref steer);
        //print("steer value: " + steer);


        if (distanceToTarget < 15 && rb.velocity.magnitude > 15.0f)
        {
            brake = (15 - distanceToTarget / 15);
            //brake = 0.5f;
        }

        //SPEED DANGER NEED TO BREAK 
        if (rb.velocity.magnitude > 25.0f && distanceToTarget < 45.0f && distanceToTarget > 15.0f)
        {
            brake = 1.0f;
        }

        obstacleAvoidance.Braking(ref brake, visionLength, visionAngle, 0.5f);
        //brake = 1.0f;
        engine.Move(accelerate, brake, steer);



        if (brake > 0.0f)
        {
            brakeLight.SetActive(true);
            brake -= 0.5f;     //CURRENT TO PREVENT BRAKING TOO HARD CHANGE LATER
        }
        else
        {
            brakeLight.SetActive(false);
        }


        if (distanceToTarget < 15.0f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= circuit.waypoints.Count)
                currentWaypointIndex = 0;

            target = circuit.waypoints[currentWaypointIndex].position;
        }


    }




}
