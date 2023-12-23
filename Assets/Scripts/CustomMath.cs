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
