using UnityEngine;
using System.Collections.Generic;
using System;

public class DeckPresenter : MonoBehaviour, IClickable, IModelContainer
{
    public DeckView deckView;
    public PlayerHandPresenter handPresenter;
    public CardType cardType;
    public RefEvent<DeckModel> DisplayedDeckModel { get; private set; }
    void Awake()
    {
        DisplayedDeckModel = new RefEvent<DeckModel>();
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
        DisplayDeck(gameModel.deckModel);
    }
    public void DisplayDeck(DeckModel deckModel)
    {
        DisplayedDeckModel.Set(deckModel);
    }

    public void OnClick()
    {
        var newCards = DisplayedDeckModel.Value.DrawCardsToFull(handPresenter.DisplayedPlayerHandModel.cardsInHand.Count);
        if (newCards == null)
        {
            Debug.LogError("Cannot draw any cards");
            return;
        }
        foreach (var item in newCards)
        {
            PlayingCardModel model = CardRegistryModel.GetModel<PlayingCardModel>(item);
            var cardPresenter = CardViewFactory.Instance.GetCardPresenter(cardType, model);
            handPresenter.AddCard(cardPresenter);
        }
    }

    public void UpdateSuit(Sprite newDeckGraphics)
    {
        deckView.UpdateSuit(newDeckGraphics);
    }
    #region ModelProvider
    public object GetModel() => DisplayedDeckModel.Value;

    public void ModelRestored()
    {
    }
    #endregion
}
