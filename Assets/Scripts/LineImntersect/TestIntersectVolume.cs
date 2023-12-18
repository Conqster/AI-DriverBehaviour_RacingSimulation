using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestIntersectVolume : MonoBehaviour
{

    public Transform target;


    public Vector3 volumeCenter = Vector3.zero;
    [SerializeField, Range(0.0f, 10.0f)] private float volumeSize = 1.0f;

    public Vector3 hitPoint;



    private void Update()
    {
        

        if(target != null)
        {
            volumeCenter = target.position;
            RaycastHit hitInfo;

            if (RayIntersectsVolume(transform.position, transform.forward, out hitInfo))
            {
                hitPoint = hitInfo.point;
            }
            else
                hitPoint = Vector3.zero;
        }
    }





    private bool RayIntersectsVolume(Vector3 rayOrigin, Vector3 rayDirection, out RaycastHit hit)
    {
        hit = new RaycastHit();

        Bounds volumeBounds = new Bounds(volumeCenter, new Vector3(volumeSize, volumeSize, volumeSize));

        if (volumeBounds.IntersectRay(new Ray(rayOrigin, rayDirection), out float distance))
        {
            hit.point = rayOrigin + rayDirection * distance;
            hit.distance = distance;
            return true;
        }

        return false;
    }



    private void OnDrawGizmos()
    {
        if( target != null )
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(hitPoint, 0.2f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(target.position, new Vector3(volumeSize, volumeSize, volumeSize));
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position,transform.position + transform.forward * 3.0f);


        Vector3 rotation = transform.eulerAngles;
        Quaternion rotationQuat = Quaternion.Euler(rotation);
        Matrix4x4 rotMatrix = Matrix4x4.TRS(Vector3.zero, rotationQuat, Vector3.one);

        Vector3 rotatedCenter = rotMatrix.MultiplyPoint(transform.position);
        Vector3 rotatedSize = rotMatrix.MultiplyVector(new Vector3(volumeSize, volumeSize, volumeSize));
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, rotatedSize);
    }

}
