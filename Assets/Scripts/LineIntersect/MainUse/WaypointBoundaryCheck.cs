using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;





[System.Serializable]
public class Point
{
    public Vector3 position;
    public Vector3 pointOnNav;
    public Vector3 direction;
    public float distance;

    public Point() { }

    public Point(Vector3 position, Vector3 pointOnNav, Vector3 direction, float distance)
    {
        this.position = position;
        this.pointOnNav = pointOnNav;
        this.direction = direction;
        this.distance = distance;
    }
}

public class WaypointBoundaryCheck : MonoBehaviour
{

    private NavMeshHit hit;
    private List<Point> points = new List<Point>();
    [SerializeField, Range(0.0f, 10.0f)] private float boundaryHeight = 3.0f;

    private Vector3 objectPosition;
    private Quaternion objectRotation;

    [Header("Debugger")]
    [SerializeField] private bool showHeight = true;
    [SerializeField] private bool showBounds = true;
    [SerializeField, Range(0.0f, 3.0f)] private float debugBoundaryPointsSize = 0.1f;


    private void Start()
    {
        UpdateBoundaryCheck();
    }

    private void Update()
    {
        if (objectPosition != transform.position || objectRotation != transform.rotation)
        {
            UpdateBoundaryCheck();
        }

    }



    /// <summary>
    /// creates and returns a bound based on the points stored in list 
    /// </summary>
    /// <returns></returns>
    public Bounds GetBoundVolume()
    {
        Point shortestPoint = new Point();
        Point secondShortestPoint = new Point();

        Gizmos.color = Color.yellow;
        TwoShortestPoint(points,ref shortestPoint,ref secondShortestPoint);

        Vector3 volumeSize = new Vector3(0.0f, boundaryHeight, 0.0f);

        volumeSize += CustomMath.AbsVector(shortestPoint.direction) * shortestPoint.distance + CustomMath.AbsVector(secondShortestPoint.direction) * secondShortestPoint.distance;

        Quaternion rotationQuat = transform.rotation;
        Matrix4x4 rotMatrix = Matrix4x4.TRS(Vector3.zero, rotationQuat, Vector3.one);

        Vector3 rotatedCenter = rotMatrix.MultiplyPoint(transform.position);
        Vector3 rotatedSize = rotMatrix.MultiplyVector(volumeSize);


        return new Bounds(transform.position, rotatedSize);
    }


    /// <summary>
    /// Calculates the 2 shortest from the list points
    /// based on their distance to center
    /// </summary>
    /// <param name="points"></param>
    /// <param name="shortestPoint"></param>
    /// <param name="secShortestPoint"></param>
    private void TwoShortestPoint(List<Point> points, ref Point shortestPoint, ref Point secShortestPoint)
    {

        float shortestLength = Mathf.Infinity;
        float secondShortestLength = Mathf.Infinity;

        foreach (Point p in points)
        {

            if (p.distance < shortestLength)
            {
                shortestLength = p.distance;
                shortestPoint = p;
            }
        }

        foreach (Point p in points)
        {
            if (p.distance < secondShortestLength)
            {
                if (p.distance > shortestLength)
                {
                    secondShortestLength = p.distance;
                    secShortestPoint = p;
                }
            }
        }
    }


    /// <summary>
    /// Updates the boundraies
    /// Currently using dircetions forward, backwards, right, left for boundary check 
    /// and updates the points list based on the boundary check
    /// </summary>
    private void UpdateBoundaryCheck()
    {

        Vector3 pointPos = transform.position;
        float length = 0.0f;

        Vector3 position = CheckBoundary(-2.0f, out pointPos, transform.right, out length);
        Point rightPoint = new Point(position, pointPos, transform.right, length);

        position = CheckBoundary(-2.0f, out pointPos, -transform.right, out length);
        Point leftPoint = new Point(position, pointPos, -transform.right, length);

        position = CheckBoundary(-2.0f, out pointPos, transform.forward, out length);
        Point forwardPoint = new Point(position, pointPos, transform.forward, length);

        position = CheckBoundary(-2.0f, out pointPos, -transform.forward, out length);
        Point backwardPoint = new Point(position, pointPos, -transform.forward, length);


        points.Clear();
        points.Add(rightPoint);
        points.Add(leftPoint);
        points.Add(forwardPoint);
        points.Add(backwardPoint);

        objectPosition = transform.position;
        objectRotation = transform.rotation;
    }



    /// <summary>
    /// Check for the boundary of a nav mesh 
    /// Starting from the center of an obj
    /// And walks way outwards based on the boundary direction 
    /// length is the distance to the center 
    /// stops when there is no more nav mesh 
    /// </summary>
    /// <param name="downwardOffset"></param>
    /// <param name="point"></param>
    /// <param name="boundaryDirection"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    private Vector3 CheckBoundary(float downwardOffset, out Vector3 point, Vector3 boundaryDirection, out float length)
    {
        float distanceOffset = 0.1f;
        Vector3 lastPos = transform.position;
        lastPos.y += downwardOffset;
        NavMeshHit hit;
        point = Vector3.zero;

        while (IsPositionOnNavMesh(lastPos, out hit))
        {
            lastPos += boundaryDirection * distanceOffset;
            point = hit.position;
        }

        length = Mathf.Sqrt((transform.position.x - lastPos.x) * (transform.position.x - lastPos.x) +
                            (transform.position.z - lastPos.z) * (transform.position.z - lastPos.z));

        return lastPos;
    }



    /// <summary>
    /// Check is pos is on Nav mesh 
    /// if so return else false
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="hitData"></param>
    /// <returns></returns>
    private bool IsPositionOnNavMesh(Vector3 pos, out NavMeshHit hitData)
    {

        if (NavMesh.SamplePosition(pos, out hitData, 0.1f, NavMesh.AllAreas))
        {
            return true;
        }

        return false;
    }




    private void OnDrawGizmos()
    {

        if(showHeight)
        {
            Vector3 temp = transform.position - (Vector3.up * 2.0f);
            Gizmos.color = (IsPositionOnNavMesh(temp, out hit)) ? Color.red : Color.blue;
            Gizmos.DrawLine(transform.position, temp);
        }

        if(showBounds && points.Count > 0)
        {

            Point shortestPoint = new Point();
            Point secondShortestPoint = new Point();

            Gizmos.color = Color.yellow;
            foreach (Point p in points)
            {
                Gizmos.DrawLine(transform.position + p.direction * p.distance, p.position);
                Gizmos.DrawSphere(p.pointOnNav, debugBoundaryPointsSize);
            }

            TwoShortestPoint(points, ref shortestPoint, ref secondShortestPoint);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(shortestPoint.pointOnNav, 0.3f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(secondShortestPoint.pointOnNav, 0.3f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(shortestPoint.pointOnNav, secondShortestPoint.pointOnNav);

            Gizmos.color = Color.magenta;

            Vector3 volumeSize = new Vector3(0.0f, 5.0f, 0.0f);
            volumeSize += CustomMath.AbsVector(shortestPoint.direction) * shortestPoint.distance + CustomMath.AbsVector(secondShortestPoint.direction) * secondShortestPoint.distance;

            Vector3 rotation = transform.eulerAngles;
            Quaternion rotationQuat = Quaternion.Euler(rotation);
            Matrix4x4 rotMatrix = Matrix4x4.TRS(Vector3.zero, rotationQuat, Vector3.one);

            Vector3 rotatedCenter = rotMatrix.MultiplyPoint(transform.position);
            Vector3 rotatedSize = rotMatrix.MultiplyVector(volumeSize);

            Gizmos.DrawWireCube(transform.position, rotatedSize);
        }
    }


}




