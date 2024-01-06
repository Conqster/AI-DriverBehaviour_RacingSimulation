using CarAI.Pathfinding;
using UnityEngine;
using TMPro;

public class AIController : MonoBehaviour
{
    private Drive[] drives;
    private Circuit circuit;
    [SerializeField, Range(0.0f, 1.0f)] private float steeringSensitivity = 0.01f;
    private Vector3 target;
    private Rigidbody rb;
    public GameObject brakeLight;
    int currentWaypointIndex = 0;

    public bool trying = false;
    public TextMeshProUGUI displaySpeed;
    public TextMeshProUGUI displayTimeScale;
    [SerializeField, Range(100, 1000)] private float maxTorque = 200f;
    [SerializeField, Range(100, 5000)] private float maxBreak = 500f;


    bool slowTime = false;

    private void Start()
    {
        drives = GetComponentsInChildren<Drive>();
        circuit = GameObject.FindGameObjectWithTag("Circuit").GetComponent<Circuit>();
        target = circuit.waypoints[currentWaypointIndex].position;
        rb = GetComponent<Rigidbody>();


        foreach (Drive drive in drives)
        {
            drive.UpdateParameters(maxTorque, maxBreak);
        }
    }


    private void Update()
    {
        

        if(Input.GetKeyDown(KeyCode.P)) slowTime = !slowTime;

        Time.timeScale = (slowTime)? 0.5f : 1.0f;   
        


        Vector3 localTarget = transform.InverseTransformPoint(target);
        float distanceToTarget = Vector3.Distance(target, transform.position); 
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;


        float accelerate = 1f;
        float brake = 0.0f;
        float steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(rb.velocity.magnitude);


        if(distanceToTarget < 15 && rb.velocity.magnitude > 15.0f)
        {
            brake = (15 - distanceToTarget/15);
            //brake = 0.5f;
        }

        //SPEED DANGER NEED TO BREAK 
        if(rb.velocity.magnitude > 25.0f && distanceToTarget < 45.0f && distanceToTarget > 15.0f)
        {
            brake = 1.0f;
        }


        if(brake > 0.0f)
        {
            brakeLight.SetActive(true);
        }
        else
        {
            brakeLight.SetActive(false);
        }



        foreach(Drive wheel in drives)
        {
            wheel.ApplyRotation(accelerate, steer, brake);
        }


        if(distanceToTarget < 10.0f)
        {
            currentWaypointIndex++;
            if(currentWaypointIndex >= circuit.waypoints.Count)
                currentWaypointIndex = 0;

            target = circuit.waypoints[currentWaypointIndex].position;
        }

    }

    private void LateUpdate()
    {
            
        if(displaySpeed)
        {
            displaySpeed.text = "Speed: " + rb.velocity.magnitude.ToString("0.0");
            displayTimeScale.text = "TimeScale: " + Time.timeScale.ToString("0.0");
        }
    }


}
