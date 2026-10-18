using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public abstract class IRuleStateModifier
{
    public abstract void Init(object variable);
    public abstract void Register(RulesModel rules);
    public abstract void Unregister(RulesModel rules);
    public abstract IRuleStateModifier Clone();

}
public static class IRuleStateModifierExtention
{
    public static List<IRuleStateModifier> GetClones(this List<IRuleStateModifier> list)
    {
        System.Collections.Generic.List<IRuleStateModifier> clones = new System.Collections.Generic.List<IRuleStateModifier>(list.Count);
        foreach (var item in list)
        {
            clones.Add(item.Clone());
        }
        return clones;
    }
}

public interface IRuleStateMaxCardsInHand
{
    public void ModifyMaxCardsInHand(ref int maxNumberOfCardsInHand);
}
public interface IRuleStateMaxNumberOfDraws
{
    public void ModifyMaxNumberOfDraws(ref int maxNumberOfCardsInHand);
}
public interface IRuleStateNumberOfRounds
{
    public void ModifyNumberOfRounds(ref int maxNumberOfCardsInHand);
}

//StoreChanges
public interface IRuleStateNumberOfCardsInStore
{
    public void ModifyNumberOfCardsInStore(ref int numberOfCardsSInStore);
}
public interface IRuleStateAllowBuyingunique
{
    public void ModifyBuyingUniques(ref bool allowBuyingUniques);
}
public interface IRuleStatePlayingCardBuyPrice
{
    public void ModifyPlayingCardBuyPrice(ref bool playingCardBuyPrice);
}
public interface IRuleStatePlayingCardSalePrice
{
    public void ModifyPlayingCardSalePrice(ref bool playingCardSalePrice);
}
public interface IRuleStateJokerBuyPrice
{
    public void ModifyJokerBuyPrice(ref bool JokerBuyPrice);
}
public interface IRuleStateJokerSalePrice
{
    public void ModifyJokerSalePrice(ref bool JokerSalePrice);
}
