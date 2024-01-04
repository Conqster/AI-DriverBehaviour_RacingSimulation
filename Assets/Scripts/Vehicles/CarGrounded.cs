using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Car;

public class CarGrounded : MonoBehaviour
{
    private CarEngine carEngine;


    [SerializeField, Range(0.0f, 10.0f)] private float maxTimeOffGround = 5.0f;
    [SerializeField, Range(0.0f, 10.0f)] private float maxTimeStuck = 6.0f;
    public float timeOffGround = 0.0f;
    public float timeStuck = 0.0f;  
    private bool needToFlip = false;

    private bool isGrounded = false;

    public bool carIsRunning = false;  

    public bool NeedToFlip
    {
        get { return needToFlip; }
    }

    private Rigidbody rb;

    private void Start()
    {
        carEngine = GetComponent<CarEngine>();
        rb = GetComponent<Rigidbody>();
    }


    private void Update()
    {
        carIsRunning = carEngine.Running;

        isGrounded = carEngine.WheelOnGround();

        if(carIsRunning)
        {

            if (!isGrounded)
            {
                timeOffGround += Time.deltaTime;
            }
            else
                timeOffGround = 0.0f;

            if (rb.velocity.magnitude < 0.1f)
                timeStuck += Time.deltaTime;
            else
                timeStuck = 0.0f;

        }


        needToFlip = (timeOffGround > maxTimeOffGround || timeStuck > maxTimeStuck);

    }


    public void Flip(Vector3 targetAhead)
    {
        timeOffGround = 0.0f;
        timeStuck = 0.0f;

        float distToTarget = Vector3.Distance(transform.position, targetAhead); 

        transform.position = new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z);

        transform.LookAt(targetAhead);
    }

}
