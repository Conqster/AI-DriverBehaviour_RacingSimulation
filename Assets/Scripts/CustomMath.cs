using Palmmedia.ReportGenerator.Core;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public static class CustomMath
{
    public static Vector3 AbsVector(Vector3 v)
    {
        Vector3 temp = Vector3.zero;

        temp.x = Mathf.Abs(v.x);
        temp.y = Mathf.Abs(v.y);
        temp.z = Mathf.Abs(v.z);

        return temp;
    }



    public static float GetRatio(float min, float max, float value)
    {
        try
        {
            float? ratio = Ratio(min, max, value);
            if (ratio != null)
            {
                return (float)ratio;
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
        return 0;
    }


    private static float? Ratio(float min, float max, float value)
    {

        if (min == max)
        {
            throw new System.ArgumentException($"Invalid input: min = {min} are equal max = {max}");
        }
        else if (value < min)
        {
            throw new System.ArgumentOutOfRangeException($"Invalid input: value = {value} is less than min = {min}");
        }
        else if (value > max)
        {
            throw new System.ArgumentOutOfRangeException($"Invalid input: value = {value} is greater than max = {max}");
        }

        return Mathf.Clamp01((value - min) / (max - min));
    }

    public static bool RayIntersectsVolume(Vector3 rayOrigin, Vector3 rayDirection, Bounds volume, out RaycastHit hit)
    {
        hit = new RaycastHit();


        if (volume.IntersectRay(new Ray(rayOrigin, rayDirection), out float distance))
        {
            hit.point = rayOrigin + rayDirection * distance;
            hit.distance = distance;
            return true;
        }

        return false;
    }

    public static void DebugObject(PrimitiveType primitive, Vector3 position, Material mat)
    {
        GameObject debuggerObj = GameObject.CreatePrimitive(primitive);

        debuggerObj.name = "debuggerObj: " + position;
        debuggerObj.transform.position = position;

        if (mat != null)
        {
            debuggerObj.GetComponent<Renderer>().material = mat;
        }

        if(debuggerObj.TryGetComponent<Collider>(out Collider col))
        {
            Object.Destroy(col);
        }
    }


    public static Vector3[] GeneratePoints(Vector3 startPoint, int waypointIndex, List<Transform> mainPath, float intervals = 8.0f, int numOfPoints = 3, bool debugLine = false, float duration = 5.0f)
    {
        Vector3[] points = new Vector3[numOfPoints];
        points[0] = startPoint;

        //get currentwayPoint
        int currentIndex = waypointIndex;

        Vector3 vecWayPointForward = Vector3.zero;

        for (int i = 0; i < points.Length - 1; i++)
        {
            while (!PointIsBehind(points[i], currentIndex, mainPath))
                currentIndex = (currentIndex + 1) % mainPath.Count;


            if (PointIsBehind(points[i], currentIndex, mainPath))
            {
                //Bring out later
                int nextWayPointIndex = (currentIndex + 1) % mainPath.Count;
                if (nextWayPointIndex > mainPath.Count - 1)
                    nextWayPointIndex = 0;


                Vector3 newVecWayPointForward = (mainPath[nextWayPointIndex].position - mainPath[currentIndex].position).normalized;

                if (vecWayPointForward != Vector3.zero)
                {
                    //check angle btw vectors newVecWayPointForward and vecWayPointForward
                    float dot = Vector3.Dot(vecWayPointForward, newVecWayPointForward);
                    float angle = (Mathf.Acos(dot / (vecWayPointForward.magnitude * newVecWayPointForward.magnitude))) * Mathf.Rad2Deg;

                    if (angle > 20.0f)
                    {
                        //check if it clockwise or anticlockwise
                        Vector3 perpenVec = Vector3.Cross(vecWayPointForward, Vector3.up);
                        float newDot = Vector3.Dot(newVecWayPointForward, (perpenVec * -1f));


                        if (newDot < -0.0f)
                        {
                            newVecWayPointForward = vecWayPointForward;
                        }
                    }
                }

                int nextPoint = i + 1;

                if (nextPoint < points.Length)
                {
                    points[nextPoint] = points[i] + newVecWayPointForward * intervals;
                }

                //vecWayPointForward = newVecWayPointForward;
                vecWayPointForward = (mainPath[nextWayPointIndex].position - mainPath[currentIndex].position).normalized;
            }

        }



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

        return points;
    }


    private static bool PointIsBehind(Vector3 point, int waypointIndex, List<Transform> mainPath)
    {
        //The Forward of each node is define by the point of the next point 
        Vector3 vecPointWayPoint = (mainPath[waypointIndex].position - point).normalized;
        Debug.DrawRay(point, vecPointWayPoint, Color.yellow);
        int nextWayPointIndex = waypointIndex + 1;
        if (nextWayPointIndex > mainPath.Count - 1)
            nextWayPointIndex = 0;

        Vector3 vecWayPointForward = (mainPath[nextWayPointIndex].position - mainPath[waypointIndex].position).normalized;
        Debug.DrawRay(mainPath[waypointIndex].position, vecWayPointForward, Color.blue);

        return (Vector3.Dot(vecPointWayPoint, vecWayPointForward) > 0);
    }
}
