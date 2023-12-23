using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct BlockingInfo
{
    public Rigidbody target;
    public Ghost nextCorner;
}


public class SM_BlockingState : StateMachine
{

    private BlockingInfo blockingInfo;
    private GameObject nextCornerObj;
    private float minDistanceAllowance = 10.0f;     //distnce to corner, if disntce is less go back to normal state

    private float blocksteerIntensity = 0.1f;
    private float coolDownValue = 0.5f;

    public SM_BlockingState(DriverData driver, BlockingInfo info) : base(driver)
    {
        sm_name = "Blocking State";
        blockingInfo = info;
    }


    protected override void Enter()
    {
        if(blockingInfo.nextCorner != null)
        {
            nextCornerObj = blockingInfo.nextCorner.obj;
        }

        sm_driver.blockingCooldown = coolDownValue;

        base.Enter();
    }


    protected override void Update()
    {

        if(nextCornerObj != null)
        {
            //if value is less, this is danger zone || for transition back to Normal state
            if (Mathf.Abs(Vector3.Distance(sm_driver.transform.position, nextCornerObj.transform.position))
                                    < minDistanceAllowance)
            {
                TriggerExit(new SM_NormalState(sm_driver));
            }
        }

        //TO-DO: GET DISTANCE ON THE RIGHT X-AXIS
        //       IF THE DISTNACE IS BETWEEN A THRESHOLD 
        //       IGNORE/ABORT BLOCK 
        //       AND TARGET IS BEHIND US, BLOCKING SUCCESSFUL 

        float distanceOnX = 0.0f;
        distanceOnX = Mathf.Abs(blockingInfo.target.transform.position.x - sm_driver.transform.position.x);
        float threshold = 1.0f;          //distance to 1.5 units

        if (distanceOnX > threshold)
        {
            float steerDir = DirectionTo(blockingInfo.target.transform.position);
            steerDir = Mathf.Clamp(steerDir, -1, 1);
            //steerDir = steerDir * blocksteerIntensity;

            sm_driver.engine.Move(0.85f, 0.0f, steerDir);
        }
        else
        {
            TriggerExit(new SM_NormalState(sm_driver));
        }



        base.Update();
    }

    protected override void Exit()
    {
        //TO-DO: PROPER GHOST DESTROY

        //blockingInfo.nextCorner.Destroy();

        base.Exit();
    }


    //TO-DO: REFACTORIZE TO MAYBE CUSTOM MATH
    private float DirectionTo(Vector3 target)
    {
        float dot;

        Vector3 right = sm_driver.transform.TransformDirection(sm_driver.transform.right);
        Vector3 toOther = target - sm_driver.transform.position;

        dot = Vector3.Dot(right, toOther);

        return dot;
    }

}
