using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
public class PotInfoUI : MonoBehaviour
{
    public PotPresenter potPresenter;
    public TextMeshProUGUI chip;
    public TextMeshProUGUI mul;
    public TextMeshProUGUI result;
    public TextMeshProUGUI type;
    public PotModel DisplayedPotModel { get; private set; }

    private void Start()
    {
        if (potPresenter == null)
        {
            gameObject.SetActive(false);
            return;
        }
        if (!potPresenter.DisplayedPotModel.isNull)
            DeckPresenterShowsNewDeckModel(potPresenter.DisplayedPotModel);
        potPresenter.DisplayedPotModel.onChange += DeckPresenterShowsNewDeckModel;
    }
    void ResetInfoUI()
    {
        if (DisplayedPotModel != null)
            DisplayedPotModel.onNewPotScoreResult -= NewPotScore;
    }
    public void DeckPresenterShowsNewDeckModel(RefEvent<PotModel> potModel)
    {
        ResetInfoUI();
        DisplayedPotModel = potModel.Value;
        NewPotScore(DisplayedPotModel.potResult);
        DisplayedPotModel.onNewPotScoreResult += NewPotScore;
    }
    public void NewPotScore(ScoreModel.ScoringResult scoreResult)
    {
        chip.text = scoreResult.TotalChipScore.ToString();
        mul.text = scoreResult.TotalMulScore.ToString();
        result.text = scoreResult.result.ToString();
        type.text = scoreResult.scoringType.scoreType.ToString();
    }
    private void OnDestroy()
    {
        if (DisplayedPotModel != null)
            DisplayedPotModel.onNewPotScoreResult -= NewPotScore;
    }
}
