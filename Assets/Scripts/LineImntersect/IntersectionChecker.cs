using UnityEngine;

public class IntersectionChecker : MonoBehaviour
{
    // Define the points for two lines (start and end points)
    public Transform line1Start;
    public Transform line1End;
    public Transform line2Start;
    public Transform line2End;

    private Vector3 oldLine1Start;
    private Vector3 oldLine1End;
    private Vector3 oldLine2Start;
    private Vector3 oldLine2End;

    private Vector3 intersectPoint;

    void Start()
    {
        Calculate();
    }

    private void Update()
    {
        
        if(oldLine1Start != line1Start.position || oldLine1End != line1End.position
            || oldLine2Start != line2Start.position || oldLine2End != line2End.position)
        {
            Calculate();
        }

    }


    private void Calculate()
    {
        //// Calculate direction vectors for both lines
        //Vector3 line1Direction = line1End.position - line1Start.position;
        //Vector3 line2Direction = line2End.position - line2Start.position;

        // Calculate the intersection point (if it exists)
        Vector3 intersectionPoint = Vector3.zero;

        // Calculate denominators for the parametric equations
        //float denominator = Vector3.Cross(line1Direction, line2Direction).sqrMagnitude;


        if(LineSegmentsIntersection(line1Start.position, line1End.position, line2Start.position, line2End.position, out intersectionPoint))
        {
            intersectPoint = intersectionPoint;
            Debug.Log("Intersection point: " + intersectionPoint);
        }
        else
        {
            Debug.Log("Lines do not intersect within line segments.");
        }

        //// Check if the lines are not parallel
        //if (denominator != 0)
        //{
        //    // Calculate parameters for the parametric equations
        //    float t = Vector3.Cross(line2Start.position - line1Start.position, line2Direction).magnitude / denominator;
        //    float u = Vector3.Cross(line2Start.position - line1Start.position, line1Direction).magnitude / denominator;

        //    // Check if the intersection point is within the line segments
        //    if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        //    {
        //        intersectionPoint = line1Start.position + line1Direction * t;
        //        intersectPoint = intersectionPoint;
        //        Debug.Log("Intersection point: " + intersectionPoint);
        //    }
        //    else
        //    {
        //        Debug.Log("Lines do not intersect within line segments.");
        //    }
        //}
        //else
        //{
        //    Debug.Log("Lines are parallel and do not intersect.");
        //}


        oldLine1Start = line1Start.position;
        oldLine1End = line1End.position;
        oldLine2Start = line2Start.position;
        oldLine2End = line2End.position;
    }


    private bool LineSegmentsIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, out Vector3 intersection)
    {
        intersection = Vector3.zero;

        Vector3 dir1 = p2 - p1;
        Vector3 dir2 = p4 - p3;

        float denominator = dir1.x * dir2.y - dir1.y * dir2.x;

        // Check if the lines are not parallel
        if (denominator != 0)
        {
            float t = ((p1.x - p3.x) * dir2.y - (p1.y - p3.y) * dir2.x) / denominator;
            float u = ((p1.x - p3.x) * dir1.y - (p1.y - p3.y) * dir1.x) / denominator;

            // Check if the intersection point is within the line segments
            if (t >= 0f && t <= 1f && u >= 0f && u <= 1f)
            {
                intersection = p1 + t * dir1;
                return true;
            }
        }

        return false;
    }


    private void OnDrawGizmos()
    {

        if (line1Start != null && line1End != null && line2Start != null && line2End != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(line1Start.position, line1End.position);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(line2Start.position, line2End.position);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(intersectPoint, 0.2f);
        }

    }


}
