using UnityEngine;
using CarAI.Vehicle;
using CarAI.Pathfinding;


[RequireComponent(typeof(CarEngine))]
public class DriverBehaviour : MonoBehaviour
{
    private CarEngine engine;
    private Circuit circuit;
    private Rigidbody rb;


    private StateMachine driverSM;
    public DriverStateInformation driverStateInfo;
    public StateMachineData driverSMData;


    public DriverData driverData;


    [Space]
    public float currentSteer;
    public bool canBlock = false;

    public bool useFixedUpdate = false;
    public SimManager simManager;
    private Camera driverCamera;

    public Camera usingCamera { get {  return driverCamera; } }  

    private void Start()
    {
        engine = GetComponent<CarEngine>();
        circuit = GameObject.FindGameObjectWithTag("Circuit").GetComponent<Circuit>();
        rb = GetComponent<Rigidbody>();
        driverData = new DriverData(engine, driverStateInfo, circuit, rb, transform);
        driverData.canUseBlock = canBlock;
        driverSM = new SM_DefaultState(driverData);

        driverCamera = GetComponentInChildren<Camera>();    
    }


    private void Update()
    {

        driverSM = driverSM.Process();
        driverSMData = driverSM.GetStateMachineData();
        //OldScript();
        //useFixedUpdate = (simManager.CurrentFPS < 30) ? true : false;

        //if (!useFixedUpdate)
        //{
        //    driverSM = driverSM.Process();
        //    driverSMData = driverSM.GetStateMachineData();
        //}
        //Time.timeScale = 0.5f;
    }

    private void FixedUpdate()
    {
        //OldScript();


        //if(useFixedUpdate)
        //{
        //    driverSM = driverSM.Process();
        //    driverSMData = driverSM.GetStateMachineData();
        //}

        //Time.timeScale = 0.5f;
    }



}
