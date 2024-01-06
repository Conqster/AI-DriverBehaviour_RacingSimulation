using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayWithCircuit : MonoBehaviour
{
    public List<Transform> nodes = new List<Transform>();


    [Header("Debugger")]
    public Color pathColour = Color.green;
    public bool drawLines = true;




    private void OnDrawGizmos()
    {
        

        if(nodes.Count > 0 && drawLines)
        {
            for(int i = 0; i < nodes.Count; i++)
            {
                Gizmos.color = pathColour;
                if(i != (nodes.Count - 1))
                {
                    Gizmos.DrawLine(nodes[i].position, nodes[i+1].position);

                }
            }
        }
    }


}
