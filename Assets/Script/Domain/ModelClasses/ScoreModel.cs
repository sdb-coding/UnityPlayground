using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;
using Newtonsoft.Json;

public class ScoreModel
{
    [System.Serializable]
    public struct ScoringType
    {
        public ScoreType scoreType;
        [SaveState] public List<string> scoringCards;
        [JsonIgnore] PlayingCardModel[] cachedScoringCards;
        [JsonIgnore]
        public PlayingCardModel[] CachedScoringCards
        {
            get
            {
                if (cachedScoringCards == null)
                {
                    cachedScoringCards = CardRegistryModel.GetModels<PlayingCardModel>(scoringCards);
                }
                return cachedScoringCards;
            }
        }
        public ScoringType(ScoreType scoreType, List<PlayingCardModel> scoringCards)
        {
            this.scoreType = scoreType;
            this.cachedScoringCards = scoringCards.ToArray();
            this.scoringCards = scoringCards.GetGUIDs();
        }
        public ScoringType(ScoreType scoreType, PlayingCardModel[] scoringCards) : this()
        {
            this.scoreType = scoreType;
            this.scoringCards = scoringCards.GetGUIDs();
            cachedScoringCards = scoringCards;
        }
        public string ToLog(ScoreModel scoreModel)
        {
            StringBuilder log = new StringBuilder(scoreType.ToString());
            log.Append(" ");
            foreach (var item in scoringCards)
            {
                var model = CardRegistryModel.GetModel<PlayingCardModel>(item);
                log.Append($"{model.GetNiceName()} {model.GetScoringValue(scoreModel)}-");
            }
            return log.ToString();
        }
    }
    [System.Serializable]
    public class ScoringResult
    {
        public ScoringResult() { }
        public ScoringResult(ScoreType scoreType)
        {
            scoringType = new ScoringType() { scoreType = scoreType };
        }
        public ScoringResult(ScoreBlueprint.ChipMulLevelValues scoreValues, ScoreType scoreType, PlayingCardModel[] scoringCards)
        {
            scoringType = new ScoringType(scoreType, scoringCards);
            this.scoreValues = scoreValues;
        }
        public ScoringResult(ScoreBlueprint.ChipMulLevelValues scoreValues, ScoringType scoringType)
        {
            this.scoringType = scoringType;
            this.scoreValues = scoreValues;
        }
        public ScoreBlueprint.ChipMulLevelValues scoreValues;
        public ScoringType scoringType;
        public int result;
        public int currentLevel => scoreValues.currentLevel;
        public int TotalChipScore
        {
            get { return chipScore + scoreValues.chipScoreLevel.GetScore(currentLevel); }
        }
        public int TotalMulScore
        {
            get
            { return mulScore + scoreValues.mulScoreLevel.GetScore(currentLevel); }
        }
        public int TotalCardMulScore
        {
            get
            { return cardMulScore + scoreValues.cardMulScoreLevel.GetScore(currentLevel); }
        }
        public int chipScore;
        public int mulScore;
        public int cardMulScore;
        public void Reset()
        {
            chipScore = 0;
            mulScore = 0;
            cardMulScore = 0;
        }
        public string ToLog(ScoreModel scoreModel)
        {
            return $"{scoringType.ToLog(scoreModel)}: {result} - {TotalChipScore} x {TotalMulScore}";
        }
    }

    public ScoreModel(ScoreBlueprint.ScoreSettings scoreSettings)
    {
        this.scoreSettings = scoreSettings;
    }
    public event Action<ScoreType, ScoreBlueprint.ChipMulLevelValues> onLevelUp;
    ScoreBlueprint.ScoreSettings scoreSettings;
    public ScoringResult GetCurrentScore(ScoringType scoringType)
    {
        foreach (var item in scoreSettings.scoreCompinations)
        {
            if (item.scoreType == scoringType.scoreType)
                return new ScoringResult(item.scoreValues, scoringType);
        }
        Debug.LogError($"Couldn't find a score for the scoretype: {scoringType.scoreType}");
        return default;
    }
    public ScoreBlueprint.ChipMulLevelValues GetBaseScore(ScoreType scoreType)
    {
        foreach (var item in scoreSettings.scoreCompinations)
        {
            if (item.scoreType == scoreType)
            {
                return item.scoreValues;
            }
        }
        Debug.LogError($"Couldn't find a score for the scoretype: {scoreType}");
        return default;
    }
    public int GetBaseCardValue(int cardRank)
    {
        return scoreSettings.cardScores[cardRank-1];
    }
    public void IncreaseLevel(ScoreType scoreType)
    {
        for (int i = 0; i < scoreSettings.scoreCompinations.Length; i++)
        {
            if (scoreSettings.scoreCompinations[i].scoreType == scoreType)
            {
                scoreSettings.scoreCompinations[i].scoreValues.currentLevel++;
                if (onLevelUp != null)
                    onLevelUp(scoreType , scoreSettings.scoreCompinations[i].scoreValues);
            }
        }
    }
}
