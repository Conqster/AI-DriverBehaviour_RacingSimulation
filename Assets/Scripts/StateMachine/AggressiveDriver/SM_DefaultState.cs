using Car;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SM_DefaultState : StateMachine
{
    private bool startRace = false;
    private float startRaceIn = 3.0f;

    public SM_DefaultState(DriverData driver) : base(driver)
    {
        sm_name = "Default State";
        sm_duration = 0.0f;
        sm_event = SM_Event.Enter;
    }


    protected override void Enter()
    {


        base.Enter();
    }

    protected override void Update()
    {
        startRace = sm_driver.start;

        if(sm_duration > startRaceIn) 
            startRace = true;

        if(startRace)
        {
            TriggerExit(new SM_NormalState(sm_driver));
        }

        base.Update();
    }

    protected override void Exit()
    {

        base.Exit();
    }
}
