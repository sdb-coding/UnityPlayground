using System;
using UnityEngine;

[Serializable]
public abstract class RuleStatePercentageChangeToSalePrices : IRuleStateModifier
{
    protected int value;

    public override IRuleStateModifier Clone()
    {
        return (IRuleStateModifier)this.MemberwiseClone();
    }

    public override void Init(object variable)
    {
        if (variable is int value)
            this.value = value;
    }
    public override void Register(RulesModel rules)
    {
        rules.playingCardSellPrice += ModifyPlayingCardSalePrice;
        rules.jokerCardSellPrice += ModifyJokerBuyPrice;
    }
    public override void Unregister(RulesModel rules)
    {
        rules.playingCardSellPrice -= ModifyPlayingCardSalePrice;
        rules.jokerCardSellPrice -= ModifyJokerBuyPrice;
    }
    public void ModifyPlayingCardSalePrice(ref int playingCardSalePrice)
    {
        Mathf.FloorToInt(playingCardSalePrice * (value / 100f));
    }
    public void ModifyJokerBuyPrice(ref int JokerBuyPrice)
    {
        Mathf.FloorToInt(JokerBuyPrice * (value / 100f));
    }
}