using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;

public class AnteInfoUI : MonoBehaviour
{
    public GameRunner gameRunner;
    public TextMeshProUGUI targetScore;
    public TextMeshProUGUI currentRoundScore;
    public TextMeshProUGUI totalScoreText;
    public AnteModel DisplayedAnteModel { get; private set; }

    private void Start()
    {
        if (gameRunner == null)
        {
            gameRunner = FindAnyObjectByType<GameRunner>();
        }
        if (!gameRunner.gameModel.isNull)
            DisplayAnteModel(gameRunner.gameModel);
        gameRunner.gameModel.onChange += DisplayAnteModel;
    }
    void ResetInfoUI()
    {
        if (DisplayedAnteModel != null)
        {
            DisplayedAnteModel.totalScore.onChange -= TotalScoreChanged;
            DisplayedAnteModel.currentRoundScore.onChange -= CurrentRoundScoreChanged;
        }
    }
    public void DisplayAnteModel(RefEvent<GameModel> gameModel)
    {
        ResetInfoUI();
        DisplayedAnteModel = gameModel.Value.anteModel;

        TotalScoreChanged(DisplayedAnteModel.totalScore);
        CurrentRoundScoreChanged(DisplayedAnteModel.currentRoundScore);
        
        targetScore.text = DisplayedAnteModel.targetScore.ToString();

        DisplayedAnteModel.totalScore.onChange += TotalScoreChanged;
        DisplayedAnteModel.currentRoundScore.onChange += CurrentRoundScoreChanged;
    }

    void CurrentRoundScoreChanged(RefEvent<float> currentScore)
    {
        currentRoundScore.text = currentScore.Value.ToString();
    }
    void TotalScoreChanged(RefEvent<float> totalScore)
    {
        totalScoreText.text = totalScore.Value.ToString();
    }

}
