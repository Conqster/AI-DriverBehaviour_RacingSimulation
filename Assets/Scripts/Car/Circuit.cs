using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circuit : MonoBehaviour
{
    public List<Transform> waypoints = new List<Transform>();


    private void OnDrawGizmos()
    {
        if (waypoints == null && waypoints.Count > 0)
            return;

        for(int i = 0; i < waypoints.Count; i++)
        {
            Gizmos.color = Color.white;
            if (i != (waypoints.Count - 1))
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
            else
            {
                //print("This is the breakout value: " + i);
                Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
            }

        }
    }

    private void OnDrawGizmosSelected()
    {
        if(waypoints != null && waypoints.Count > 0)
        {
            for(int i = 0; i < waypoints.Count; i++)
            {
                //print(i);
                
                Gizmos.DrawWireSphere(waypoints[i].position, 10.0f);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(waypoints[i].position, 15.0f);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(waypoints[i].position, 35.0f);
            }
        }
    }
}
