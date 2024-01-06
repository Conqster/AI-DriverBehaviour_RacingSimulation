using UnityEngine;

public class Spline : MonoBehaviour
{
    public int numberOfPoints = 50; // Number of points on the spline
    public Vector3[] controlPoints; // Bezier control points

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numberOfPoints;
        DrawSpline();
    }

    void DrawSpline()
    {
        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = (float)i / (numberOfPoints - 1);
            Vector3 point = CalculateBezierPoint(t, controlPoints[0], controlPoints[1], controlPoints[2], controlPoints[3]);
            lineRenderer.SetPosition(i, point);
        }
    }

    // Bezier curve calculation
    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point =
            uuu * p0 +
            3 * uu * t * p1 +
            3 * u * tt * p2 +
            ttt * p3;

        return point;
    }
}
