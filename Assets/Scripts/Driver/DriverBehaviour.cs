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

    private void Start()
    {
        engine = GetComponent<CarEngine>();
        circuit = GameObject.FindGameObjectWithTag("Circuit").GetComponent<Circuit>();
        rb = GetComponent<Rigidbody>();
        driverData = new DriverData(engine, driverStateInfo, circuit, rb, transform);
        driverData.canUseBlock = canBlock;
        driverSM = new SM_DefaultState(driverData);
    }

    private void FixedUpdate()
    {
        //OldScript();
        driverSM = driverSM.Process();
        driverSMData = driverSM.GetStateMachineData();
        //Time.timeScale = 0.5f;
    }



    //private void Update()
    //{
    //    //OldScript();
    //    driverSM = driverSM.Process();
    //    driverSMData = driverSM.GetStateMachineData();
    //    //Time.timeScale = 0.5f;
    //}
}
