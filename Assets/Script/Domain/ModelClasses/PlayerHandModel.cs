using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerHandModel
{
    public event Action<List<string>> onCardsAdded;
    public event Action<string> onCardRemoved;
    public event Action onCardsCleared;
    [SaveState][CardModelType(typeof(PlayingCardModel))] public List<string> cardsInHand;

    public PlayerHandModel()
    {
        cardsInHand = new List<string>();
    }
    public PlayerHandModel(PlayingCardModel[] startingCards)
    {
        cardsInHand = startingCards.GetGUIDs();
    }
    public PlayerHandModel(List<string> startingCards)
    {
        cardsInHand = startingCards;
    }
    public void AddCards(List<string> cards)
    {
        cardsInHand.AddRange(cards);
        
        if (onCardsAdded != null)
            onCardsAdded.Invoke(cards);
    }
    public void RemoveCard(string card)
    {
        cardsInHand.Remove(card);
        if (onCardRemoved != null)
            onCardRemoved.Invoke(card);
    }
}
