using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerHandPresenter : MonoBehaviour, IModelContainer, ICardPresenterHolder
{
    public PlayerHandModel DisplayedPlayerHandModel { get; private set; }
    public CardType cardType;
    public CardHolderView cardHolderView;
    public IDropablePoint dropablePoint;
    private void Start()
    {
        dropablePoint = GetComponent<IDropablePoint>();
    }
    void OnEnable()
    {
        RunContext.onRunContextBound += RunContextBound;
        if (RunContext.Current != null)
            RunContextBound(RunContext.Current);
    }
    void OnDisable()
    {
        RunContext.onRunContextBound -= RunContextBound;
    }
    void RunContextBound(GameModel gameModel)
    {
        if (gameModel.playerHandModel != null)
            DisplayPlayerHand(gameModel.playerHandModel);
    }
    public void DisplayPlayerHand(PlayerHandModel playerHandModel)
    {
        if (DisplayedPlayerHandModel != null)
            ClearCurrentPotModelConnections();
        DisplayedPlayerHandModel = playerHandModel;

        DisplayedPlayerHandModel.onCardsCleared += CardsClearedFromModel;
        DisplayedPlayerHandModel.onCardsAdded += CardsAddedToModel;
        DisplayedPlayerHandModel.onCardRemoved += CardsRemovedFromModel;
        foreach (var item in playerHandModel.cardsInHand)
        {
            var model = CardRegistryModel.GetModel<PlayingCardModel>(item);
            var cardPresenter = CardViewFactory.Instance.GetCardPresenter(cardType, model);
            AddCardSilent(cardPresenter);
        }
    }
    void ClearCurrentPotModelConnections()
    {
        DisplayedPlayerHandModel.onCardsCleared -= CardsClearedFromModel;
        DisplayedPlayerHandModel.onCardsAdded -= CardsAddedToModel;
    }
    void CardsClearedFromModel()
    {
        cardHolderView.DestroyCards();
    }
    void CardsAddedToModel(List<string> cardGuids)
    {
        List<string> newAdditions = new List<string>();
        foreach (var item in cardGuids)
        {
            if (!cardHolderView.ContainsCardModel(item))
                newAdditions.Add(item);
        }
        PlayingCardPresenter[] newPresenters = new PlayingCardPresenter[newAdditions.Count];
        for (int i = 0; i < newPresenters.Length; i++)
        {
            var model = CardRegistryModel.GetModel<PlayingCardModel>(newAdditions[i]);
            newPresenters[i] = CardViewFactory.Instance.GetCardPresenter(cardType, model) as PlayingCardPresenter;
            newPresenters[i].currentCardPresenterHolder = this;
        }
        
        cardHolderView.AddCardPresenterToHolder(newPresenters);
    }
    void CardsRemovedFromModel(string cardModels)
    {
        cardHolderView.RemoveCardsFromHolder(cardModels);
    }
    #region ICardHolder
    public void RemoveCard(ICardPresenter cardPresenter)
    {
        if (cardPresenter is PlayingCardPresenter playingCardPresenter)
            DisplayedPlayerHandModel.RemoveCard(playingCardPresenter.currentCardModel.Guid);
    }
    public void RemoveCardSilent(ICardPresenter cardPresenter)
    {
        cardHolderView.RemoveCardsFromHolder(cardPresenter);
    }
    public void AddCard(ICardPresenter cardPresenter)
    {
        if (cardPresenter is PlayingCardPresenter playingCardPresenter)
        {
            DisplayedPlayerHandModel.AddCards(new List<string>() { playingCardPresenter.currentCardModel.Guid });
            playingCardPresenter.currentCardPresenterHolder = this;
        }
    }
    public void AddCards(ICardPresenter[] cardPresenters)
    {
        if (cardPresenters.TryGetPlayingCardModel(out List<PlayingCardModel> playingCardModels))
        {
            DisplayedPlayerHandModel.AddCards(playingCardModels.GetGUIDs());
            foreach (var item in cardPresenters)
            {
                item.currentCardPresenterHolder = this;
            }
        }
    }
    public void AddCardSilent(ICardPresenter cardPresenter)
    {
        if (cardPresenter is PlayingCardPresenter playingCardPresenter)
        {
            cardHolderView.AddCardPresenterToHolder(playingCardPresenter);
            playingCardPresenter.currentCardPresenterHolder = this;
        }
    }
    public bool ValidateCards(ICardPresenter[] cardPresenters)
    {
        var cardModels = cardPresenters.GetCardModelsList();
        return true;
    }
    public bool IsMoveable(ICardPresenter cardPresenter)
    {
        return true;
    }
    public void MoveAttempted(ICardPresenter[] cardPresenter)
    {
        throw new System.NotImplementedException();
    }
    public void SortBy(SortType sortType)
    {
        cardHolderView.SortBy(sortType);
    }

    public object GetModel() => DisplayedPlayerHandModel;

    public void ModelRestored()
    {
    }
    #endregion
}
