

using UnityEngine;

public class SM_DefaultState : StateMachine
{
    private bool startRace = false;
    private float startRaceIn = 3.0f;

    private float startEngine = 0.0f;

    public SM_DefaultState(DriverData driver) : base(driver)
    {
        sm_name = "Default State";
        sm_duration = 0.0f;
        sm_event = SM_Event.Enter;
    }


    protected override void Enter()
    {

        canDrive = false;
        base.Enter();
    }

    protected override void Update()
    {
        startRace = sm_driver.canStartRace;

        //if(sm_duration > startRaceIn) 
        //    startRace = true;

        if (startRace)
            startEngine += Time.deltaTime;


        if(startEngine > startRaceIn)
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
