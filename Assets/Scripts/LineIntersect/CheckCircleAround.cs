using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;



public class CheckCircleAround : MonoBehaviour
{
    public float northDist = 0.0f;
    public float eastDist = 0.0f;
    public float southDist = 0.0f;
    public float westDist = 0.0f;

    private float startDist = 5.0f;
    private NavMeshHit hit;

    private List<Point> points = new List<Point>();
    public bool check = false;
    public bool checkNewMethod = false;
    [SerializeField, Range(0.0f, 20.0f)] private float testPos = 3.0f;
    public Vector3 objectPosition;
    public Quaternion objectRotation;

    private Vector3 testVolume = Vector3.zero;
    public float shortestDist = 0.0f;
    public Vector3 shortestDirection = Vector3.zero;
    public float secShortestDist = 0.0f;
    public Vector3 secShortesDirection = Vector3.zero;
    public Vector3 nullifiedDirection = Vector3.zero;

    private void Start()
    {
        UpdateBoundaryCheck();
    }



    private void Update()
    {
        Vector3 pos = transform.position;

        if(objectPosition != transform.position || objectRotation != transform.rotation)
        {
            UpdateBoundaryCheck();
        }


        //check boundary volume
        bool waiting = false;   
        if(waiting)
        {


        }

    }


    public Bounds GetBoundVolume()
    {

        float shortestLength = Mathf.Infinity;
        float secondShortestLength = Mathf.Infinity;
        Point shortestPoint = new Point();
        Point secondShortestPoint = new Point();

        Gizmos.color = Color.yellow;
        foreach (Point p in points)
        {
            //Gizmos.DrawLine(transform.position + p.direction * p.distance, p.position);
            //Gizmos.DrawSphere(p.pointOnNav, testPos);

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
                    secondShortestPoint = p;
                }
            }
        }

        Vector3 volumeSize = new Vector3(0.0f, 5.0f, 0.0f);
        
        //volumeSize += CustomMath.AbsVector(shortestPoint.direction) * shortestPoint.distance + CustomMath.AbsVector(secondShortestPoint.direction) * secondShortestPoint.distance;
        
        
        volumeSize += CustomMath.AbsVector(shortestPoint.direction) * shortestPoint.distance + CustomMath.AbsVector(secondShortestPoint.direction) * secondShortestPoint.distance;

        Quaternion rotationQuat = transform.rotation;
        Matrix4x4 rotMatrix = Matrix4x4.TRS(Vector3.zero, rotationQuat, Vector3.one);

        Vector3 rotatedCenter = rotMatrix.MultiplyPoint(transform.position);
        Vector3 rotatedSize = rotMatrix.MultiplyVector(volumeSize);


        return new Bounds(transform.position, rotatedSize);
    }

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


    private bool IsPositionOnNavMesh(Vector3 pos, out NavMeshHit hitData)
    {

        if(NavMesh.SamplePosition(pos, out hitData, 0.1f, NavMesh.AllAreas))
        { 
            return true; 
        }

        return false;
    }


    private Vector3 CheckBoundary(float downwardOffset, out Vector3 point, Vector3 boundaryDirection, out float length)
    {
        bool onNavMesh = false;
        float distance = 0.0f;
        float distanceOffset = 0.1f;
        Vector3 lastPos = transform.position;
        lastPos.y += downwardOffset;
        NavMeshHit hit;
        point = Vector3.zero;

        while(IsPositionOnNavMesh(lastPos, out hit))
        {
            lastPos += boundaryDirection * distanceOffset;
            point = hit.position;
        }

        length = Mathf.Sqrt((transform.position.x - lastPos.x) * (transform.position.x - lastPos.x) +
                            (transform.position.z - lastPos.z) * (transform.position.z - lastPos.z));

        return lastPos;
    }





    private void OnDrawGizmos()
    {

        Vector3 temp = transform.position - (Vector3.up * 2.0f);
        Gizmos.color = (IsPositionOnNavMesh(temp, out hit)) ? Color.red : Color.blue;
        Gizmos.DrawLine(transform.position, temp);

        
        if(check)
        {
            Gizmos.color = Color.yellow;
            Vector3 pointPos = transform.position;
            float length = 0.0f;
            float shortest = Mathf.Infinity;
            Vector3 firstShortPoint = Vector3.zero;
            Vector3 secondShortPoint = Vector3.zero;

            //left
            Vector3 position = CheckBoundary(-2.0f, out pointPos, -transform.right, out length);
            Gizmos.DrawLine(transform.position - (transform.right * length), position);
            Gizmos.DrawSphere(pointPos, testPos);

            if(length < shortest)
            {
                print("Length at left: " + length);
                shortest = length;
                firstShortPoint = pointPos;
                
            }


            //right
            position = CheckBoundary(-2.0f, out pointPos, transform.right, out length);
            Gizmos.DrawLine(transform.position + (transform.right * length), position);
            Gizmos.DrawSphere(pointPos, testPos);


            if (length < shortest)
            {
                print("Length at right: " + length);
                shortest = length;
                firstShortPoint = pointPos;
            }

            //forward
            position = CheckBoundary(-2.0f, out pointPos, transform.forward, out length);
            Gizmos.DrawLine(transform.position + (transform.forward * length), position);
            Gizmos.DrawSphere(pointPos, testPos);


            if (length < shortest)
            {
                print("Length at forward: " + length);
                shortest = length;
                firstShortPoint = pointPos;
            }

            //BACk
            position = CheckBoundary(-2.0f, out pointPos, -transform.forward, out length);
            Gizmos.DrawLine(transform.position - (transform.forward * length), position);
            Gizmos.DrawSphere(pointPos, testPos);

            if (length < shortest)
            {
                print("Length at back: " + length);
                shortest = length;
                firstShortPoint = pointPos;
            }
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(firstShortPoint, testPos);


        }

        if(checkNewMethod)
        {

            if (points.Count > 0)
            {

                float shortestLength = Mathf.Infinity;
                float secondShortestLength = Mathf.Infinity;
                Point shortestPoint = new Point();
                Point secondShortestPoint = new Point();

                Gizmos.color = Color.yellow;
                foreach (Point p in points)
                {
                    Gizmos.DrawLine(transform.position + p.direction * p.distance, p.position);
                    Gizmos.DrawSphere(p.pointOnNav, testPos);

                    if(p.distance < shortestLength)
                    {
                        shortestLength = p.distance;
                        shortestPoint = p;
                    }
                }

                foreach(Point p in points)
                {
                    if(p.distance < secondShortestLength)
                    {
                        if(p.distance > shortestLength)
                        {
                            secondShortestLength = p.distance;
                            secondShortestPoint = p;
                        }
                    }
                }

                shortestDist = shortestPoint.distance;
                shortestDirection = shortestPoint.direction;
                secShortestDist = secondShortestPoint.distance;
                secShortesDirection = secondShortestPoint.direction;

                

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

}
