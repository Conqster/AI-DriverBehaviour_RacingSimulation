using Car;
using UnityEngine;
using static StateMachine;


[System.Serializable]
public class DriverData
{
    public CarEngine engine;
    public Circuit circuit;
    public Rigidbody rb;
    public ObstacleAvoidance obstacleAvoidance;

    public GameObject brakeLight;

    public int currentWaypointIndex = 0;

    [SerializeField] private float maxTorque = 200f;
    [SerializeField] private float maxBreak = 897f;

    public Transform transform;

    [SerializeField, Range(0.0f, 1.0f)] public float steeringSensitivity = 0.01f;
    [SerializeField, Range(0.0f, 20.0f)] public float visionLength = 9.0f;
    [SerializeField, Range(0.0f, 90.0f)] public float visionAngle = 25.0f;

    [Header("External Information")]
    [SerializeField] public bool start = false;
    [SerializeField] public bool goBerserk = false;
    [SerializeField] public bool testState = false;

    [Header("Debugger")]
    [SerializeField] public float targetAngle;
    [SerializeField] public float currentIntensity;
    [SerializeField] public float rotRatio;
    [SerializeField] public float currentDirection;
    [SerializeField] public float currentSteer;
    [SerializeField] public float normalSteerIntensity;


    public DriverData(CarEngine engine, Circuit circuit, Rigidbody rb, ObstacleAvoidance obstacleAvoidance, GameObject brakeLight)
    {
        this.engine = engine;
        this.circuit = circuit;
        this.rb = rb;
        this.obstacleAvoidance = obstacleAvoidance;
        this.brakeLight = brakeLight;
    }
}

[System.Serializable]
public struct StateMachineData
{
    public string stateName;
    public SM_Event stateEvent;
    public float stateDuration;
}






public class StateMachine
{


    public enum SM_Event
    {
        Enter,
        Update,
        Exit
    }
    


    private StateMachineData sm_stateData;
    protected StateMachine sm_transitTo;  //next state
    protected bool sm_transitionTriggered = false;  //triggered for transition
    protected SM_Event sm_event;   //current event in state 
    protected string sm_name;             // state name 
    protected float sm_duration;
    
    protected DriverData sm_driver;
    
    protected int currentWaypointIndex = 0;
    
    //protected float 
    
    public StateMachine(DriverData driver)
    {
        sm_name = "Base State";
        sm_event = SM_Event.Enter;
        sm_driver = driver;
        sm_duration = 0.0f;
    }
    
    
    public StateMachine Process()
    {
    
        switch (sm_event)
        {
            case SM_Event.Enter:
                Enter();
                break;
            case SM_Event.Update:
                Update();
                break;
            case SM_Event.Exit:
                Debug.Log("Prepare to exit");
                Exit();
                return sm_transitTo;
        }

        sm_stateData.stateName = sm_name;
        sm_stateData.stateDuration = sm_duration;
        sm_stateData.stateEvent = sm_event;
        return this;
    }
    
    
    /// <summary>
    /// special logic of thing to do -
    /// when enter new state, 
    /// if some parameter needs to be checked - configurations
    /// </summary>
    protected virtual void Enter()
    {
        sm_duration = 0.0f;
        //after all logic as be performed change state event to update
        sm_event = SM_Event.Update;
    }
    
    /// <summary>
    /// what specific state do on it event update every frame
    /// </summary>
    protected virtual void Update()
    {
        sm_duration += Time.deltaTime;

        //if(sm_driver.testState)
        //{
        //    StateMachine testState = new SM_TestState(sm_driver);

        //    Debug.Log(testState.GetSM_Name());

        //    if (GetStateMachine() != testState)
        //        TriggerExit(testState);
        //}

    
        if (sm_transitionTriggered)
            return;
    
        sm_event = SM_Event.Update;
    }
    
    
    /// <summary>
    /// what state should do before transiting to next state 
    /// </summary>
    protected virtual void Exit()
    {
    
    
        sm_event = SM_Event.Exit;
    }
    
    
    protected virtual void TriggerExit(StateMachine transition)
    {
        sm_transitTo = transition;
        sm_transitionTriggered = true;
        sm_event = SM_Event.Exit;
    }
    
    
    public string GetSM_Name() { return sm_name; }
    public SM_Event GetSM_Event() { return sm_event; }

    public StateMachineData GetStateMachineData() { return sm_stateData; }

    private StateMachine GetStateMachine() { return this; }
    
}

