using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StateData
{
    [Header("Sensitivity")]
    public float steeringSensitivity;

    [Header("Perception")]
    public float visionLength;
    public float visionAngle;

    [Header("Extra")]
    public float startRaceIn;
    public float specialUtilityMeter;
    public float coolDown1;
}


[CreateAssetMenu(fileName = "StateData", menuName = "ScriptableObjects/Driver Information", order = 1)]
public class DriverStateInformation : ScriptableObject
{
    [SerializeField] private StateData defaultStateData;
    [SerializeField] private StateData NormalStateData;
    [SerializeField] private StateData OvertakingStateData;

    public StateData DefaultStateData {  get { return defaultStateData; } } 
    public StateData NS_Data {  get { return NormalStateData; } }
    public StateData OS_Data { get { return OvertakingStateData; } }


}
