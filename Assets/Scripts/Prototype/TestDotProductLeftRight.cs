using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDotProductLeftRight : MonoBehaviour
{
    public Transform target;
    public float value;
    public string direction = "Neutral";

    private void Update()
    {
        
        if(target)
        {
            Vector3 right = transform.TransformDirection(transform.right);
            Vector3 toOther = target.position - transform.position;

            if(Vector3.Dot(right, toOther) < 0 )
            {
                print("The other transform is on my left!");
                direction = "Left";
            }

            if(Vector3.Dot(right, toOther) > 0)
            {
                print("The other transform is to my right!");
                direction = "Right";
            }

            value = Vector3.Dot(right, toOther);
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * 2);



        Gizmos.color = Color.blue;

        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2);
    }

}
