using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class UnderCheckWaypointBoundary : MonoBehaviour
{
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

    public Bounds GetBoundVolume()
    {
        Point shortestPoint = points[0];
        Point secondShortestPoint = points[1];

        foreach (Point p in points)
        {
            if (p.distance < shortestPoint.distance)
            {
                secondShortestPoint = shortestPoint;
                shortestPoint = p;
            }
            else if (p.distance < secondShortestPoint.distance)
            {
                secondShortestPoint = p;
            }
        }

        Vector3 volumeSize = Vector3.zero;
        volumeSize += CustomMath.AbsVector(shortestPoint.direction) * shortestPoint.distance;
        volumeSize += CustomMath.AbsVector(secondShortestPoint.direction) * secondShortestPoint.distance;
        volumeSize.y = boundaryHeight;

        Quaternion rotationQuat = transform.rotation;
        Matrix4x4 rotMatrix = Matrix4x4.TRS(Vector3.zero, rotationQuat, Vector3.one);

        Vector3 rotatedSize = rotMatrix.MultiplyVector(volumeSize);

        return new Bounds(transform.position, rotatedSize);
    }

    private void UpdateBoundaryCheck()
    {
        points.Clear();
        CheckBoundaryDirection(transform.right);
        CheckBoundaryDirection(-transform.right);
        CheckBoundaryDirection(transform.forward);
        CheckBoundaryDirection(-transform.forward);
        objectPosition = transform.position;
        objectRotation = transform.rotation;
    }

    private void CheckBoundaryDirection(Vector3 boundaryDirection)
    {
        float distanceOffset = 0.1f;
        Vector3 lastPos = transform.position;
        lastPos.y -= 2.0f;
        NavMeshHit hit;

        while (IsPositionOnNavMesh(lastPos, out hit))
        {
            lastPos += boundaryDirection * distanceOffset;
        }

        float length = Vector3.Distance(transform.position, lastPos);
        points.Add(new Point(lastPos, hit.position, boundaryDirection, length));
    }

    private bool IsPositionOnNavMesh(Vector3 pos, out NavMeshHit hitData)
    {
        return NavMesh.SamplePosition(pos, out hitData, 0.1f, NavMesh.AllAreas);
    }

    private void OnDrawGizmos()
    {
        if (showHeight)
        {
            Vector3 temp = transform.position - (Vector3.up * 2.0f);
            Gizmos.color = IsPositionOnNavMesh(temp, out _) ? Color.red : Color.blue;
            Gizmos.DrawLine(transform.position, temp);
        }

        if (showBounds && points.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (Point p in points)
            {
                Gizmos.DrawLine(transform.position + p.direction * p.distance, p.position);
                Gizmos.DrawSphere(p.pointOnNav, debugBoundaryPointsSize);
            }

            Point shortestPoint = points[0];
            Point secondShortestPoint = points[1];

            foreach (Point p in points)
            {
                if (p.distance < shortestPoint.distance)
                {
                    secondShortestPoint = shortestPoint;
                    shortestPoint = p;
                }
                else if (p.distance < secondShortestPoint.distance)
                {
                    secondShortestPoint = p;
                }
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(shortestPoint.pointOnNav, 0.3f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(secondShortestPoint.pointOnNav, 0.3f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(shortestPoint.pointOnNav, secondShortestPoint.pointOnNav);

            Vector3 volumeSize = Vector3.zero;
            volumeSize += CustomMath.AbsVector(shortestPoint.direction) * shortestPoint.distance;
            volumeSize += CustomMath.AbsVector(secondShortestPoint.direction) * secondShortestPoint.distance;
            volumeSize.y = boundaryHeight;

            Quaternion rotationQuat = transform.rotation;
            Matrix4x4 rotMatrix = Matrix4x4.TRS(Vector3.zero, rotationQuat, Vector3.one);

            Vector3 rotatedSize = rotMatrix.MultiplyVector(volumeSize);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.position, rotatedSize);
        }
    }
}
