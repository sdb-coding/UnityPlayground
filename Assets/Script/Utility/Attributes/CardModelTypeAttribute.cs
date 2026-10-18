using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class CardModelTypeAttribute : Attribute
{
    public Type CardType { get; }

    public CardModelTypeAttribute(Type cardType)
    {
        CardType = cardType;
    }
}