using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;

public class PlayingCardView : MonoBehaviour, ICardView
{
    public event Action onSortingOrderUpdated;
    public int lowestSortingOrder { get => backgroundGraphics.sortingOrder; }
    [System.Serializable]
    public struct SuitValueLocations
    {
        public int value;
        public SpriteRenderer[] suitLocations;
    }
    public SpriteRenderer backgroundGraphics;
    public TMPro.TextMeshPro[] textLocations;
    public SpriteRenderer[] suitIndicators;
    public SuitValueLocations[] suitValueLocations; // could be rewritten to use const Vector3 local positions instead and then spawn the Sprite renderer.
    public SpriteRenderer rankGraphics;
    public SpriteRenderer backGraphics;

    public Transform valueSymbolLocations;

    SpriteRenderer[] allSpriteRenderers;
    private void Awake()
    {
        allSpriteRenderers = valueSymbolLocations.GetComponentsInChildren<SpriteRenderer>();
    }
    
    void ResetCard()
    {
        foreach (var spriteRenderer in allSpriteRenderers)
        {
            spriteRenderer.gameObject.SetActive(false);
        }
        backGraphics.gameObject.SetActive(false);
    }
    public void DisplayCard(PlayingCardModel cardModel)
    {
        ResetCard();
        foreach (var text in textLocations)
            text.text = PlayingCardModel.GetValueIndicatorFromValue(cardModel.CardValue);
        foreach (var valueLocation in suitValueLocations)
        {
            if (valueLocation.value == cardModel.CardValue)
            {
                foreach(var spriteRenderer in valueLocation.suitLocations)
                    spriteRenderer.gameObject.SetActive(true);
                break;
            }
        }
        SetupSuitGraphics(cardModel.suitBlueprint.Suit, cardModel.CardValue);
#if UNITY_EDITOR
        gameObject.name = $"{cardModel.suitBlueprint.name} {cardModel.CardValue}";
#endif
    }
    public void SetupSuitGraphics(SuitBlueprint.SuitGraphicsContainer suit, int rank)
    {
        foreach (var spriteRenderer in allSpriteRenderers)
        {
            spriteRenderer.sprite = suit.SuitGraphic.sprite;
            spriteRenderer.color = suit.SuitGraphic.suitColor;
        }
        foreach (var item in suitIndicators)
        {
            item.sprite = suit.SuitGraphic.sprite;
            item.color = suit.SuitGraphic.suitColor;
        }
        foreach (var text in textLocations)
            text.color = suit.SuitGraphic.suitColor;

        if (suit.SuitGraphic.GetRankSprite(rank) is Sprite rankGraphics)
        {
            this.rankGraphics.sprite = rankGraphics;
            this.rankGraphics.gameObject.SetActive(true);
        }
        else
            this.rankGraphics.gameObject.SetActive(false);

            backGraphics.sprite = suit.DeckGraphic;
    }
    public int UpdateSortingOrder(int baseValue)
    {
        backgroundGraphics.sortingOrder = ++baseValue; //is increment before so there is a unused sortingorder before this card, for the selection graphics
        backGraphics.sortingOrder = baseValue;
        rankGraphics.sortingOrder = ++baseValue;
        foreach (var spriteRenderer in allSpriteRenderers)
            spriteRenderer.sortingOrder = ++baseValue;
        foreach (var item in suitIndicators)
            item.sortingOrder = ++baseValue;
        foreach (var text in textLocations)
            text.sortingOrder = ++baseValue;
        if (onSortingOrderUpdated != null)
            onSortingOrderUpdated();
        return ++baseValue;
    }
}

