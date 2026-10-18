using UnityEngine;
using System.Collections.Generic;
using System;

public class AnteModel 
{
    public AnteModel(GameModel gameModel)
    {
        totalScore = new RefEvent<float>();
        currentRoundScore = new RefEvent<float>();
        currentRound = new RefEvent<int>();

        gameModel.nextRoundRequested += NextRoundRequested;
        gameModel.potUpdatedScore += PotUpdatedScore;
    }
    public void SetupAfterLoad()
    {
        RunContext.Current.nextRoundRequested += NextRoundRequested;
        RunContext.Current.potUpdatedScore += PotUpdatedScore;
    }
    public event Action onNextRound;
    public event Action<bool> onTargetScoreReached;
    [SaveState] public Dictionary<string, ScoreModel.ScoringResult> potScores = new();
    [SaveState] public RefEvent<int> currentRound;
    [SaveState] public RefEvent<float> currentRoundScore;
    [SaveState] public RefEvent<float> totalScore;
    bool gameOver;
    [SaveState] public int targetScore;

    public void StartGame(int targetScore)
    {
        this.targetScore = targetScore;
        gameOver = false;
        currentRound.Set(0);
        currentRoundScore.Set(0);
        totalScore.Set(0);
    }
    public void PotUpdatedScore(PotScorePayload potScorePayload)
    {
        if (potScores.ContainsKey(potScorePayload.potGuid))
            potScores[potScorePayload.potGuid] = potScorePayload.currentScore;
        else
            potScores.Add(potScorePayload.potGuid, potScorePayload.currentScore);
        float newCurrentRoundScore = 0;
        foreach (var model in potScores)
        {
            newCurrentRoundScore += model.Value.result;
        }
        currentRoundScore.Set(newCurrentRoundScore);
        if (RunContext.Current.rulesModel.rulesBlueprint.automaticallyScoreAndClearPots)
        {
            NextRoundRequested();
        }
    }
    public void NextRoundRequested()
    {
        if (gameOver)
            return;
        totalScore.Value += currentRoundScore.Value;
        currentRoundScore.Set(0);
        if (totalScore.Value > targetScore)
        {
            TargetScoreReached(true);
            gameOver = true;
        } else if (currentRound == RunContext.Current.rulesModel.GetNumberOfRounds())
        {
            TargetScoreReached(false);
            gameOver = true;
        } else
        {
            currentRound.Value++;
            if (RunContext.Current.rulesModel.rulesBlueprint.automaticallyDrawCardsOnNewRound)
            {
                if (RunContext.Current.nextRoundHappening != null)
                    RunContext.Current.nextRoundHappening();
                RunContext.Current.DrawToFull();
            }
        }
    }
    public void TargetScoreReached(bool didWin)
    {
        if (didWin)
        {
            RunContext.Current.inventoryModel.coins.Value += RunContext.Current.anteScoreBlueprint.GetCoinsForReachingTargetScore(RunContext.Current.currentAnte.Value);
            RunContext.Current.currentAnte.Value++;
            if (RunContext.Current.onAnteComplete != null)
                RunContext.Current.onAnteComplete();
        }
        else
        {
            if (RunContext.Current.onGameOver != null)
                RunContext.Current.onGameOver();
        }
    }
}
