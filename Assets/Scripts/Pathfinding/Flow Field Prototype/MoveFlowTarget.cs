using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFlowTarget : MonoBehaviour
{
    [SerializeField, Range(0.0f, 10.0f)] private float speed = 2.0f;
    public Vector3 moveVector = Vector3.zero;



    private void Update()
    {
        moveVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));


        transform.position += moveVector * speed * Time.deltaTime;    
    }


}
