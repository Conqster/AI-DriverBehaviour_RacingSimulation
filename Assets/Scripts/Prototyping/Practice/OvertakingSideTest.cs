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

    [Space]
    public bool performCheck2 = false;
    public int whatSide;

    [Space]
    public CarAI.ObstacleSystem.WhiskeySearchType searchType = CarAI.ObstacleSystem.WhiskeySearchType.SingleOnly;

    private void Update()
    {
        if (performCheck)
            OvertakeCriteria();

        if (performCheck2)
            whatSide = CheckSideToOpponent(opponent);

        DoRayCast();    
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


    private int CheckSideToOpponent(Transform opponent)
    {
        float side = 0;

        Vector3 opponentRight = opponent.right;
        Vector3 vecToOppenent = (opponent.position - transform.position).normalized;

        side = Vector3.Dot(opponentRight, vecToOppenent);

        return (side > 0) ? -1 : 1;
    }



    private void DoRayCast()
    {

        Vector3 origin = Vector3.zero;
        Vector3 mid = transform.forward;

        

        float angle = 20.0f;
        float length = 15.0f;

        Vector3 leftPoint = Vector3.zero;
        Vector3 rightPoint = Vector3.zero;


        switch(searchType)
        {
            case CarAI.ObstacleSystem.WhiskeySearchType.CentralWithParallel:
                origin = transform.position;
                leftPoint = transform.position - transform.right * 2.0f;
                rightPoint = transform.position + transform.right * 2.0f;

                Debug.DrawRay(origin, transform.forward * length, Color.yellow);
                Debug.DrawRay(leftPoint, transform.forward * length, Color.green);
                Debug.DrawRay(rightPoint, transform.forward * length, Color.green);

                break;
            case CarAI.ObstacleSystem.WhiskeySearchType.CentralRayWithWhiskey:
                origin = transform.position;
                rightPoint = (Quaternion.Euler(0, angle, 0) * transform.forward);
                leftPoint = (Quaternion.Euler(0, -angle, 0) * transform.forward);


                Debug.DrawRay(origin, transform.forward * length, Color.yellow);
                Debug.DrawRay(origin, rightPoint * length, Color.green);
                Debug.DrawRay(origin, leftPoint * length, Color.green);

                break;
            case CarAI.ObstacleSystem.WhiskeySearchType.SingleOnly:
                origin = transform.position;
                Debug.DrawRay(origin, transform.forward * length, Color.yellow);
                break;
        }


    }

}
