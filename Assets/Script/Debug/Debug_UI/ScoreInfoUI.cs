using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ScoreInfoUI : MonoBehaviour
{
    struct ScoreTextCombinations
    {
        public ScoreType scoreType;
        public TextMeshProUGUI textText;
        public TextMeshProUGUI chipValue;
        public TextMeshProUGUI mulValue;
        public TextMeshProUGUI cardMulValue;

        public void UpdateScore(ScoreBlueprint.ChipMulLevelValues scoreValues)
        {
            chipValue.text = scoreValues.chipScoreLevel.ToString();
            mulValue.text = scoreValues.mulScoreLevel.ToString();
            cardMulValue.text = scoreValues.cardMulScoreLevel.ToString();
        }
        public ScoreTextCombinations(Transform scoreTypeInfoPrefab, ScoreType scoreType, ScoreBlueprint.ChipMulLevelValues scoreValues)
        {
            textText = scoreTypeInfoPrefab.Find("Text&CardMul/Header").GetComponent<TextMeshProUGUI>();
            textText.text = scoreType.ToString();
            cardMulValue = scoreTypeInfoPrefab.Find("Text&CardMul/CardMul").GetComponent<TextMeshProUGUI>();
            var cardMulScore = scoreValues.cardMulScoreLevel.GetScore(scoreValues.currentLevel);
            cardMulValue.text = cardMulScore == 0 ? "" : $"x {cardMulScore.ToString()}";
            chipValue = scoreTypeInfoPrefab.Find("Chip&Mul/Chip").GetComponent<TextMeshProUGUI>();
            chipValue.text = scoreValues.chipScoreLevel.GetScore(scoreValues.currentLevel).ToString();
            mulValue = scoreTypeInfoPrefab.Find("Chip&Mul/Mul").GetComponent<TextMeshProUGUI>();
            mulValue.text = scoreValues.mulScoreLevel.GetScore(scoreValues.currentLevel).ToString();
            this.scoreType = scoreType;
        }
    }
    [Header("Scene References")]
    [Header("UI settings")]
    public Transform scoreTypeInfoPrefab;
    public Transform Container;

    public GameRunner gameRunner;
    public ScoreModel DisplayedScoreModel { get; private set; }

    ScoreTextCombinations[] scores;
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
        if (DisplayedScoreModel != null)
        {
            DisplayedScoreModel.onLevelUp -= UpdateScoreText;
        }
    }
    public void DisplayGameModel(RefEvent<GameModel> gameModel)
    {
        ResetInfoUI();
        if (gameModel.Value.scoreModel != null)
        {
            DisplayedScoreModel = gameModel.Value.scoreModel;
            SetupText();
            DisplayedScoreModel.onLevelUp += UpdateScoreText;
        }
    }
    void SetupText()
    {
        var values = System.Enum.GetValues(typeof(ScoreType));
        scores = new ScoreTextCombinations[values.Length];
        int index = 0;
        foreach (ScoreType scoreType in values)
        {
            if (scoreType != ScoreType.Nothing && scoreType != ScoreType.Unknown)
            {
                Transform newSpawn = Instantiate(scoreTypeInfoPrefab, Container);
                scores[index] = new ScoreTextCombinations(newSpawn, scoreType, DisplayedScoreModel.GetBaseScore(scoreType));
                index++;
            }
        }
    }
    void UpdateScoreText(ScoreType scoreType, ScoreBlueprint.ChipMulLevelValues scoreValues)
    {
        foreach (var score in scores)
        {
            if (score.scoreType == scoreType)
            {
                score.UpdateScore(scoreValues);
                break;  
            }
        }
    }
    private void OnDestroy()
    {
        if (DisplayedScoreModel != null)
            DisplayedScoreModel.onLevelUp -= UpdateScoreText;
    }
}
