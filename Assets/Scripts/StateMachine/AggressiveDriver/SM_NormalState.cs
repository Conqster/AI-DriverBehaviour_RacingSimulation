using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SM_NormalState : StateMachine
{
    private Vector3 waypointTarget;

    private float steeringSensitivity = 0.01f;
    private float visionLength = 9.0f;
    private float visionAngle = 25.0f;

    private float steer;

    public SM_NormalState(DriverData driver) : base(driver)
    {
        sm_name = "Normal State";
        sm_event = SM_Event.Enter;
        sm_duration = 0.0f;
    }

    protected override void Enter()
    {
        waypointTarget = sm_driver.circuit.waypoints[currentWaypointIndex].position;

        sm_driver.steeringSensitivity = steeringSensitivity;
        sm_driver.visionLength = visionLength;
        sm_driver.visionAngle = visionAngle;

        base.Enter();
    }

    protected override void Update()
    {
        if(sm_driver.goBerserk)
        {
            TriggerExit(new SM_AggressiveState(sm_driver));
        }

        Vector3 localTarget = sm_driver.rb.transform.InverseTransformPoint(waypointTarget);
        float distanceToTarget = Vector3.Distance(waypointTarget, sm_driver.rb.transform.position);
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;


        //constant acceleration
        float accelerate = 1f;
        float brake = 0.0f;

        //obstacleAhead = obstacleAvoidance.DangerAhead(visionLength, visionAngle);
        //Should ignore steering towards target if obstacle ahead
        if (!sm_driver.obstacleAvoidance.DangerAhead(visionLength, visionAngle))
        {
            //steer based on the difference to target angle and use sensitivity.
            steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(sm_driver.rb.velocity.magnitude);
            //float steer = 0.0f;
        }

        sm_driver.obstacleAvoidance.Perception(visionLength, visionAngle, steeringSensitivity * 100f, ref steer);
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

        sm_driver.obstacleAvoidance.Braking(ref brake, visionLength, visionAngle, 0.5f);
        //brake = 1.0f;
        sm_driver.engine.Move(accelerate, brake, steer);



        if (brake > 0.0f)
        {
            sm_driver.brakeLight.SetActive(true);
            brake -= 0.5f;     //CURRENT TO PREVENT BRAKING TOO HARD CHANGE LATER
        }
        else
        {
            sm_driver.brakeLight.SetActive(false);
        }


        if (distanceToTarget < 15.0f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= sm_driver.circuit.waypoints.Count)
                currentWaypointIndex = 0;

            waypointTarget = sm_driver.circuit.waypoints[currentWaypointIndex].position;
        }
        base.Update();
    }

    protected override void Exit()
    {
        //TEST TEST
        sm_driver.goBerserk = false; 
        base.Exit();
    }

}
