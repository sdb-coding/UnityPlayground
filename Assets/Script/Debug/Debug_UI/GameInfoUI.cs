using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class GameInfoUI : MonoBehaviour
{
    public TextMeshProUGUI anteCount;
    public TextMeshProUGUI maxAntes;

    public GameRunner gameRunner;
    public GameModel DisplayedGameModel { get; private set; }

    private void Start()
    {
        if (gameRunner == null)
        {
            gameRunner = FindAnyObjectByType<GameRunner>();
        }
        if (!gameRunner.gameModel.isNull)
            DisplayGameModel(gameRunner.gameModel);
        gameRunner.gameModel.onChange += DisplayGameModel;
    }
    void ResetInfoUI()
    {
        if (DisplayedGameModel != null)
        {
            DisplayedGameModel.currentAnte.onChange -= RoundChanged;
        }
    }
    public void DisplayGameModel(RefEvent<GameModel> gameModel)
    {
        ResetInfoUI();
        DisplayedGameModel = gameModel.Value;

        DisplayedGameModel.currentAnte.onChange += RoundChanged;
        RoundChanged(DisplayedGameModel.currentAnte);

    }

    void RoundChanged(RefEvent<int> newCurrentRound)
    {
        anteCount.text = (newCurrentRound.Value + 1).ToString();
        if (newCurrentRound > DisplayedGameModel.anteScoreBlueprint.rounds.Length)
            maxAntes.text = "-";
        else
            maxAntes.text = DisplayedGameModel.anteScoreBlueprint.rounds.Length.ToString();
    }
}
