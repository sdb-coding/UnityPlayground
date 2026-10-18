using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class RoundInfoUI : MonoBehaviour
{
    public GameRunner gameRunner;
    public TextMeshProUGUI roundsLeftValue;

    public AnteModel DisplayedAnteModel { get; private set; }

    private void Start()
    {
        if (gameRunner == null)
        {
            gameRunner = FindAnyObjectByType<GameRunner>();
        }
        if (!gameRunner.gameModel.isNull)
            AnteRunnerHaveNewAnteModel(gameRunner.gameModel);
        gameRunner.gameModel.onChange += AnteRunnerHaveNewAnteModel;
    }
    void ResetInfoUI()
    {
        if (DisplayedAnteModel != null)
        {
            DisplayedAnteModel.currentRound.onChange -= CurrentRoundChanged;
        }
    }
    public void AnteRunnerHaveNewAnteModel(RefEvent<GameModel> gameModel)
    {
        ResetInfoUI();
        DisplayedAnteModel = gameModel.Value.anteModel;
        CurrentRoundChanged(DisplayedAnteModel.currentRound);
        DisplayedAnteModel.currentRound.onChange += CurrentRoundChanged;
    }
    void CurrentRoundChanged(RefEvent<int> currentRound)
    {
        roundsLeftValue.text = (RunContext.Current.rulesModel.GetNumberOfRounds() - currentRound.Value).ToString();
    }
}
