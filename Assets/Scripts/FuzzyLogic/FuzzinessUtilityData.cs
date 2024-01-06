using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CarAI.FuzzySystem
{
    [System.Serializable]
    public struct FuzzinessDegree
    {
        public AnimationCurve min;
        public AnimationCurve mid;
        public AnimationCurve max;
    }

    [System.Serializable]
    public struct DecisionRating
    {
        public float high;
        public float average;
        public float low;
    }


    public struct FuzzyRuleSet
    {
        public bool big;
        public bool medium;
        public bool small;
    }




    [CreateAssetMenu(fileName = "Distance-Speed Degree Data", menuName = "ScriptableObjects/Distance&Speed Utility", order = 1)]
    public class FuzzinessUtilityData : ScriptableObject
    {
        public FuzzinessDegree distanceFuzziness;
        public FuzzinessDegree speedFuzziness;
    }
}


