using System.Collections;
using System.Collections.Generic;
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
}
