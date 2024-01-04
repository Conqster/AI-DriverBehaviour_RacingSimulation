using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Car;

public class CarGrounded : MonoBehaviour
{
    public CarEngine carEngine;

    public Transform targetHit;
    public Transform objectHit;

    [SerializeField, Range(0.0f, 10.0f)] private float maxTimeOffGround = 5.0f;
    public float timeOffGround = 0.0f;
    public bool needToFlip = false;
    public bool quickFixNotOnGround = false;

    public Transform currentBodyHit;

    private void Start()
    {
        carEngine = GetComponent<CarEngine>();
        targetHit = GameObject.FindGameObjectWithTag("Track").GetComponent<Transform>();
    }


    private void Update()
    {
        CheckWheels();
        quickFixNotOnGround = carEngine.QuickFix();
    }


    private void CheckWheels()
    {
        if (!carEngine.CheckAllWheelsOnGround(ref objectHit, targetHit))
        {
            timeOffGround += Time.deltaTime;
        }
        else
            timeOffGround = 0.0f;


        needToFlip = (timeOffGround > maxTimeOffGround);
           

    }


    private void OnCollisionStay(Collision collision)
    {
        timeOffGround += Time.deltaTime;
        currentBodyHit = collision.transform;
    }

    private void OnCollisionExit(Collision collision)
    {
        //timeOffGround = 0;
    }

}
