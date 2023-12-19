using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SM_TestState : StateMachine
{
    

    public SM_TestState(DriverData driver) : base(driver)
    {
        sm_name = "Test State";
    }

    protected override void Enter()
    {
        base.Enter();
    }

    protected override void Update()
    {
        if (!sm_driver.testState)
        {
            TriggerExit(new SM_NormalState(sm_driver));
        }

        sm_driver.engine.Move(0.0f, 1.0f, 0.0f);
        sm_driver.brakeLight.SetActive(true);

        base.Update();
    }

    protected override void Exit()
    {
        base.Exit();
    }
}
