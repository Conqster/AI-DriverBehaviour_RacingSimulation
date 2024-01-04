using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleCalculate : MonoBehaviour
{
    public Transform other;
    public float angle;





    private void Update()
    {
        Vector3 localTarget = transform.InverseTransformPoint(other.position);
        angle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
    }

}
