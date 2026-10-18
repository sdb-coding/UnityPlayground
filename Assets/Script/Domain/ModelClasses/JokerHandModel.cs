using UnityEngine;
using System.Collections.Generic;
using System;

public class JokerHandModel
{
    public event Action<List<JokerModel>> onJokersAdded;
    public event Action<JokerModel> onJokersRemoved;
    public event Action onJokersRearanged;
    [SaveState][CardModelType(typeof(JokerModel))] public List<string> jokersInPlay;
    public JokerHandModel()
    {
        jokersInPlay = new List<string>();
    }
    public JokerHandModel(JokerModel[] startingCards)
    {
        jokersInPlay = startingCards.GetGUIDs();
    }
    public JokerHandModel(List<JokerModel> startingCards)
    {
        jokersInPlay = startingCards.GetGUIDs();
    }
    public void AddCards(List<JokerModel> cards)
    {
        jokersInPlay.AddRange(cards.GetGUIDs());
        foreach (JokerModel card in cards)
        {
            card.ActivateJoker(RunContext.Current.rulesModel, false);
        }
        if (onJokersAdded != null)
            onJokersAdded(cards);
        if (onJokersRearanged != null)
            onJokersRearanged();
    }
    public void RemoveCards(JokerModel card)
    {
        jokersInPlay.Remove(card.Guid);
        card.DeactivateJoker(RunContext.Current.rulesModel, false);

        if (onJokersRemoved != null)
            onJokersRemoved(card);
        if (onJokersRearanged != null)
            onJokersRearanged();
    }
    public void BuyCard(JokerModel card)
    {
        card.storeModel.BuyCard(card);
    }
    public void NewJokerOrder(int previousIndex, int newIndex)
    {

    }
}
