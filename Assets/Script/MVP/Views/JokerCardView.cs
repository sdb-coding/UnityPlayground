using UnityEngine;
using System.Collections.Generic;
using TMPro;
public class JokerCardView : MonoBehaviour, ICardView
{
    public event System.Action onSortingOrderUpdated;
    public int lowestSortingOrder { get => backgroundGraphics.sortingOrder; }

    public SpriteRenderer backgroundGraphics;
    public SpriteRenderer backGraphics;
    public SpriteRenderer jokerGraphics;
    public TextMeshPro[] jokerDescriptions;

    void ResetCard()
    {
        jokerGraphics.gameObject.SetActive(false);
        backGraphics.gameObject.SetActive(false);
        foreach (var text in jokerDescriptions)
            text.gameObject.SetActive(false);
    }
    public void DisplayCard(JokerModel jokerModel)
    {
        ResetCard();
        foreach (var text in jokerDescriptions)
            text.gameObject.SetActive(jokerModel.JokerBlueprint.showJokerTitle);

        jokerGraphics.gameObject.SetActive(true);

        jokerGraphics.sprite = jokerModel.JokerBlueprint.jokerGraphics;
#if UNITY_EDITOR
        gameObject.name = $"{jokerModel.JokerBlueprint.name} ";
#endif
    }
    public int UpdateSortingOrder(int baseValue)
    {
        backgroundGraphics.sortingOrder = ++baseValue; //is increment before so there is a unused sortingorder before this card, for the selection graphics
        backGraphics.sortingOrder = baseValue;
        jokerGraphics.sortingOrder = ++baseValue;
        foreach (var text in jokerDescriptions)
            text.sortingOrder = ++baseValue;
        if (onSortingOrderUpdated != null)
            onSortingOrderUpdated();
        return ++baseValue;
    }
}
