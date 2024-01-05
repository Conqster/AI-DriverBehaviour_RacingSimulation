using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuzzinessUtilityDataCollection : MonoBehaviour
{
    [Header("Utility Data")]
    [SerializeField] private FuzzinessUtilityData FSM_NS_FuzzyUtilityData;
    [SerializeField] private FuzzinessUtilityData FSM_OS_FuzzyUtilityData;


    [Header("Normal State")]
    [SerializeField] private SpeedAllowance FSM_NS_SpeedAllowance;
    [SerializeField] private DistanceAllowance FSM_NS_DistanceAllowance;

    [Header("Overtaking State")]
    [SerializeField] private SpeedAllowance FSM_OS_SpeedAllowance;
    [SerializeField] private DistanceAllowance FSM_OS_DistanceAllowance;

    public FuzzinessUtilityData NS_FuzzyUtilityData { get { return FSM_NS_FuzzyUtilityData; } }
    public FuzzinessUtilityData OS_FuzzyUtilityData { get { return FSM_OS_FuzzyUtilityData; } }

    public SpeedAllowance NS_SpeedAllowance {  get { return FSM_NS_SpeedAllowance; } }
    public DistanceAllowance NS_DistanceAllowance { get { return FSM_NS_DistanceAllowance; } }

    public SpeedAllowance OS_SpeedAllowance { get { return FSM_OS_SpeedAllowance;} }
    public DistanceAllowance OS_DistanceAllowance { get { return FSM_OS_DistanceAllowance;} }

}
