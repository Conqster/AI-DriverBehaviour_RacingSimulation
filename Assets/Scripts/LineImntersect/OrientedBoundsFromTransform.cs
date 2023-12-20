using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientedBoundsFromTransform : MonoBehaviour
{
    void Update()
    {
        Bounds orientedBounds = CalculateOrientedBoundsFromTransform(transform);

        // Use orientedBounds for further operations or checks
        //Debug.Log("Center: " + orientedBounds.center);
        //Debug.Log("Size: " + orientedBounds.size);
    }

    Bounds CalculateOrientedBoundsFromTransform(Transform targetTransform)
    {
        Vector3 center = targetTransform.position;
        Vector3 size = Vector3.one; // Set the default size or adjust based on your requirements
        Vector3 rotation = targetTransform.eulerAngles;

        Quaternion rotationQuaternion = Quaternion.Euler(rotation);
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, rotationQuaternion, Vector3.one);

        Vector3 rotatedCenter = rotationMatrix.MultiplyPoint(center);
        Vector3 rotatedSize = rotationMatrix.MultiplyVector(size);

        Bounds orientedBounds = new Bounds(rotatedCenter, rotatedSize);

        return orientedBounds;
    }


    private void OnDrawGizmos()
    {
        Vector3 center = transform.position;
        Vector3 size = Vector3.one; // Set the default size or adjust based on your requirements
        Vector3 rotation = transform.eulerAngles;

        Quaternion rotationQuaternion = Quaternion.Euler(rotation);
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, rotationQuaternion, Vector3.one);

        Vector3 rotatedCenter = rotationMatrix.MultiplyPoint(center);
        Vector3 rotatedSize = rotationMatrix.MultiplyVector(size);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(rotatedCenter, rotatedSize);    
    }
}

