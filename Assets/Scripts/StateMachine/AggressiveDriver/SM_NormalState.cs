using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class SM_NormalState : StateMachine
{
    //private Vector3 waypointTarget;


    private float lastObtacleTime = Mathf.Infinity;
    private float normalSteerIntensity = 0.01f;

    private bool wantToOverTake = false;
    //because this would delay overtaking if possible at start f

    private Rigidbody overtakeOpponent;

    public SM_NormalState(DriverData driver) : base(driver)
    {
        sm_name = "Normal State";
        sm_event = SM_Event.Enter;
        sm_duration = 0.0f;
    }

    protected override void Enter()
    {

        speedAllowance = fuzzyUtilityCollection.NS_SpeedAllowance;
        distanceAllowance = fuzzyUtilityCollection.NS_DistanceAllowance;

        sm_driver.currentFuzzinessUtilityData = fuzzyUtilityCollection.NS_FuzzyUtilityData;
        

        sm_driver.steeringSensitivity = sm_driver.stateInfo.NS_Data.steeringSensitivity;
        sm_driver.visionLength = sm_driver.stateInfo.NS_Data.visionLength;
        sm_driver.visionAngle = sm_driver.stateInfo.NS_Data.visionAngle;

        sm_driver.overtakeCooldown = sm_driver.stateInfo.NS_Data.coolDown1;

        canDrive = true;
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


        //TEST TEST TEST 
        if(CheckBehind(out Rigidbody target, sm_driver.rayLength * 1.5f, sm_driver.behindRayType) && sm_driver.canUseBlock && sm_driver.blockingCooldown < 0)
        {
            //Time.timeScale = 0.3f;

            //if (target == null)
            //    return; //bad because its get outcompletly from the function


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
        sm_driver.overtakeCooldown -= Time.deltaTime;

        if(sm_driver.overtakeCooldown < 0)
        {
            sm_driver.overtakeCooldown = Mathf.Clamp(sm_driver.overtakeCooldown, 0, Mathf.Infinity);
            if (PotentialToOvertake(ref overtakeOpponent))
            {
                TriggerExit(new SM_OvertakingState(sm_driver, new OvertakingInfo()));
                //Debug.Log("Trying to overtake");
                wantToOverTake = true;
                //sm_event = SM_Event.Exit;
            }
        }

        ObstacleAviodance(sm_driver.visionLength, sm_driver.visionAngle, sm_driver.steeringSensitivity, ref steer);

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
            if(TryOvertaking(out OvertakingInfo info, overtakeOpponent))
            {
                TriggerExit(new SM_OvertakingState(sm_driver, info));
                //Debug.Log("NOW NOW DAMN IT!!!!!");
            }
            else
            {
                //TriggerExit(new SM_AggressiveState(sm_driver));
                TriggerExit(new SM_NormalState(sm_driver));
                //tryOvertakeCoolDown = 5.0f;

            }
            sm_driver.overtakeCooldown = sm_driver.stateInfo.NS_Data.coolDown1;
            wantToOverTake = false;
        }


        base.Exit();
    }






    /// <summary>
    /// A true/false value to potentially 
    /// overtake. 
    /// Later change to using the degree of true
    /// </summary>
    /// <param name="overtakeTarget"></param>
    /// <returns></returns>
    private bool PotentialToOvertake(ref Rigidbody overtakeTarget)
    {
        //TO-DO: get the info of the car wanting to overtake
        //more dominant side for overtaking 
        //width to target for overtaking
        //distance to next corner 

        RaycastHit hit;
        //Rigidbody target;

        overtakeTarget = GetOvertakingOpponent();

        if(overtakeTarget != null)
        {
            //only overtake if my speed beats opponent
            if (sm_driver.rb.velocity.magnitude < overtakeOpponent.velocity.magnitude)
                return false;

            if (overtakeOpponent == sm_driver.rb)
                return false;

            //dominant side 
            Vector3 opponentCenterMass = overtakeTarget.transform.position + (Vector3.up * 0.5f);

            RaycastHit leftSideHit, rightSideHit;

            bool dominantLeft = obstacleAvoidance.ObstacleDetection(opponentCenterMass, -overtakeTarget.transform.right, out leftSideHit, Mathf.Infinity, "Wall");
            bool dominantRight = obstacleAvoidance.ObstacleDetection(opponentCenterMass, overtakeTarget.transform.right, out rightSideHit, Mathf.Infinity, "Wall");

            int dominantSide = 0;

            dominantSide = (dominantLeft) ? -1 : 0;
            dominantSide = (dominantRight) ? 1 : 0;

            if (dominantLeft && dominantRight)
            {
                if (leftSideHit.distance > rightSideHit.distance)
                    dominantSide = -1;
                else
                    dominantSide = 1;
            }



            #region prepare Overtake
            //distance to next corner 
            float distance = 0;
            Vector3 origin = sm_driver.transform.position + (Vector3.up * 2.0f);

            //sm_driver.obstacleAvoidance.ObstacleDetection(origin, sm_driver.transform.forward, out hit, Mathf.Infinity, "Wall");

            //distance = hit.distance;

            UpdateDistanceToCorner(ref distance, origin, sm_driver.transform.forward, true);
            //Debug.DrawLine(origin, sm_driver.transform.forward * distance, Color.magenta, 10.0f);


            //length of about four cars
            float minimumCornerDistanceOvertake = 19.0f;
            //float minimumCornerDistanceOvertake = 100.0f;

            if (distance > minimumCornerDistanceOvertake)
            {
                Debug.Log("Distance To Corner: " + distance);


                //distance to target --  2 length of a car
                float minimumDistance = 8.0f;
                float distanceToOpponent = Vector3.Distance(sm_driver.rb.position, overtakeOpponent.position);
                Debug.Log("Distance To Opponent: " + distanceToOpponent);

                if (distanceToOpponent > minimumDistance)
                    return true;

                sm_driver.overtakeCooldown = 2f;
            }
            #endregion
        }



        return false;
    }


    private Rigidbody GetOvertakingOpponent()
    {
        RaycastHit opponentHit;

        Vector3 origin = sm_driver.transform.position +
           new Vector3(0.0f, sm_driver.raycastUpOffset, 0.0f);

        Vector3 leftWhiskey = origin - (sm_driver.transform.right);
        Vector3 rightWhiskey = origin + (sm_driver.transform.right);

        if(obstacleAvoidance.ObstacleDetection(origin, sm_driver.transform.forward, out opponentHit, sm_driver.rayLength, "Vehicle"))
        return opponentHit.rigidbody;

        if(obstacleAvoidance.ObstacleDetection(leftWhiskey, sm_driver.transform.forward, out opponentHit, sm_driver.rayLength, "Vehicle"))
        return opponentHit.rigidbody;

        if(obstacleAvoidance.ObstacleDetection(rightWhiskey, sm_driver.transform.forward, out opponentHit, sm_driver.rayLength, "Vehicle"))
        return opponentHit.rigidbody;

        return null;
    }




    /// <summary>
    /// Try to get set/ready for overtake
    /// returns true/false if achievable
    /// </summary>
    /// <param name="info"></param>
    /// <param name="opponent"></param>
    /// <returns></returns>
    private bool TryOvertaking(out OvertakingInfo info, Rigidbody opponent)
    {

        //TO-DO: REGISTER A GHOST TARGET / VECTOR DIRECTION TO MAINTAIN
        //       OPPONENT TRANSFORM
        //       POTENTIAL GHOST POSITION 
        //       IF POSITION IS ACHIEVED, AND OVEERTAKE IS NOT ACHIEVED 
        //       PROSPONE OVERTAKE | OR RESORT IN AGGRESIVE BEHAVIOUR
        //       IF ALL ABOVE DATA HAS BEEN STORED RETURN, TURN
        //       TRY POSITION DRIVER FOR OVERTAKE

        info = new OvertakingInfo();

        if (opponent != null)
        {
            //dominant side 
            Vector3 opponentCenterMass = opponent.transform.position + (Vector3.up * 0.5f);

            RaycastHit leftSideHit, rightSideHit;

            bool dominantLeft = obstacleAvoidance.ObstacleDetection(opponentCenterMass, -opponent.transform.right, out leftSideHit, Mathf.Infinity, "Wall");
            bool dominantRight = obstacleAvoidance.ObstacleDetection(opponentCenterMass, opponent.transform.right, out rightSideHit, Mathf.Infinity, "Wall");

            int dominantSide = 0;

            dominantSide = (dominantLeft) ? -1 : 0;
            dominantSide = (dominantRight) ? 1 : 0;

            if (dominantLeft && dominantRight)
            {
                if (leftSideHit.distance > rightSideHit.distance)
                    dominantSide = -1;
                else
                    dominantSide = 1;
            }

            #region old side check
            //check my side to the opponent 
            //Vector3 right = sm_driver.transform.TransformDirection(opponent.transform.right);
            //Vector3 toOther = opponent.position - sm_driver.transform.position;

            //Vector3 localTarget = sm_driver.transform.InverseTransformPoint(opponent.transform.position);
            //float angle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

            //int angleDirOpponentSide = (angle >  0) ? 1 : -1;

            //if(angleDirOpponentSide == dominantSide)
            //{
            //    //meaning driver have to turn towards opponent to overtake
            //    //which could be bad 

            //    if(angle > 5.0f)  //should be able to make it under 20 degrees
            //        return false;

            //    //for now return later, perform a calcul;ation to repath a overatking
            //}
            #endregion


            //heavy steer in the overtake direction
            sm_driver.engine.Move(accelerate, 0.0f, dominantSide);

            //then i can overtake
            Vector3 overtakeMilestone1 = Vector3.zero;
            float widthOf2Vehicle = 4.0f;
            float lengthOfVechicle = 4.0f;

            overtakeMilestone1 = opponent.position + (opponent.transform.up * 0.5f)+
                                (opponent.transform.right * dominantSide * widthOf2Vehicle) + 
                                (opponent.transform.forward * ((3.0f/4.0f) * lengthOfVechicle));

            #region New Side Check
            Vector3 vecToOpponent = (opponent.position - sm_driver.transform.position).normalized;
            Vector3 vecToOvertake = (overtakeMilestone1 - sm_driver.transform.position).normalized;

            float dot = Vector3.Dot(vecToOvertake, vecToOpponent);
            float angleBtw = (Mathf.Acos(dot / (vecToOpponent.magnitude * vecToOvertake.magnitude))) * Mathf.Rad2Deg;

            Debug.Log("Angle for overtaking: " + angleBtw);
            if (angleBtw < 10f)
                return false;

            #endregion

 

            //update info
            info.opponentRb = opponent;
            //REGISTER CURRENT WAYPOINT IN OVERTAKINGDATA
            info.initialWaypoint = sm_driver.currentWaypoint.position;

            info.milestone1 = overtakeMilestone1;
            float intervalsBtwMilestones = 10.0f;
            //info.milestone2 = overtakeMilestone1 + (opponent.transform.transform.forward * intervalsBtwMilestones);
            //info.milestone3 = overtakeMilestone1 + ((opponent.transform.transform.forward * intervalsBtwMilestones) * 2.0f);

            // a math function generate points based on current path, points for overtaking path
            Vector3[] myPoints = CustomMath.GeneratePoints(overtakeMilestone1, sm_driver.currentWaypointIndex, sm_driver.circuit.waypoints, intervalsBtwMilestones,4);


            info.milestone2 = myPoints[1];
            info.milestone3 = myPoints[2];
            info.milestone4 = myPoints[3];

            Debug.DrawLine(sm_driver.transform.position, info.milestone1, Color.magenta, 10.0f);
            Debug.DrawLine(info.milestone1, info.milestone2, Color.cyan, 10.0f);
            Debug.DrawLine(info.milestone2, info.milestone3, Color.yellow, 10.0f);
            Debug.DrawLine(info.milestone3, info.milestone4, Color.cyan, 10.0f);
            return true;
        }
        else
            return false;

    }



}
