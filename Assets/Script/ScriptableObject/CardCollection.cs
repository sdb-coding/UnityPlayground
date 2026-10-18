using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardCollection", menuName = "Data/CardCollection")]
public class CardCollection : ScriptableObject
{
    public PlayingCardBlueprint[] cardBlueprints;
}
public static class CardCollectionExtention
{
    public static PlayingCardModel[] GetCardModels(this CardCollection cardCollection)
    {
        PlayingCardModel[] cardModels = new PlayingCardModel[cardCollection.cardBlueprints.Length];
        for (int i = 0; i < cardModels.Length; i++)
        {
            cardModels[i] = new PlayingCardModel(cardCollection.cardBlueprints[i]);
        }
        return cardModels;
    }
}
