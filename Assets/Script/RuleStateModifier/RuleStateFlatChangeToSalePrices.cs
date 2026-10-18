using System;
[Serializable]
public abstract class RuleStateFlatChangeToSalePrices : IRuleStateModifier
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
        rules.playingCardSellPrice += ModifyPlayingCardSellPrice;
        rules.jokerCardSellPrice += ModifyJokerSellPrice;
    }
    public override void Unregister(RulesModel rules)
    {
        rules.playingCardSellPrice -= ModifyPlayingCardSellPrice;
        rules.jokerCardSellPrice -= ModifyJokerSellPrice;
    }
    public void ModifyPlayingCardSellPrice(ref int playingCardSellPrice)
    {
        playingCardSellPrice += value;
    }
    public void ModifyJokerSellPrice(ref int JokerSellPrice)
    {
        JokerSellPrice += value;
    }
}