using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AngleBtwVectors : MonoBehaviour
{
    public Transform A, B, C;

    public float angle;
    public float dot;

    ///NEW NEW NEW  <summary>
    /// NEW NEW NEW 
    /// </summary>
    public PlayWithCircuit circuit;
    public int nxIndex;
    public float locationsAngle;
    public bool perform;



    private void Update()
    {
        if (perform)
            NXPoints();

        #region Angle
        Vector3 AB = (B.position - A.position).normalized;   
        Vector3 BC = (C.position - B.position).normalized;  
        
        float ABmag = AB.magnitude;
        float BCmag = BC.magnitude;

        float dotABBC = Vector3.Dot(AB, BC);

        angle = (Mathf.Acos(dotABBC / (ABmag * BCmag))) * Mathf.Rad2Deg;


        //all this with above is only check is angle is too big
        AB.Normalize();
        BC.Normalize();

        //Vector3 vecTestC = C.position - testPoint.position;

        //need to create a vector that is perpendicular to AB
        Vector3 perpenVect = Vector3.Cross(AB, Vector3.up);


        float newDot = Vector3.Dot(BC, perpenVect);
        dot = newDot;
        #endregion
    }


    private void NXPoints()
    {
        locationsAngle = CustomMath.NextThreePointIsAPossibleCorner(circuit.nodes, nxIndex);
        perform = false;
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(A.position, B.position);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine (B.position, C.position);

    }






}
