using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "AnteScoreBlueprint", menuName = "Data/Blueprint/AnteScoreBlueprint")]
public class AnteTargetScoreBlueprint : ScriptableObject
{
    [System.Serializable]
    public struct Round
    {
        public float targetScore;
        public int coinsForReachingTargetScore;
    }
    public Round[] rounds;
    public float constant = 1.6f;
    public float multiplier = 0.75f;
    public float exponent = 0.2f;

    public int coinsAboveRoundCount = 4;

    public int GetTargetScore(int round)
    {
        if (round < 0)
        {
            Debug.LogError("Was requested target score for a negative round number");
            return 0;
        }
        if (round < rounds.Length)
            return  Mathf.FloorToInt(rounds[round].targetScore);

        int roundMinusMaxRounds = round +1 - rounds.Length;
        float calculatedExponent = 1 - (exponent * roundMinusMaxRounds);
        float calculationWithExponent = Mathf.Pow(multiplier * roundMinusMaxRounds, calculatedExponent);
        return Mathf.FloorToInt(((float)rounds[rounds.Length - 1].targetScore) * (constant + Mathf.Pow(calculationWithExponent, roundMinusMaxRounds)));
    }

    public int GetCoinsForReachingTargetScore(int ante)
    {
        if (ante < 0)
        {
            Debug.LogError("Was requested target score for a negative round number");
            return 0;
        }
        if (ante < rounds.Length)
            return rounds[ante].coinsForReachingTargetScore;
        return coinsAboveRoundCount;
    }
}
