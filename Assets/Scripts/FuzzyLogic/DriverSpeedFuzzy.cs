using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarAI.FuzzySystem;


[System.Serializable]
public struct SpeedAllowance
{
    public float max;
    public float min;
}

[System.Serializable]
public struct DistanceAllowance
{
    public float max;
    public float min;   
}

public enum SpeedAdjust
{
    FloorIt,
    SpeedUp,
    MaintainSpeed,
    SlowDown,
    BrakeHard
}

public class DriverSpeedFuzzy : MonoBehaviour
{
    [Header("Properties Input")]
    [SerializeField, Range(0.0f, 100f)] private float currentSpeed = 50.0f;
    [SerializeField] private SpeedAllowance speedAllowance;
    [Space][SerializeField, Range(0.0f, 50.0f)] private float currentDistance = 20.0f;
    [SerializeField] private DistanceAllowance distanceAllowance;
    [SerializeField] private FuzzinessUtilityData fuzzinessUtilityData;

    [Header("Rating Input")]
    [SerializeField] private DecisionRating distanceRating;
    [SerializeField] private DecisionRating speedRating;

    [Header("Decision Output")]
    [SerializeField] private SpeedAdjust speedAdjustment;


    public bool InitFuzzySystem(DistanceAllowance distAllow, SpeedAllowance speedAllow, FuzzinessUtilityData fuzzyData)
    {
        distanceAllowance = distAllow;
        speedAllowance = speedAllow;
        fuzzinessUtilityData = fuzzyData;

        if(fuzzinessUtilityData != null)
            return true;

        return false;
    }

    public void InitFuzzySystem(DistanceAllowance distAllow, SpeedAllowance speedAllow)
    {
        distanceAllowance = distAllow;
        speedAllowance = speedAllow;
    }


    public void Process(ref SpeedAdjust speedAdjust, float speed, float distance)
    {
        currentSpeed = speed;
        currentDistance = distance;
        Fuzzification(ref distanceRating, ref speedRating, fuzzinessUtilityData.distanceFuzziness, fuzzinessUtilityData.speedFuzziness);
        speedAdjust = speedAdjustment;
    }


    private void Update()
    {
        //Fuzzification(ref distanceRating, ref speedRating, fuzzinessUtilityData.distanceFuzziness, fuzzinessUtilityData.speedFuzziness);
    }
    public void Process(ref SpeedAdjust speedAdjust, DistanceAllowance distAllow, SpeedAllowance speedAllow)
    {
        Fuzzification(ref distanceRating, ref speedRating, fuzzinessUtilityData.distanceFuzziness, fuzzinessUtilityData.speedFuzziness);
        speedAdjust = speedAdjustment;
    }

    private void Fuzzification(ref DecisionRating distanceRate, ref DecisionRating speedRate, FuzzinessDegree distanceFuzziness, FuzzinessDegree speedFuzziness)
    {
        FuzzyRuleSet distanceRuleSet = new FuzzyRuleSet();
        FuzzyRuleSet speedRuleSet = new FuzzyRuleSet();

        float distanceRatio = CustomMath.GetRatio(distanceAllowance.min, distanceAllowance.max, currentDistance);

        //Rating distance based on the average and fuzziness rating Evaluation
        RatingDecision(ref distanceRate, distanceFuzziness, distanceRatio);

        //What does the rating mean
        Inference(ref distanceRuleSet, distanceRate);

        float speedRatio = CustomMath.GetRatio(speedAllowance.min, speedAllowance.max, currentSpeed);


        //Rating Speed based on the average ratio evaluation
        RatingDecision(ref speedRate, speedFuzziness, speedRatio);

        //What does the ratinig means
        Inference(ref speedRuleSet, speedRate);

        //Defuzzification
        Defuzzification(ref speedAdjustment, distanceRuleSet, speedRuleSet);
    }


    /// <summary>
    /// Rates the fuzziness Degree,
    /// Based on current average computed ratio
    /// Updates the Rating Evaluation.
    /// </summary>
    /// <param name="rating"></param>
    /// <param name="fuzzyDegree"></param>
    /// <param name="inputRatio"></param>
    private void RatingDecision(ref DecisionRating rating, FuzzinessDegree fuzzyDegree, float inputRatio)
    {
        rating.high = fuzzyDegree.max.Evaluate(inputRatio);
        rating.average = fuzzyDegree.mid.Evaluate(inputRatio);
        rating.low = fuzzyDegree.min.Evaluate(inputRatio);
    }


    /// <summary>
    /// What does the rating means
    /// How does the System thing of the rating,
    /// Current Could be big, medium or small
    /// updates specified rule.
    /// </summary>
    /// <param name="rule"></param>
    /// <param name="rating"></param>
    private void Inference(ref FuzzyRuleSet rule, DecisionRating rating)
    {
        rule.big = (rating.high > rating.average && rating.high > rating.low);
        rule.medium = (rating.average > rating.high && rating.average > rating.low);
        rule.small = (rating.low > rating.high && rating.low > rating.average);
    }


    /// <summary>
    /// human-like decision needs to be defuizzified for computer to understand true or falsec | 1 or 0
    /// As an Enum type
    /// </summary>
    /// <param name="speedAdjustment"></param>
    /// <param name="disRuleSet"></param>
    /// <param name="speedRuleSet"></param>
    private void Defuzzification(ref SpeedAdjust speedAdjustment, FuzzyRuleSet disRuleSet, FuzzyRuleSet speedRuleSet)
    {
        int? speedAdjustIndex = 0;

        if (disRuleSet.big && speedRuleSet.small) speedAdjustIndex = 0;

        if ((disRuleSet.big && speedRuleSet.medium) || (disRuleSet.medium && speedRuleSet.small)
                                                || (disRuleSet.small && speedRuleSet.small)) speedAdjustIndex = 1;

        if ((disRuleSet.big && speedRuleSet.big) || (disRuleSet.medium && speedRuleSet.medium)) speedAdjustIndex = 2;

        if ((disRuleSet.medium && speedRuleSet.big) || (disRuleSet.small && speedRuleSet.medium)) speedAdjustIndex = 3;
        
        if (disRuleSet.small && speedRuleSet.big) speedAdjustIndex = 4;

        speedAdjustment = (SpeedAdjust)speedAdjustIndex;
    }
}
