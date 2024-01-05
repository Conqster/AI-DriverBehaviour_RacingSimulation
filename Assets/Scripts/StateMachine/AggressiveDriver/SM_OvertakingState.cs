using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Infomation about overtaking 
/// need to more to a different script
/// </summary>
public struct OvertakingInfo
{
    public Transform opponentTransform;
    public Rigidbody opponentRb;

    public Vector3 milestone1;
    public Vector3 milestone2;
    public Vector3 milestone3;
    public Vector3 milestone4;

    public Ghost overtakeGhost;
    public Ghost potentialGoal;
    public Ghost initialGoal;

    public Vector3 initialWaypoint; //keeps track of waypoint
}



public class SM_OvertakingState : StateMachine
{

    private OvertakingInfo overtakingData;
    private float aggersiveMeter = 0.0f;


    private float steeringSensitivity = 0.01f;
    private float normalSteerIntensity = 0.01f;

    private float visionLength = 9.0f;
    private float visionAngle = 25.0f;

    private Vector3 currentGoalTarget;

    //Make is state machine global later to decision which car should be cautions 
    private bool beCaution = true;

    public SM_OvertakingState(DriverData driver, OvertakingInfo overtakingInfo) : base(driver)
    {
        sm_name = "Overtaking State";
        this.overtakingData = overtakingInfo;
    }



    protected override void Enter()
    {
        //check data in OvertakingInfo 
        //if not valid
        //TriggerExit(new SM_NormalState(sm_driver));

        //and check, if transiton is smooth
        //in right position
        //if not readjust to position 
        //for a smooth overtake

        speedAllowance = fuzzyUtilityCollection.OS_SpeedAllowance;
        distanceAllowance = fuzzyUtilityCollection.OS_DistanceAllowance;

        sm_driver.currentFuzzinessUtilityData = fuzzyUtilityCollection.OS_FuzzyUtilityData;

        sm_driver.steeringSensitivity = steeringSensitivity;
        sm_driver.visionLength = visionLength;
        sm_driver.visionAngle = visionAngle;

        if(overtakingData.initialGoal == null)
        {
            Debug.Log("The initial goal is null");
        }

        canDrive = true;
        base.Enter();
    }

    protected override void Update()
    {
        accelerate = 1.0f;
        brake = 0.0f;

        if (beCaution)
        {
            BeCaution(ref brake, ref accelerate);
        }


        if (OvertakingAchieved())
        {
            TriggerExit(new SM_NormalState(sm_driver));
        }


        ObstacleAviodance(visionLength, visionAngle, steeringSensitivity, ref steer);


        if (UpdateGoal(ref currentGoalTarget))
        {
            //Movement();
            //COULD DO THIS
            //if(!UpdateGoal(ref sm_driver.currentTarget))
            //{
            //    TriggerExit(new SM_NormalState(sm_driver));
            //}
            UpdateCurrentGoal(currentGoalTarget);
        }
        else
        {
            //if passed goal but still next to opponent ignore
            //TO-DO: COULD HAVE THAT BASED ON CURRENT GOAL 
            //       ACCELERATION INTENSITY INCREASES 
            TriggerExit(new SM_NormalState(sm_driver));
        }



        if(ChangesInConditions(ref aggersiveMeter))
        {
            if(aggersiveMeter > 0.4f)
            {
                TriggerExit(new SM_AggressiveState(sm_driver));
                //return;  IF A BUG OCCURS
            }
            else
            {
                TriggerExit(new SM_NormalState(sm_driver));
            }
        }

        // not really required any more 
        overtakingData.initialWaypoint = UpdateWaypointGoal();
        


        #region OldWays
        //NEED TO CHANGE JUST PLACED HERE, FOR UPDATE
        //float distanceToWaypoint = Vector3.Distance(overtakingData.initialWaypoint, sm_driver.rb.transform.position);
        //if (distanceToWaypoint < 15.0f)
        //{
        //    sm_driver.currentWaypointIndex++;
        //    if (sm_driver.currentWaypointIndex >= sm_driver.circuit.waypoints.Count)
        //        sm_driver.currentWaypointIndex = 0;

        //    overtakingData.initialWaypoint = sm_driver.circuit.waypoints[sm_driver.currentWaypointIndex].position;
        //    //sm_driver.currentWaypointIndex = sm_driver.currentWaypointIndex; //debug info
        //}
        #endregion
        base.Update();
    }

    protected override void Exit()
    {

    //        public Ghost overtakeGhost;
    //public Ghost potentialGoal;
    //public Ghost initialGoal;
        
        //if(overtakingData.overtakeGhost != null)
        //{
        //    overtakingData.overtakeGhost.Destroy();
        //}
        //if(overtakingData.potentialGoal != null)
        //{
        //    overtakingData.potentialGoal.Destroy();
        //}
        //if(overtakingData.initialGoal != null)
        //{
        //    overtakingData.initialGoal.Destroy();
        //}

        base.Exit();
    }




