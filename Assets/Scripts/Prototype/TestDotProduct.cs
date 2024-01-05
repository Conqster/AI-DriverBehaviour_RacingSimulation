using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDotProduct : MonoBehaviour
{
    public Transform target;
    public float value;


    private void Update()
    {
        if (target)
        {
            Vector3 forward = transform.TransformDirection(transform.forward);
            Vector3 toOther = target.position - transform.position;

            if (Vector3.Dot(forward, toOther) < 0)
            {
                //print("The other transform is behind me!");
            }
            if(Vector3.Dot(forward, toOther) > 0)
            {

                //print("The other transform is ahead me!");
            }

            value = Vector3.Dot(forward, toOther);
        }
    }



    private void OnDrawGizmos()
    {
         


        Gizmos.color = Color.blue;   

        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2);   
    }
}
