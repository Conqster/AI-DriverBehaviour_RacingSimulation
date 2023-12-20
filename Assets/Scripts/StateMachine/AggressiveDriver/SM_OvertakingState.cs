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
    public Transform overtakeGhost;
    public Transform potentialGoal;
    public Transform initialGoal;
    //public float distanceToNextCorner;
    //public int numberOfCars;
    //public float initialSpeed;
    //public float requiredSpeed;
    //public Vector3 opponentInitialPos;
    //public Vector3 predicitedOpponentPos;
}



public class SM_OvertakingState : StateMachine
{

    private OvertakingInfo overtakingData;
    private float aggersiveMeter = 0.0f;

    private float steer;

    private float steeringSensitivity = 0.01f;
    private float normalSteerIntensity = 0.01f;

    private float visionLength = 9.0f;
    private float visionAngle = 25.0f;

    private Vector3 currentGoalTarget;

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

        steer = sm_driver.currentSteer;


        sm_driver.steeringSensitivity = steeringSensitivity;
        sm_driver.visionLength = visionLength;
        sm_driver.visionAngle = visionAngle;

        if(overtakingData.initialGoal == null)
        {
            Debug.Log("The initial goal is null");
        }

        base.Enter();
    }

    protected override void Update()
    {
        if(OvertakingAchieved())
        {
            TriggerExit(new SM_NormalState(sm_driver));
        }


        if(UpdateGoal(ref currentGoalTarget))
        {
            Movement();
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




        base.Update();
    }

    protected override void Exit()
    {
        base.Exit();
    }




    private bool UpdateGoal(ref Vector3 goal)
    {
        Vector3 forward = sm_driver.transform.TransformDirection(sm_driver.transform.forward);

        if (overtakingData.initialGoal)
        {
            Vector3 toOther = overtakingData.initialGoal.position - sm_driver.transform.position;
            if (Vector3.Dot(forward, toOther) > 0)
            {
                goal = overtakingData.initialGoal.position;
                return true;
            }
        }

        if (overtakingData.potentialGoal)
        {
            Vector3 toOther = overtakingData.potentialGoal.position - sm_driver.transform.position;
            if (Vector3.Dot(forward, toOther) > 0)
            {
                goal = overtakingData.potentialGoal.position;
                return true;
            }
        }

        if (overtakingData.overtakeGhost)
        {
            Vector3 toOther = overtakingData.overtakeGhost.position - sm_driver.transform.position;
            if (Vector3.Dot(forward, toOther) > 0)
            {
                goal = overtakingData.overtakeGhost.position;
                return true;
            }
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



    private void Movement()
    {

        Vector3 localTarget = sm_driver.rb.transform.InverseTransformPoint(currentGoalTarget);
        float distanceToTarget = Vector3.Distance(currentGoalTarget, sm_driver.rb.transform.position);
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

        sm_driver.targetAngle = targetAngle;

        //constant acceleration
        float accelerate = 1f;
        float brake = 0.0f;

        steer = Mathf.Clamp(targetAngle * normalSteerIntensity, -1, 1) * Mathf.Sign(sm_driver.rb.velocity.magnitude);    //problem sets the angle directly, so it creates a snapping pos rotation
                                                                                                                         //float steer = 0.0f;
        sm_driver.normalSteerIntensity = normalSteerIntensity;

        sm_driver.obstacleAvoidance.Perception(visionLength, visionAngle, steeringSensitivity * 100f, ref steer);
        //print("steer value: " + steer);



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

        //NEED TO CHANGE JUST PLACED HERE, FOR UPDATE
        Vector3 ghostWaypoint = sm_driver.circuit.waypoints[sm_driver.currentWaypointIndex].position;
        float distanceTOWaypoint = Vector3.Distance(ghostWaypoint, sm_driver.rb.transform.position);
        if (distanceToTarget < 15.0f)
        {
            sm_driver.currentWaypointIndex++;
            if (sm_driver.currentWaypointIndex >= sm_driver.circuit.waypoints.Count)
                sm_driver.currentWaypointIndex = 0;

            //sm_driver.currentWaypointIndex = sm_driver.currentWaypointIndex; //debug info
        }
    }

}
