using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circuit : MonoBehaviour
{
    public List<Transform> waypoints = new List<Transform>();

    [Header("Debug Colour")]
    [SerializeField] private Color startColour = Color.red;
    [SerializeField] private Color endColour = Color.black;
    [SerializeField] private Color pathLine = Color.green;

    private void OnDrawGizmos()
    {
        if (waypoints == null && waypoints.Count > 0)
            return;

        for(int i = 0; i < waypoints.Count; i++)
        {
            Gizmos.color = pathLine;
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

        Colour();
    }


    private void Colour()
    {
        Renderer startRend = waypoints[0].GetComponent<Renderer>();
        startRend.material.color = startColour;
        
        Renderer endRend = waypoints[waypoints.Count - 1].GetComponent<Renderer>();
        endRend.material.color = endColour; 

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
