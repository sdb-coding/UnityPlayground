using UnityEngine;
using System.Collections.Generic;
using System;

public static class ScoreCalculator
{
    //public class ScoreResult
    //{
    //    public ScoreResult(ScoreType scoreType)
    //    {
    //        this.scoreType = scoreType;
    //    }
    //    public ScoreType scoreType;
    //    public int chips;
    //    public int mul;
    //    public int result;

    //    public string ToLog()
    //    {
    //        return $"{scoreType}: {result} from: {chips} x {mul}";
    //    }
    //}
    public static ScoreModel.ScoringResult ScorePot(List<PlayingCardModel> cardModels,RulesModel rulesModel, ScoreModel scoreModel)
    {
        var potType = PotValidater.ValidatePot(cardModels, rulesModel);
        return ScorePot(cardModels, rulesModel, scoreModel, scoreModel.GetCurrentScore(potType));
    }

    public static ScoreModel.ScoringResult ScorePot(List<PlayingCardModel> cardModels, RulesModel rulesModel, ScoreModel scoreModel, ScoreModel.ScoringResult currentScore)
    {
        rulesModel.OnBeforeScoring(currentScore);
        rulesModel.OnScoring(currentScore);

        foreach (var cardModel in currentScore.scoringType.CachedScoringCards)
        {
            currentScore.chipScore += cardModel.GetScoringValue(scoreModel);
        }
        currentScore.result = currentScore.TotalChipScore * (currentScore.TotalMulScore + (currentScore.TotalCardMulScore * cardModels.Count));
        
        rulesModel.OnAfterScoring(currentScore);
        return currentScore;
    }
}
