using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public enum SimState
{
    Neutral,
    Ready,
    Running,
    Pause
}


public class SimManager : MonoBehaviour
{
    [Header("Properties")]
    private List<Camera> driversCameras = new List<Camera>();
    private List<DriverBehaviour> driversBehaviour = new List<DriverBehaviour>();

    [Header("Simulation State")]
    [SerializeField] private SimState simState = SimState.Ready;

    //private string buttonText;
    [Header("External Info")]
    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI fpsText;
    public TextMeshProUGUI extraInfo;
    public TextMeshProUGUI updateDriverButton;
    public Slider numOfDriverSlider;
    public Transform updateSystemTransform;
    public TextMeshProUGUI currentCamInfo;
    public TextMeshProUGUI warningInfo;
    public Transform credits;
    public TextMeshProUGUI creditButton;
    private bool creditButtonPressed = false;
    
    

    [Header("Input")]
    [SerializeField, Range(0, 7)] private int cameraIndex = 0;
    public int currentCamIndex = 0;
    public int value;
    public KeyCode keyValue;
    public int inputValue;
    public int cameraCount;

    [Header("Info")]
    [SerializeField] private float timeScale;
    [SerializeField] private float frameRate;

    private float updateInterval = 0.5f;
    private float accum = 0f;
    private int frames = 0;
    private float timeLeft;
    public float numOfDrivers;

    private Camera[] trackCamera;
    private DriverBehaviour[] trackDriverBehaviour;



    public float CurrentFPS { get { return frameRate; } }

    private void Start()
    {
        GetAllCameras();
        InitDrivers();

        timeLeft = updateInterval;

#if UNITY_2022_3_OR_NEWER
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 220;
#endif
    }

    private void Update()
    {
        

        if(InputValue(ref inputValue))
        {
            if(inputValue < driversCameras.Count + 1)
                ChangeCamera2(inputValue - 1);
        }



    }

    private void LateUpdate()
    {
        ComputeFrameRate(ref frameRate);
        fpsText.text = "Frame Rate: " + frameRate.ToString("00.0") + " FPS.";


        if(simState == SimState.Neutral)
        {
            numOfDrivers = numOfDriverSlider.value;
            extraInfo.text = "Press Keys: 1 to " + numOfDrivers.ToString("0") + " to switch cameras between drivers.";  //call in start 
            updateDriverButton.text = "Update Numbers of Drivers: " + numOfDrivers.ToString("0") + ".";
            warningInfo.text = "Set the number of drivers: " + numOfDrivers.ToString("0") + ". min:2, max:7";
        }

        if(simState == SimState.Ready)
        {
            if(numOfDrivers != numOfDriverSlider.value)
            {
                driversCameras = new List<Camera>(trackCamera);
                driversBehaviour = new List<DriverBehaviour>(trackDriverBehaviour);

                foreach(DriverBehaviour driver in driversBehaviour)
                    driver.gameObject.SetActive(true);

                simState = SimState.Neutral;
            }
            warningInfo.text = "Ready to start simulation.";
        }


        creditButton.transform.parent.gameObject.SetActive((simState == SimState.Neutral) ? true : false);

        

    }


    public void UpdateDrivers()
    {

        GameObject[] drivers = GameObject.FindGameObjectsWithTag("Vehicle");

        foreach (GameObject driver in drivers)
            driver.SetActive(false);

        trackCamera = driversCameras.ToArray();
        trackDriverBehaviour = driversBehaviour.ToArray();

        driversBehaviour.Clear();
        driversCameras.Clear();

        for (int i = 0; i < numOfDrivers; i++)
        {
            driversBehaviour.Add(drivers[i].GetComponent<DriverBehaviour>());
            driversCameras.Add(driversBehaviour[i].usingCamera);
            drivers[i].SetActive(true);
        }
        

        simState = SimState.Ready;
    }



    private void GetAllCameras()
    {
        GameObject[] gameCamera = GameObject.FindGameObjectsWithTag("DriverCamera");

        foreach(GameObject gameObject in gameCamera)
        {
            driversCameras.Add(gameObject.GetComponent<Camera>());
        }
    }

    private void InitDrivers()
    {
        GameObject[] drivers = GameObject.FindGameObjectsWithTag("Vehicle");

        foreach (GameObject gameObject in drivers)
        {
            driversBehaviour.Add(gameObject.GetComponent<DriverBehaviour>());
        }
    }


    private void StartRace()
    {
        foreach(var drivers in driversBehaviour)
            drivers.driverData.canStartRace = true;

        buttonText.text = "Pause Sim";
        warningInfo.text = "";

        updateSystemTransform.gameObject.SetActive(false);

        simState = SimState.Running;
    }

    private void PauseRace()
    {
        Time.timeScale = 0;
        buttonText.text = "Resume Sim";
        warningInfo.text = "Resume Simulation";
        simState = SimState.Pause;
    }

    private void ResumeRace()
    {
        Time.timeScale = 1;
        buttonText.text = "Pause Sim";
        warningInfo.text = "";
        simState = SimState.Running;
    }


    public void ChangeState()
    {
        switch (simState)
        {
            case SimState.Ready:

                StartRace();

                break;
            case SimState.Running:

                PauseRace();  
                
                break;
            case SimState.Pause:

                ResumeRace();

                break;
            default:

                warningInfo.text = "Set the number of drivers to start simulation";

                break;
        }
    }

    public void ToggleCredits()
    {
        creditButtonPressed = !creditButtonPressed;
        credits.gameObject.SetActive(creditButtonPressed);

        creditButton.text = (creditButtonPressed) ? "Back" : "Credits";    

        updateSystemTransform.gameObject.SetActive(!creditButtonPressed);
        warningInfo.gameObject.SetActive(!creditButtonPressed);
    }

    public void QuitSimulation()
    {
        Application.Quit(); 
    }


    public void RestartSimulation()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }

    private void ChangeCamera2(int to)
    {
        driversCameras[currentCamIndex].enabled = false;

        driversCameras[to].enabled = true;
        currentCamIndex = to;

        currentCamInfo.text = "Current Cam: " + (to + 1).ToString("0") + ".";

    }

    private bool InputValue(ref int inputValue)
    {

        if (Input.anyKeyDown)   
        {
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    value = (int)keyCode;
                    keyValue = keyCode;

                    

                    switch(keyValue)
                    {
                        case KeyCode.Alpha1:
                            inputValue = 1;
                            return true;
                        case KeyCode.Alpha2:
                            inputValue = 2;
                            return true;
                        case KeyCode.Alpha3:
                            inputValue = 3;
                            return true;
                        case KeyCode.Alpha4:
                            inputValue = 4;
                            return true;
                        case KeyCode.Alpha5:
                            inputValue = 5;
                            return true;
                        case KeyCode.Alpha6:
                            inputValue = 6;
                            return true;
                        case KeyCode.Alpha7:
                            inputValue = 7;
                            return true;
                    }
       
                }
            }
        }

        return false;
    }


    private void ComputeFrameRate(ref float fps)
    {
        timeLeft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        if (timeLeft <= 0.0f)
        {
            float frameRate = accum / frames;
            fps = frameRate;
            timeLeft = updateInterval;
            accum = 0f;
            frames = 0;
        }
    }
}
