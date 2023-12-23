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

    [Header("Debugger")]
    [SerializeField] private bool drawSphere = false;
    [SerializeField] private bool drawLine = true;
    private bool done = false;

    private void OnDrawGizmos()
    {
        if (waypoints == null && waypoints.Count > 0 && !drawLine)
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

        if(!done)
            Colour();
    }


    private void Colour()
    {

        Renderer endRend = waypoints[waypoints.Count - 1].GetComponent<Renderer>();
        Material defaultMat = endRend.GetComponent<Renderer>().sharedMaterial;

        Material instanceMat1 = new Material(defaultMat);
        Material instanceMat2 = new Material(defaultMat);

        endRend.material = instanceMat1;
        endRend.sharedMaterial.color = endColour;


        Renderer startRend = waypoints[0].GetComponent<Renderer>();
        startRend.material = instanceMat2;
        startRend.sharedMaterial.color = startColour;
        
        done = true;
    }

    private void OnDrawGizmosSelected()
    {
        if(waypoints != null && waypoints.Count > 0 && drawSphere)
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
