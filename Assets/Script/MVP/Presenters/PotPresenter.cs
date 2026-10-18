using UnityEngine;
using System.Collections.Generic;
using System;

public class PotPresenter : MonoBehaviour, ICardPresenterHolder, IModelContainer
{
    #region IModelContainer
    public object GetModel() => DisplayedPotModel.Value;

    public void ModelRestored()
    {
        cardHolderView.DestroyCards(false);
        List<PlayingCardModel> playingCardModels = new List<PlayingCardModel>();
        foreach (var item in DisplayedPotModel.Value.cardsInPot)
        {
            var model = CardRegistryModel.GetModel<PlayingCardModel>(item);

            AddCardSilent(CardViewFactory.Instance.GetCardPresenter(cardType, model));
            playingCardModels.Add(model);
            model.SetupFromLoad();
        }
        DisplayedPotModel.Value.PopulatePlayingCardCache(playingCardModels);
    }
    #endregion
    public RefEvent<PotModel> DisplayedPotModel { get; private set; }
    public CardType cardType;
    public CardHolderView cardHolderView;
    public IDropablePoint dropablePoint;
    void Awake()
    {
        DisplayedPotModel = new RefEvent<PotModel>(new PotModel(GUID.Generate().ToString()));
        DisplayPot(DisplayedPotModel.Value);
    }
    private void Start()
    {
        dropablePoint = GetComponent<IDropablePoint>();
    }
    public void DisplayPot(PotModel potModel)
    {
        if (!DisplayedPotModel.isNull)
            ClearCurrentPotModelConnections();
        DisplayedPotModel.Set(potModel);

        DisplayedPotModel.Value.onCardsCleared += CardsClearedFromModel;
        DisplayedPotModel.Value.onNewPotScoreResult += NewPotValue;
        DisplayedPotModel.Value.onCardsAdded += CardsAddedToModel;
    }
    void ClearCurrentPotModelConnections()
    {
        DisplayedPotModel.Value.onCardsCleared -= CardsClearedFromModel;
        DisplayedPotModel.Value.onNewPotScoreResult -= NewPotValue;
        DisplayedPotModel.Value.onCardsAdded -= CardsAddedToModel;
    }
    void CardsClearedFromModel()
    {
        cardHolderView.DestroyCards();
    }
    void CardsAddedToModel(List<PlayingCardModel> cardModels)
    {
        List<PlayingCardModel> newAdditions = new List<PlayingCardModel>();
        foreach (var item in cardModels)
        {
            if (!cardHolderView.ContainsCardModel(item.Guid))
                newAdditions.Add(item);
        }
        PlayingCardPresenter[] newPresenters = new PlayingCardPresenter[newAdditions.Count];
        for (int i = 0; i < newPresenters.Length; i++)
        {
            newPresenters[i] = CardViewFactory.Instance.GetCardPresenter(cardType, newAdditions[i]) as PlayingCardPresenter;
            newPresenters[i].currentCardPresenterHolder = this;
        }

        cardHolderView.AddCardPresenterToHolder(newPresenters);
    }
    void NewPotValue(ScoreModel.ScoringResult potResult)
    {
        Debug.Log(potResult.ToLog(RunContext.Current.scoreModel));
    }
    #region ICardHolder
    public void RemoveCard(ICardPresenter cardPresenter)
    {
        DisplayedPotModel.Value.RemoveCards(new List<PlayingCardModel>() { cardPresenter.PresentedModel as PlayingCardModel });
    }
    public void RemoveCardSilent(ICardPresenter cardPresenter)
    {
        cardHolderView.RemoveCardsFromHolder(cardPresenter);
    }
    public void AddCard(ICardPresenter cardPresenter)
    {
        DisplayedPotModel.Value.AddCards(new List<PlayingCardModel>() { cardPresenter.PresentedModel as PlayingCardModel });
    }
    public void AddCards(ICardPresenter[] cardPresenters)
    {
        var cardModels = cardPresenters.GetCardModelsList();
        foreach (var item in cardPresenters) //TODO: Find a more elegant and less jank solution.
        {
            AddCardSilent(item);
        }
        DisplayedPotModel.Value.AddCards(cardModels);
    }
    public void AddCardSilent(ICardPresenter cardPresenter)
    {
        cardHolderView.AddCardPresenterToHolder(cardPresenter);
        cardPresenter.currentCardPresenterHolder = this;
    }
    public bool ValidateCards(ICardPresenter[] cardPresenters)
    {
        return DisplayedPotModel.Value.ValidateCards(cardPresenters.GetCardModelsList()).scoreType != ScoreType.Nothing;
    }
    public bool IsMoveable(ICardPresenter cardPresenter)
    {
        return false;
    }
    public void MoveAttempted(ICardPresenter[] cardPresenter)
    {
        throw new System.NotImplementedException();
    }
    public void SortBy(SortType sortType)
    {
        cardHolderView.SortBy(sortType);
    }
    #endregion
}
