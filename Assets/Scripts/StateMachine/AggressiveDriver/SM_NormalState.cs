using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SM_NormalState : StateMachine
{
    //private Vector3 waypointTarget;

    private float steeringSensitivity = 0.01f;
    private float visionLength = 9.0f;
    private float visionAngle = 25.0f;


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
        speedAllowance.max = 50.0f;
        speedAllowance.min = 10.0f;
        distanceAllowance.max = 80.0f;
        distanceAllowance.min = 0.0f;

        useFuzzySystem = driverSpeedFuzzy.InitFuzzySystem(distanceAllowance, speedAllowance, sm_driver.currentFuzzinessUtilityData);

        sm_driver.steeringSensitivity = steeringSensitivity;
        sm_driver.visionLength = visionLength;
        sm_driver.visionAngle = visionAngle;

        base.Enter();
    }

    protected override void Update()
    {

        //UPDATES THE WAYPOINT AND CURRENT GOAL/TARGET
        UpdateCurrentGoal(UpdateWaypointGoal());


        //sm_driver.currentTargetInfo = sm_driver.circuit.waypoints[sm_driver.currentWaypointIndex];
        if (sm_driver.goBerserk)
        {
            TriggerExit(new SM_AggressiveState(sm_driver));
        }

        if (sm_driver.testState)
        {
            TriggerExit(new SM_TestState(sm_driver));
        }


        //TEST TEST TEST 
        if(CheckBehind(out Rigidbody target, sm_driver.rayLength * 1.5f, sm_driver.behindRayType) && sm_driver.canUseBlock && sm_driver.blockingCooldown < 0)
        {
            //Time.timeScale = 0.3f;

            if(target == null)
                return;

            Vector3 right = sm_driver.transform.TransformDirection(sm_driver.transform.right);
            Vector3 toOther = target.transform.position - sm_driver.transform.position;

            float steerDirToTarget = Vector3.Dot(right, toOther);

            steerDirToTarget = Mathf.Clamp(steerDirToTarget, -1.0f, 1.0f);

            float distanceOnX = Mathf.Abs(target.transform.position.x - sm_driver.transform.position.x);
            float threshold = 0.5f;     //distance to 1.0 units
            if(distanceOnX > threshold)
            {
                sm_driver.engine.Move(0.85f, 0.1f, steerDirToTarget);
                Debug.Log("Steer direction: " +  steerDirToTarget); 
            }


            BlockingInfo info = new BlockingInfo();
            info.target = target;


            //TEST TST TEST
            TriggerExit(new SM_BlockingState(sm_driver, info));
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

        ObstacleAviodance(visionLength, visionAngle, steeringSensitivity, ref steer);

        float distanceToTarget = Vector3.Distance(sm_driver.currentTarget, sm_driver.rb.transform.position);
        //Brakes(ref brake, distanceToTarget, ref accelerate);


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
                TriggerExit(new SM_NormalState(sm_driver));
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


    /// <summary>
    /// A true/false value to potentially 
    /// overtake. 
    /// Later change to using the degree of true
    /// </summary>
    /// <returns></returns>
    private bool PotentialToOvertake()
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

                //TO-DO: REFACTORIZE WITH OTHER DOT PRODUCTS
                //direction to overtake from 
                int overtakingdirection = 0;

                Vector3 waypointAhead = sm_driver.currentTarget;
                Transform targetOpponent = hit.transform;

                Vector3 right = targetOpponent.TransformDirection(targetOpponent.right);
                Vector3 toOther = waypointAhead - targetOpponent.position;

                float dot = Vector3.Dot(right, toOther);

                overtakingdirection = (dot < 0.0f) ? 1 : -1;

                //TESTING AND NEED CHANGING
                sm_driver.engine.Move(0.5f, 0.2f, overtakingdirection);

                //the offsetting value from the opponent 
                //we overdircetion for which side to consider for overtaking 
                float sideOvertakingOffset = 2.0f * overtakingdirection;    //2 unit offset 
                //Vector3 sidePointToOpponent = info.opponentTransform.position - 
                //                                info.opponentTransform.right * sideOvertakingOffset;

                Vector3 sidePointToOpponent = hit.transform.position +
                                hit.transform.right * sideOvertakingOffset;

                CustomMath.DebugObject(PrimitiveType.Capsule, sidePointToOpponent, sm_driver.mat);
                GameObject sidePointpotentialObj = new GameObject();
                Ghost sidePointpotential = new Ghost(sidePointpotentialObj);
                sidePointpotential.obj.transform.position = sidePointToOpponent;
                info.potentialGoal = sidePointpotential;

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
                Ghost ghostInfo = new Ghost(ghost);
                info.overtakeGhost = ghostInfo;

                //TO-DO: STEER AWAY FROM OPPONENT
                //TESTING
                sidePointToOpponent = sidePointToOpponent - (hit.transform.forward * 3.0f); //draw backwards by one units
                GameObject sidePointObj = new GameObject();
                Ghost sidePoint = new Ghost(sidePointObj);
                sidePoint.obj.transform.position = sidePointToOpponent;
                info.initialGoal = sidePoint;


                //REGISTER CURRENT WAYPOINT IN OVERTAKINGDATA
                info.initialWaypoint = sm_driver.currentWaypoint.position;

                CustomMath.DebugObject(PrimitiveType.Cube, sidePointToOpponent, sm_driver.mat);

               
                return true;    
            }
        }


        return false;
    }



    
}
