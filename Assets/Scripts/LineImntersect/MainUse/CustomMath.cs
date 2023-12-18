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
}
