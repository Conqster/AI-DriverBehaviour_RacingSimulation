using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SM_AggressiveState : StateMachine
{
    private Vector3 waypointTarget;

    private float steeringSensitivity = 0.01f;
    private float visionLength = 3.0f;
    private float visionAngle = 15.0f;

    private float steer;

    private float comeDownIn = 1.0f;

    public SM_AggressiveState(DriverData driver) : base(driver)
    {
        sm_name = "Aggressive State";
        sm_event = SM_Event.Enter;
    }

    protected override void Enter()
    {
        waypointTarget = sm_driver.circuit.waypoints[sm_driver.currentWaypointIndex].position;

        sm_driver.steeringSensitivity = steeringSensitivity;
        sm_driver.visionLength = visionLength;
        sm_driver.visionAngle = visionAngle;

        canDrive = true;
        base.Enter();
    }

    protected override void Update()
    {
        if(sm_duration > comeDownIn)
        {
            TriggerExit(new SM_NormalState(sm_driver));
        }

        Vector3 localTarget = sm_driver.rb.transform.InverseTransformPoint(waypointTarget);
        float distanceToTarget = Vector3.Distance(waypointTarget, sm_driver.rb.transform.position);
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;


        //constant acceleration
        float accelerate = 1f;
        float brake = 0.0f;

        //obstacleAhead = obstacleAvoidance.DangerAhead(visionLength, visionAngle);
        //Should ignore steering towards target if obstacle ahead
        if (!obstacleAvoidance.DangerAhead(visionLength, visionAngle))
        {
            //steer based on the difference to target angle and use sensitivity.
            steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(sm_driver.rb.velocity.magnitude);
            //float steer = 0.0f;
        }

        obstacleAvoidance.Perception(visionLength, visionAngle, steeringSensitivity * 100f, ref steer);
        //print("steer value: " + steer);


        if (distanceToTarget < 15 && sm_driver.rb.velocity.magnitude > 15.0f)
        {
            brake = (15 - distanceToTarget / 15);
            //brake = 0.5f;
        }

        //SPEED DANGER NEED TO BREAK 
        if (sm_driver.rb.velocity.magnitude > 25.0f && distanceToTarget < 45.0f && distanceToTarget > 15.0f)
        {
            brake = 1.0f;
        }

        obstacleAvoidance.Braking(ref brake, visionLength, visionAngle, 0.5f);
        //brake = 1.0f;
        sm_driver.engine.Move(accelerate, brake, steer);



        if (brake > 0.0f)
        {
            //sm_driver.brakeLight.SetActive(true);
            brake -= 0.5f;     //CURRENT TO PREVENT BRAKING TOO HARD CHANGE LATER
        }
        else
        {
            //sm_driver.brakeLight.SetActive(false);
        }


        if (distanceToTarget < 15.0f)
        {
            sm_driver.currentWaypointIndex++;
            if (sm_driver.currentWaypointIndex >= sm_driver.circuit.waypoints.Count)
                sm_driver.currentWaypointIndex = 0;

            waypointTarget = sm_driver.circuit.waypoints[sm_driver.currentWaypointIndex].position;
        }
        base.Update();
    }

    protected override void Exit()
    {

        base.Exit();
    }
}
