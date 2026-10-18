using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

public interface ICardModel
{
    public string Guid { get; }
    public event Action onActivation;
    public CardType cardType { get; }
    //UI info
    public Task<string> GetCardDescriptionAsync();
    public Task<string> GetCardTitleAsync();
    public CardCondition[] cardConditions { get; }
    public Rarity Rarity { get; }
    public void SetupFromLoad();

#if UNITY_EDITOR
    public string GetCardTitleDebug();
#endif
}
public static class ICardModelExtensions
{
    public static List<string> GetGUIDs<T>(this List<T> cards) where T : ICardModel
    {
        var ids = new List<string>();
        foreach (var card in cards) 
            ids.Add(card.Guid);
        return ids;
    }

    public static List<string> GetGUIDs<T>(this T[] cards) where T : ICardModel
    {
        var ids = new List<string>();
        foreach (var card in cards)
            ids.Add(card.Guid);
        return ids;
    }
}
