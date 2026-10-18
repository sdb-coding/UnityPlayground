using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DeckInfoUI : MonoBehaviour
{
    public TextMeshProUGUI cardsLeftValue;
    public TextMeshProUGUI drawsLeftValue;
    public DeckPresenter deckPresenter;

    public DeckModel DisplayedDeckModel { get; private set; }

    private void Start()
    {
        if (deckPresenter == null)
        {
            deckPresenter = FindAnyObjectByType<DeckPresenter>();
        }
        if (!deckPresenter.DisplayedDeckModel.isNull)
            DeckPresenterShowsNewDeckModel(deckPresenter.DisplayedDeckModel);
        deckPresenter.DisplayedDeckModel.onChange += DeckPresenterShowsNewDeckModel;
    }
    void ResetInfoUI()
    {
        if (DisplayedDeckModel != null)
        {
            DisplayedDeckModel.onCardsInCurrentDeckChanged -= CurrentDeckChanged;
        }
        if (RunContext.Current != null)
            RunContext.Current.numberOfDrawsInCurrentAnte.onChange -= DrawsLeftChanged;
        
    }
    public void DeckPresenterShowsNewDeckModel(RefEvent<DeckModel> deckModel)
    {
        ResetInfoUI();
        DisplayedDeckModel = deckModel;
        CurrentDeckChanged();
        DisplayedDeckModel.onCardsInCurrentDeckChanged += CurrentDeckChanged;
        DrawsLeftChanged(RunContext.Current.numberOfDrawsInCurrentAnte);
        RunContext.Current.numberOfDrawsInCurrentAnte.onChange += DrawsLeftChanged;
    }
    void CurrentDeckChanged()
    {
        cardsLeftValue.text = DisplayedDeckModel.cardsInCurrentDeck.Count.ToString();
    }
    void DrawsLeftChanged(RefEvent<int> drawsTaken)
    {
        drawsLeftValue.text = (RunContext.Current.rulesModel.GetMaxNumberOfDraws() - drawsTaken.Value).ToString();
    }
    private void OnDestroy()
    {
        if (DisplayedDeckModel != null)
            DisplayedDeckModel.onCardsInCurrentDeckChanged -= CurrentDeckChanged;
    }
}
