using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDirection : MonoBehaviour
{

    public Transform target;
    [SerializeField, Range(0.0f, 10.0f)] private float distance;



    private void OnDrawGizmos()
    {

        Vector3 direction = target.position - transform.position;
        Gizmos.color = Color.yellow;
        float actualDistance = distance / direction.magnitude;
        Gizmos.DrawLine(transform.position, transform.position + (direction * actualDistance));
    }
}
