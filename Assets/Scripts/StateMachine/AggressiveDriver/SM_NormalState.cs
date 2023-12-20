using System;
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

    private bool wantToOverTake = false;
    private float tryOvertakeCoolDown = 5.0f; // probably going to bring out to sm_driver 
    //because this would delay overtaking if possible at start f

    public SM_NormalState(DriverData driver) : base(driver)
    {
        sm_name = "Normal State";
        sm_event = SM_Event.Enter;
        sm_duration = 0.0f;
    }

    protected override void Enter()
    {
        waypointTarget = sm_driver.circuit.waypoints[sm_driver.currentWaypointIndex].position;

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

        if (sm_driver.testState)
        {
            TriggerExit(new SM_TestState(sm_driver));
        }


        ///Overtaking condition
        ///if there is a car infront 
        ///there is a potential to overtake 
        ///If there is opening for overtaking factors 
        tryOvertakeCoolDown -= Time.deltaTime;
        if (PotentialToOvertake())
        {
            TriggerExit(new SM_OvertakingState(sm_driver, new OvertakingInfo()));
            //Debug.Log("Trying to overtake");
            wantToOverTake = true;
            //sm_event = SM_Event.Exit;
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


        Brakes(ref brake, distanceToTarget, ref accelerate);
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
            sm_driver.currentWaypointIndex++;
            if (sm_driver.currentWaypointIndex >= sm_driver.circuit.waypoints.Count)
                sm_driver.currentWaypointIndex = 0;

            waypointTarget = sm_driver.circuit.waypoints[sm_driver.currentWaypointIndex].position;
            //sm_driver.currentWaypointIndex = sm_driver.currentWaypointIndex; //debug info
        }
        base.Update();
    }

    protected override void Exit()
    {
        //TEST TEST
        sm_driver.goBerserk = false; 

        if(wantToOverTake)
        {
            if(TryOvertaking(out OvertakingInfo info))
            {
                TriggerExit(new SM_OvertakingState(sm_driver, info));
                //Debug.Log("NOW NOW DAMN IT!!!!!");
            }
            else
            {
                //TriggerExit(new SM_AggressiveState(sm_driver));
                //TriggerExit(new SM_NormalState(sm_driver));
                tryOvertakeCoolDown = 5.0f;
            }
            wantToOverTake = false;
        }


        base.Exit();
    }


    private void Brakes(ref float brake, float distanceToTarget, ref float acc)
    {
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


        RaycastHit hit;
        Vector3 origin = sm_driver.transform.position +
                 new Vector3(0.0f, sm_driver.raycastUpOffset, 0.0f);
        float tooClose = sm_driver.rayLength * (3.0f / 4.0f);
        bool opponentValid = false;    

        if (Physics.Raycast(origin, sm_driver.transform.forward, out hit, sm_driver.rayLength))
        {
            if (hit.distance < tooClose && hit.transform.CompareTag("Vehicle"))
            {
                float minAcc = 0.5f;
                float maxAcc = 0.9f;
                //determine how close is it to half tooclose
                float closeness = Mathf.InverseLerp(tooClose * 0.5f, tooClose, hit.distance);
                acc = Mathf.Lerp(minAcc, maxAcc, closeness);
                //Debug.Log("Acc: " + acc);
                brake = 1f;
                opponentValid = true;
            }
        }
        Color toUse = (opponentValid) ? Color.red : Color.blue;
        Debug.DrawRay(origin, sm_driver.transform.forward * sm_driver.rayLength, toUse);

        //TO-DO: check distance to target, there is a corner to it use brakes, if not do not brakes 
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


    //ease normal steering back to fully strength after just avoided an obstacle
    private void AvoidedObstacle(ref float normalSteerIntensity, float steerIntensity)
    {
        lastObtacleTime += Time.deltaTime;        // increases overtime last obstacle avioded 
        float minIntensity = steeringSensitivity * 0.2f;

        normalSteerIntensity = Mathf.Lerp(minIntensity, steerIntensity, lastObtacleTime);
    }


    private bool PotentialToOvertake(float directionTo = 0.0f)
    {
        ///current speed against opponent speed 
        ///distance to opponent
        ///
        bool mayOvertake = false;
        bool overtakeRight = false;
        bool overtakeLeft = false;

        RaycastHit hit;
        Vector3 origin = sm_driver.transform.position + 
                         new Vector3(0.0f, sm_driver.raycastUpOffset, 0.0f);

        Vector3 leftWhiskey = origin - (sm_driver.transform.right);
        Vector3 rightWhiskey = origin + (sm_driver.transform.right);

        float tooClose = sm_driver.rayLength * (3.0f/4.0f);
        if(Physics.Raycast(origin, sm_driver.transform.forward, out hit, sm_driver.rayLength))
        {
            if(hit.distance > tooClose && hit.transform.CompareTag("Vehicle"))
            {
                mayOvertake  = true; 
            }
        }
        if (Physics.Raycast(rightWhiskey, sm_driver.transform.forward, out hit, sm_driver.rayLength))
        {
            if (hit.distance > tooClose && hit.transform.CompareTag("Vehicle"))
            {
                overtakeRight = true;
                //Debug.Log("Right hit" + hit.distance);
            }
        }
        if (Physics.Raycast(leftWhiskey, sm_driver.transform.forward, out hit, sm_driver.rayLength))
        {
            if (hit.distance > tooClose && hit.transform.CompareTag("Vehicle"))
            {
                overtakeLeft = true;
                //Debug.Log("Left hit" + hit.distance);
            }
        }

        Color hitColour = (mayOvertake) ? Color.blue : Color.red;
        Color rightColour = (overtakeRight) ? Color.blue : Color.red;
        Color leftColour = (overtakeLeft) ? Color.blue : Color.red;

        Debug.DrawRay(origin, sm_driver.transform.forward * sm_driver.rayLength, hitColour);
        Debug.DrawRay(rightWhiskey, sm_driver.transform.forward * sm_driver.rayLength, rightColour);
        Debug.DrawRay(leftWhiskey, sm_driver.transform.forward * sm_driver.rayLength, leftColour);



        return mayOvertake || overtakeLeft || overtakeRight;
    }


    /// <summary>
    /// Try to get set/ready for overtake
    /// returns true/false if achievable
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private bool TryOvertaking(out OvertakingInfo info)
    {
        info = new OvertakingInfo();

        //raycast etc get opponent Transform
        RaycastHit hit;


        //TO-DO: REGISTER A GHOST TARGET / VECTOR DIRECTION TO MAINTAIN
        //       OPPONENT TRANSFORM
        //       POTENTIAL GHOST POSITION 
        //       IF POSITION IS ACHIEVED, AND OVEERTAKE IS NOT ACHIEVED 
        //       PROSPONE OVERTAKE | OR RESORT IN AGGRESIVE BEHAVIOUR
        //       IF ALL ABOVE DATA HAS BEEN STORED RETURN, TURN
        //       TRY POSITION DRIVER FOR OVERTAKE

        //ray cast origin point
        Vector3 origin = sm_driver.transform.position +
                  new Vector3(0.0f, sm_driver.raycastUpOffset, 0.0f);

        if (Physics.Raycast(origin, sm_driver.transform.forward, out hit))
        {
            if(hit.transform.CompareTag("Vehicle"))
            {
                info.opponentTransform = hit.transform;
                info.opponentRb = hit.rigidbody;
            }

            if(info.opponentTransform != null)
            {
                float sideOvertakingOffset = 2.0f;    //2 unit offset 
                //Vector3 sidePointToOpponent = info.opponentTransform.position - 
                //                                info.opponentTransform.right * sideOvertakingOffset;

                Vector3 sidePointToOpponent = hit.transform.position -
                                hit.transform.right * sideOvertakingOffset;

                CustomMath.DebugObject(PrimitiveType.Capsule, sidePointToOpponent, sm_driver.mat);
                GameObject sidePointpotential = new GameObject();
                sidePointpotential.transform.position = sidePointToOpponent;
                info.potentialGoal = sidePointpotential.transform;

                //predict achieve position after overtake
                //let say 10 units 

                GameObject ghost = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ghost.name = "Ghost";
                //ghost.transform.position = sidePointToOpponent + (info.opponentTransform.forward * 10.0f);
                ghost.transform.position = sidePointToOpponent + (hit.transform.forward * 60.0f);   //ahead by 60 

                if (sm_driver.mat != null)
                {
                    ghost.GetComponent<Renderer>().material = sm_driver.mat;
                }

                if (ghost.TryGetComponent<Collider>(out Collider col))
                {
                    UnityEngine.Object.Destroy(col);
                }

                info.overtakeGhost = ghost.transform;

                //TO-DO: STEER AWAY FROM OPPONENT
                //TESTING
                sidePointToOpponent = sidePointToOpponent - (hit.transform.forward * 3.0f); //draw backwards by one units
                GameObject sidePoint = new GameObject();
                sidePoint.transform.position = sidePointToOpponent;
                info.initialGoal = sidePoint.transform;

                Vector3 localTarget = sm_driver.rb.transform.InverseTransformPoint(sidePointToOpponent);
                float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
                steer = Mathf.Clamp(targetAngle * normalSteerIntensity, -1, 1) * Mathf.Sign(sm_driver.rb.velocity.magnitude);    //problem sets the angle directly, so it creates a snapping pos rotation

                CustomMath.DebugObject(PrimitiveType.Cube, sidePointToOpponent, sm_driver.mat);
                Time.timeScale = 0.5f;

                float accelerate = 1f;
                float brake = 0.0f;    
                sm_driver.engine.Move(accelerate, brake, steer);
                return true;    
            }
        }

        //info.opponentTransform = hit.transform;
        //info.numberOfCars = 1;
        //info.initialSpeed = sm_driver.rb.velocity.magnitude;
        //info.opponentInitialPos = info.opponentTransform.position;

        //based on calculation initialSpeed opponentSpeed
        //info.requiredSpeed = 12946.0f;

        //if (hit.transform.tag != "A car" && hit.transform.tag == "Track wall")
        //    info.distanceToNextCorner = hit.distance;

        ////if some condition are meet like 
        //if(info.distanceToNextCorner > 50 && info.initialSpeed < sm_driver.engine.TopSpeed)   //TO-DO: TOP SPEED IS YET TO BE IMPLEMENTED
        //    return true;


        return false;
    }

}
