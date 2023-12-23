using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFacingSameDirection : MonoBehaviour
{
    public Transform target;
    public bool yes = false;

    public  float calDot;


    private void Update()
    {

        if(target != null)
        {
            yes = CheckDriverParalleToTracker(target.right);
        }
        
    }


    private bool CheckDriverParalleToTracker(Vector3 targetForward)
    {
        //Vector3 me = transform.forward;
        ////ignore X rot 

        ////make the transform of the same elevation as the car
        //targetForward.y = me.y; 

        float dot = Vector3.Dot(transform.right, targetForward);
        calDot = dot;

        return (dot > 0.995f);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (transform.forward * 2.0f));
        Gizmos.DrawLine(transform.position, transform.position + transform.right);

        if(target != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(target.position, target.position + (target.forward * 2.0f));
            Gizmos.DrawLine(target.position, target.position + target.right);
        }
    }
}
