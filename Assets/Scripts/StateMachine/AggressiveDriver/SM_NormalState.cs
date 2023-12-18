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

    private float lastObtacleTime = Mathf.Infinity;
    private float normalSteerIntensity = 0.01f;

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

        sm_driver.targetAngle = targetAngle;

        //constant acceleration
        float accelerate = 1f;
        float brake = 0.0f;

        

        //obstacleAhead = obstacleAvoidance.DangerAhead(visionLength, visionAngle);
        //Should ignore steering towards target if obstacle ahead
        if (sm_driver.obstacleAvoidance.DangerAhead(visionLength, visionAngle))
        {
            lastObtacleTime = 0.0f;
        }
        else
        {
            //steer based on the difference to target angle and use sensitivity.
            //steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(sm_driver.rb.velocity.magnitude);    //problem sets the angle directly, so it creates a snapping pos rotation

            AvoidedObstacle(ref normalSteerIntensity, steeringSensitivity);
            //SteerToTarget(targetAngle, normalSteerIntensity * 10f, ref steer);
            steer = Mathf.Clamp(targetAngle * normalSteerIntensity, -1, 1) * Mathf.Sign(sm_driver.rb.velocity.magnitude);    //problem sets the angle directly, so it creates a snapping pos rotation
            //float steer = 0.0f;
        }

        sm_driver.normalSteerIntensity = normalSteerIntensity;

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

        sm_driver.currentSteer = steer;

        if (distanceToTarget < 15.0f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= sm_driver.circuit.waypoints.Count)
                currentWaypointIndex = 0;

            waypointTarget = sm_driver.circuit.waypoints[currentWaypointIndex].position;
            sm_driver.currentWaypointIndex = currentWaypointIndex; //debug info
        }
        base.Update();
    }

    protected override void Exit()
    {
        //TEST TEST
        sm_driver.goBerserk = false; 
        base.Exit();
    }


    private void SteerToTarget(float targetAngle, float rotIntensity, ref float steerDirRatio)
    {
        //get the angle to target 
        //get rotation direction to target
        //the further the intensity 
        //the closer, less intensity
        float maxRotationAt = 60.0f;// make global to current state later 
        float minRotationAt = 30.0f;

        //lerp rotation intensity based on max and min RotationAt
        float rotationRatio = Mathf.InverseLerp(minRotationAt, maxRotationAt, Mathf.Abs(targetAngle));
        //float currentIntensity = rotIntensity * rotationRatio;
        float currentIntensity = Mathf.Lerp(rotIntensity * 0.1f, rotIntensity, rotationRatio);

        //steer direction
        float steerDir = Mathf.Sign(sm_driver.rb.velocity.magnitude);

        //increase steer based on intensity and direction
        steerDirRatio += (currentIntensity * steerDir);
        sm_driver.currentDirection = steerDir;
        sm_driver.currentIntensity = currentIntensity;
        sm_driver.rotRatio = rotationRatio;

        //keep steer in the range -1 and 1
        steerDirRatio = Mathf.Clamp(steerDirRatio, -1.0f, 1.0f);
    }

    private void AvoidedObstacle(ref float normalSteerIntensity, float steerIntensity)
    {
        lastObtacleTime += Time.deltaTime;        // increases overtime last obstacle avioded 
        float minIntensity = steeringSensitivity * 0.2f;

        normalSteerIntensity = Mathf.Lerp(minIntensity, steerIntensity, lastObtacleTime);
    }

}
