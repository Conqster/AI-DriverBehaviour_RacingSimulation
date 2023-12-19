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
    public float distanceToNextCorner;
    public int numberOfCars;
    public float initialSpeed;
    public float requiredSpeed;
    public Vector3 opponentInitialPos;
    public Vector3 predicitedOpponentPos;
}

public class SM_OvertakingState : StateMachine
{

    private OvertakingInfo overtakingData;
    private float aggersiveMeter = 0.0f;

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


        base.Enter();
    }

    protected override void Update()
    {
        if(OvertakingAchieved())
        {
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

}
