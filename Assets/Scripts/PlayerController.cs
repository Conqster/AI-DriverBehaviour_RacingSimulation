using CarAI.Vehicle;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Transform camera;
    public InputAction IA_CameraCtrl;
    public InputAction IA_AcclerateCtrl;
    public InputAction IA_SteeringCtrl;
    public InputAction IA_BrakeCtrl;
    public CarEngine carEngine;

    [Header("Input Debugger")]
    public Vector2 move;
    public float throttle;
    public float steering;
    public float brake;



    private void Start()
    {
        IA_CameraCtrl.Enable();
        IA_AcclerateCtrl.Enable();
        IA_SteeringCtrl.Enable();
        IA_BrakeCtrl.Enable();
        carEngine = GetComponent<CarEngine>();
    }

    private void Update()
    {
        move = IA_CameraCtrl.ReadValue<Vector2>();
        throttle = IA_AcclerateCtrl.ReadValue<float>();
        steering = IA_SteeringCtrl.ReadValue<float>();
        steering = Mathf.Clamp(steering, -1.0f, 1.0f);
        brake = IA_BrakeCtrl.ReadValue<float>();
        brake = Mathf.Clamp01(brake);

        RotateCamera();


        carEngine.Move(throttle, brake, steering);

        //TO-DO: if no inputs for 10 seconds reset camera
    }


    private void RotateCamera()
    {
        //camera.RotateAround(transform.position, Vector3.up, 50.0f * horValue * Time.deltaTime);

        //Vector3 moveAxis = Vector2.zero;
        //if (Mathf.Abs(move.x) > 0) moveAxis = Vector3.up;
        //else if (Mathf.Abs(move.y) > 0) moveAxis = Vector3.right;

        camera.RotateAround(transform.position, (new Vector2(move.y, move.x)).normalized, move.magnitude * 50.0f * Time.deltaTime);
    }
}
