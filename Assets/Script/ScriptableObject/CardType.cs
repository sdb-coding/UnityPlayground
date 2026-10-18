using UnityEngine;

[CreateAssetMenu(fileName = "CardType", menuName = "Data/CardType")]
public class CardType : ScriptableObject
{
}
public static class CardTypeExtentions
{
    public static bool ContainsCardtype(this CardType[] array, CardType targetObject)
    {
        foreach (var cardType in array)
            if (cardType == targetObject)
                return true;
        return false;
    }
}
