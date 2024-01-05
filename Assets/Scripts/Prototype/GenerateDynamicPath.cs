using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateDynamicPath : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private Transform startPoint;
    [SerializeField, Range(0, 50)] private int numOfPoints = 10;
    [SerializeField, Range(0, 10)] private float lengthBtwNodes = 3.5f;
    public bool generate = false;


    [Header("External Information")]
    [SerializeField, Range(0, 10)] private int currentWaypointIndex;
    public PlayWithCircuit circuit;
    public float dotProduct;
    public string message;
    public int lastCurrentIndex = 0;
    public Vector3[] points;

    private void Update()
    {
    

        if(generate)
            GeneratePoints(startPoint.position, circuit.nodes, lengthBtwNodes, numOfPoints, true);

        
    }



    private bool PointIsBehind(Vector3 point, int waypointIndex, List<Transform> mainPath)
    {
        //The Forward of each node is define by the point of the next point 
        Vector3 vecPointWayPoint = (mainPath[waypointIndex].position - point).normalized;
        Debug.DrawRay(point, vecPointWayPoint, Color.yellow);
        int nextWayPointIndex = waypointIndex + 1;
        if (nextWayPointIndex > mainPath.Count - 1)
            nextWayPointIndex = 0;

        Vector3 vecWayPointForward = (mainPath[nextWayPointIndex].position - mainPath[waypointIndex].position).normalized;
        Debug.DrawRay(mainPath[waypointIndex].position, vecWayPointForward, Color.blue);

        dotProduct = Vector3.Dot(vecPointWayPoint, vecWayPointForward);

        return dotProduct > 0;
    }


    private Vector3[] GeneratePoints(Vector3 startPoint, List<Transform> mainPath, float intervals = 8.0f, int numOfPoints = 3, bool debugLine = false, float duration = 5.0f)
    {
        points = new Vector3[numOfPoints];
        points[0] = startPoint;

        //get currentwayPoint
        int currentIndex = currentWaypointIndex;

        Vector3 vecWayPointForward = Vector3.zero;

        for (int i = 0; i < points.Length - 1 ; i++) 
        {
            while (!PointIsBehind(points[i], currentIndex, mainPath))
                currentIndex = (currentIndex + 1)% mainPath.Count;


            if (PointIsBehind(points[i], currentIndex, mainPath))
            {
                //Bring out later
                int nextWayPointIndex = (currentIndex + 1) % mainPath.Count;
                if (nextWayPointIndex > mainPath.Count - 1)
                    nextWayPointIndex = 0;

                
                Vector3 newVecWayPointForward = (mainPath[nextWayPointIndex].position - mainPath[currentIndex].position).normalized;

                if(vecWayPointForward != Vector3.zero)
                {
                    //check angle btw vectors newVecWayPointForward and vecWayPointForward
                    float dot = Vector3.Dot(vecWayPointForward, newVecWayPointForward);
                    float angle = (Mathf.Acos(dot / (vecWayPointForward.magnitude * newVecWayPointForward.magnitude))) * Mathf.Rad2Deg;

                    if (angle > 20.0f)
                    {
                        //check if it clockwise or anticlockwise
                        Vector3 perpenVec = Vector3.Cross(vecWayPointForward, Vector3.up);
                        float newDot = Vector3.Dot(newVecWayPointForward, (perpenVec * -1f));

                        print("Calledfdd: " + newDot + ",a: " + angle);

                        if (newDot < -0.0f)
                        {
                            newVecWayPointForward = vecWayPointForward;
                        }
                    }
                }

                int nextPoint = i + 1;

                if(nextPoint < points.Length)
                {
                    points[nextPoint] = points[i] + newVecWayPointForward * intervals;
                }

                //vecWayPointForward = newVecWayPointForward;
                vecWayPointForward = (mainPath[nextWayPointIndex].position - mainPath[currentIndex].position).normalized;
            }

        }

        

        lastCurrentIndex = currentIndex;

        if (points.Length > 0 && debugLine)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (i != (points.Length - 1))
                {
                    Debug.DrawLine(points[i], points[i + 1], Color.magenta, duration);

                }
                else if (i == (points.Length - 1))
                {
                    Vector3 VecForward = (points[i] - points[i - 1]).normalized;
                    Debug.DrawRay(points[i], VecForward * 5f, Color.red, duration);
                }
            }
        }

        generate = false;
        return points;
    }


  

}
