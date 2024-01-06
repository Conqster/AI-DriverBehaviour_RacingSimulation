using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvertakingSideTest : MonoBehaviour
{

    public Transform opponent;
    public Transform overtakeNode;
    public bool performCheck = false;

    public float dot;
    public float dot2;

    public float angle;

    private void Update()
    {
        if (performCheck)
            OvertakeCriteria();
    }


    private void OvertakeCriteria()
    {
        Vector3 vecToOpponent = opponent.position - transform.position; 
        Vector3 vecToOvertake = overtakeNode.position - transform.position;




        Debug.DrawRay(transform.position, vecToOpponent, Color.red/*, 5.0f*/);
        Debug.DrawRay(transform.position, vecToOvertake, Color.magenta/*, 5.0f*/);
        //performCheck = false;


        vecToOpponent.Normalize();
        vecToOvertake.Normalize();

        vecToOpponent = vecToOpponent * -1;
        vecToOvertake = vecToOvertake * -1;

        dot = Vector3.Dot(vecToOvertake, vecToOpponent);
        dot2 = Vector3.Dot(vecToOpponent, vecToOvertake);

        angle = (Mathf.Acos(dot/(vecToOpponent.magnitude * vecToOvertake.magnitude))) * Mathf.Rad2Deg;
    }

}
