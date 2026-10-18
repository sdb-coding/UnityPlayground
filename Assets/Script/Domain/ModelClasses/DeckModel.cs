using UnityEngine;
using System.Collections.Generic;
using System;
public class DeckModel
{
    public event Action onCardsInFullDeckChanged;
    public event Action onCardsInCurrentDeckChanged;
    [CardModelType(typeof(PlayingCardModel))]
    [SaveState] public List<string> cardsInFullDeck;
    [CardModelType(typeof(PlayingCardModel))]
    [SaveState] public List<string> cardsInCurrentDeck;
    public DeckModel(PlayingCardModel[] cardModels)
    {
        cardsInFullDeck = cardModels.GetGUIDs();
        cardsInCurrentDeck = new List<string>(cardModels.Length);
    }
    public void AddCardToDeck(PlayingCardModel cardModel)
    {
        cardsInFullDeck.Add(cardModel.Guid);
        cardsInCurrentDeck.Add(cardModel.Guid);
        if (onCardsInCurrentDeckChanged != null)
            onCardsInCurrentDeckChanged();
        if (onCardsInFullDeckChanged != null)
            onCardsInFullDeckChanged();
    }
    public void RemoveCardFromDeck(PlayingCardModel cardModel)
    {
        cardsInCurrentDeck.Remove(cardModel.Guid);
        if (onCardsInCurrentDeckChanged != null)
            onCardsInCurrentDeckChanged();
    }
    public void DestroyCardInDeck(PlayingCardModel cardModel)
    {
        cardsInFullDeck.Remove(cardModel.Guid);
        if (cardsInCurrentDeck.Contains(cardModel.Guid))
        {
            RemoveCardFromDeck(cardModel);
        }
        if (onCardsInFullDeckChanged != null)
            onCardsInFullDeckChanged();
    }
    public void RemoveCardFromDeck(PlayingCardModel[] cardsToRemove)
    {
        foreach (var cardModel in cardsToRemove)
        {
            cardsInCurrentDeck.Remove(cardModel.Guid);
        }
        if (onCardsInCurrentDeckChanged != null)
            onCardsInCurrentDeckChanged();
    }
    public void ShuffleDeck()
    {
        cardsInCurrentDeck.Shuffle();
    }
    public List<string> DrawCardsToFull(int cardsInHand, bool increaseNumberOfDraws = true)
    {
        int cardsLeft = RunContext.Current.rulesModel.GetMaxNumberOfCardsInHand() - cardsInHand;
        
        return DrawCards(cardsLeft, increaseNumberOfDraws);
    }
    public List<string> DrawCards(int numberOfCards, bool increaseNumberOfDraws = true)
    {
        if (numberOfCards == 0
            || RunContext.Current.numberOfDrawsInCurrentAnte.Value >= RunContext.Current.rulesModel.GetMaxNumberOfDraws())
            return null;
        
        int removedCardsCount = Mathf.Min(numberOfCards, cardsInCurrentDeck.Count);
        List<string> removedCards = cardsInCurrentDeck.GetRange(cardsInCurrentDeck.Count - removedCardsCount, removedCardsCount);
        cardsInCurrentDeck.RemoveRange(cardsInCurrentDeck.Count - removedCardsCount, removedCardsCount);

        if (increaseNumberOfDraws)
            RunContext.Current.numberOfDrawsInCurrentAnte.Value++;

        if (onCardsInCurrentDeckChanged != null)
            onCardsInCurrentDeckChanged();
        return removedCards;
    }

    public void PrepNewAnte()
    {
        cardsInCurrentDeck = new List<string>(cardsInFullDeck);
        RunContext.Current.numberOfDrawsInCurrentAnte.Set(0);
    }
}