    private bool UpdateGoalOld(ref Vector3 goal)
    {
        Vector3 forward = sm_driver.transform.TransformDirection(sm_driver.transform.forward);

        if (overtakingData.initialGoal.obj)
        {
            Vector3 toOther = overtakingData.initialGoal.obj.transform.position - sm_driver.transform.position;
            if (Vector3.Dot(forward, toOther) > 0)
            {
                goal = overtakingData.initialGoal.obj.transform.position;

                //FOR DEBUGGING PURPOSES
                //sm_driver.currentTargetInfo = overtakingData.initialGoal.obj.transform;
                return true;
            }
        }

        if (overtakingData.potentialGoal.obj)
        {
            Vector3 toOther = overtakingData.potentialGoal.obj.transform.position - sm_driver.transform.position;
            if (Vector3.Dot(forward, toOther) > 0)
            {
                goal = overtakingData.potentialGoal.obj.transform.position;

                //FOR DEBUGGING PURPOSES
                //sm_driver.currentTargetInfo = overtakingData.potentialGoal.obj.transform;
                return true;
            }
        }

        if (overtakingData.overtakeGhost.obj)
        {
            Vector3 toOther = overtakingData.overtakeGhost.obj.transform.position - sm_driver.transform.position;
            if (Vector3.Dot(forward, toOther) > 0)
            {
                goal = overtakingData.overtakeGhost.obj.transform.position;


                //FOR DEBUGGING PURPOSES
                //sm_driver.currentTargetInfo = overtakingData.overtakeGhost.obj.transform;
                return true;
            }
        }

 

        return false;
    }


    private bool UpdateGoal(ref Vector3 goal)
    {
        Vector3 forward = sm_driver.transform.TransformDirection(sm_driver.transform.forward);


        Vector3 toOther = overtakingData.milestone1 - sm_driver.transform.position;
        if (Vector3.Dot(forward, toOther) > 0)
        {
            goal = overtakingData.milestone1;

            //FOR DEBUGGING PURPOSES
            //sm_driver.currentTargetInfo = overtakingData.initialGoal.obj.transform;
            return true;
        }
        

        toOther = overtakingData.milestone2 - sm_driver.transform.position;
        if (Vector3.Dot(forward, toOther) > 0)
        {
            goal = overtakingData.milestone2;

            //FOR DEBUGGING PURPOSES
            //sm_driver.currentTargetInfo = overtakingData.potentialGoal.obj.transform;
            return true;
        }
        

        toOther = overtakingData.milestone3 - sm_driver.transform.position;
        if (Vector3.Dot(forward, toOther) > 0)
        {
            goal = overtakingData.milestone3;


            //FOR DEBUGGING PURPOSES
            //sm_driver.currentTargetInfo = overtakingData.overtakeGhost.obj.transform;
            return true;
        }
        



        return false;
    }

    /// <summary>
    ///check if overtaking is successful 
    /// </summary>
    /// <returns></returns>
    private bool OvertakingAchieved()
    {
        ///check if overtaking is successful 
        

        return false;
    }

    /// <summary>
    /// modifies calmness/agressiveMeter 
    /// based on the state of the environment
    /// when true might when to change state 
    /// </summary>
    /// <param name="calmness"></param>
    private bool ChangesInConditions(ref float agressiveness, float rate = 0.1f)
    {
        //if everything is going normal
        agressiveness -= rate;

        //if everything is not going well based on some cnditions 
        agressiveness += rate;

        //to stay between 0 and 1
        agressiveness = Mathf.Clamp01(agressiveness);

        //getting too agressive 
        //still going to add other conditions (if overtaking can not be achieved)
        if(agressiveness > 1)  //might become Aggressive overtake state 
            return true;

        return false;
    }






    private void BeCaution(ref float brake, ref float acc)
    {

        RaycastHit hit;
        Vector3 origin = sm_driver.transform.position +
                 new Vector3(0.0f, sm_driver.raycastUpOffset, 0.0f);
        float tooClose = sm_driver.rayLength * (3.0f / 4.0f);
        bool opponentValid = false;

        if (Physics.Raycast(origin, sm_driver.transform.forward, out hit, sm_driver.rayLength * 2.0f))
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



                //TEST FOR NOW FOR SWITCH BACK
                if(hit.distance < (tooClose * 0.5f))
                {
                    acc = 0.0f;
                    TriggerExit(new SM_NormalState(sm_driver));
                }
            }
        }
        Color toUse = (opponentValid) ? Color.red : Color.blue;
        Debug.DrawRay(origin, sm_driver.transform.forward * sm_driver.rayLength * 2f, toUse);

        //TO-DO: check distance to target, there is a corner to it use brakes, if not do not brakes 
    }



}
