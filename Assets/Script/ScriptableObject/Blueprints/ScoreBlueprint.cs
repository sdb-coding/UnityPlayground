using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "ScoreBlueprint", menuName = "Data/Blueprint/ScoreBlueprint")]
public class ScoreBlueprint : ScriptableObject
{
    [System.Serializable]
    public struct ScoreSettings
    {
        public ScoreTypeLevelValues[] scoreCompinations;
        public int[] cardScores;
    }
    [System.Serializable]
    public struct ScoreTypeLevelValues
    {
        public ScoreType scoreType;
        public ChipMulLevelValues scoreValues;
    }
    [System.Serializable]
    public struct ChipMulLevelValues
    {
        public int currentLevel;
        public LevelValue chipScoreLevel;
        public LevelValue mulScoreLevel;
        public LevelValue cardMulScoreLevel;
    }
    [System.Serializable]
    public struct LevelValue
    {
        public int score;
        public int levelIncrease;
        public int GetScore(int level)
        {
            return score + (level * levelIncrease);
        }
    }
    
    public ScoreSettings scoreSettings;
}
