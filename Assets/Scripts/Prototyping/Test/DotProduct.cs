using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotProduct : MonoBehaviour
{
    [SerializeField] private Transform target = null;
    public float dotValue;
    public float angle;

    private void Update()
    {



        dotValue = transform.position.x * target.position.x + transform.position.y * target.position.y;

        float product = transform.forward.x * target.forward.x + transform.forward.y * target.forward.y;
        float mag = transform.forward.magnitude * target.forward.magnitude;


        angle = Mathf.Acos(product / mag) * Mathf.Rad2Deg;

    }


}