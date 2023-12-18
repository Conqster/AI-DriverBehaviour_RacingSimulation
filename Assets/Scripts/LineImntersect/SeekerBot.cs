using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SeekerBot : MonoBehaviour
{


    public bool checkVolume = false;
    public WaypointBoundaryCheck currentWayPoint;

    private Vector3 hitPoint;

    public Transform mainTarget = null;
    //a ghost as target
    public Transform ghostTarget = null;
    public Transform currentTarget = null;
    public Material mat;

    private void Start()
    {
        GameObject ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ghost.name = "Ghost";
        ghost.transform.parent = transform;
        ghostTarget = ghost.transform; 

        if (mat != null)
        {
            ghost.GetComponent<Renderer>().material = mat;
        }
    }

    private void Update()
    {
        Move();
        
        if(checkVolume && currentWayPoint != null)
        {
            Bounds targetVolume = currentWayPoint.GetBoundVolume();
            RaycastHit hitInfo;

            if(CustomMath.RayIntersectsVolume(transform.position, transform.forward, targetVolume, out hitInfo))
            {
                hitPoint = hitInfo.point;
                ghostTarget.position = hitInfo.point;
                currentTarget = ghostTarget;
            }
            else
            {
                hitPoint = Vector3.zero;
                ghostTarget.position = Vector3.zero;
                currentTarget = mainTarget;
            }
        }

    }


    private void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        transform.position += new Vector3(v, 0.0f, h) * 10.0f * Time.deltaTime;

    }





    private void OnDrawGizmos()
    {
        if (checkVolume && currentWayPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(hitPoint, 0.2f);

        }

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 3.0f);
    }
}
