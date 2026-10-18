using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardViewFactory : MonoBehaviour
{
    [System.Serializable]
    public struct CardTypePrefab
    {
        public CardType cardType;
        public GameObject cardViewPrefab;
        [NonSerialized] public Stack<ICardPresenter> existingPresenters;
    }
    public static CardViewFactory Instance;
    
    [SerializeField] SuitGraphics defaultSuitGraphics;
    [SerializeField] DeckGraphics defaultDeckGraphics;

    public CardTypePrefab[] prefabs;
    public Transform cardViewContainer;
    private void Awake()
    {
        Instance = this;
        SuitBlueprint.currentSuitGraphics = defaultSuitGraphics;
        SuitBlueprint.currentDeckGraphics = defaultDeckGraphics;
        for (int i = 0; i < prefabs.Length; i++)
            prefabs[i].existingPresenters = new Stack<ICardPresenter>();
    }
    public PlayingCardModel[] ConstructCardModels(CardCollection cardCollection)
    {
        PlayingCardModel[] cards = new PlayingCardModel[cardCollection.cardBlueprints.Length];
        for (int i = 0; i < cardCollection.cardBlueprints.Length; i++)
        {
            cards[i] = new PlayingCardModel(cardCollection.cardBlueprints[i]);
        }
        return cards;
    }
    public PlayingCardModel[] ConstructCardModels(PlayingCardBlueprint[] cardBlueprints)
    {
        PlayingCardModel[] cards = new PlayingCardModel[cardBlueprints.Length];
        for (int i = 0; i < cardBlueprints.Length; i++)
        {
            cards[i] = new PlayingCardModel(cardBlueprints[i]);
        }
        return cards;
    }
    public ICardPresenter[] GetCardView(CardType cardType, ICardModel[] cardModels)
    {
        foreach (var cardTypePrefab in prefabs)
        {
            if (cardTypePrefab.cardType == cardType)
            {
                ICardPresenter[] cardViews = new ICardPresenter[cardModels.Length];
                for (int i = 0; i < cardViews.Length; i++)
                {
                    cardViews[i] = GetCardPresenter(cardTypePrefab, cardModels[i]);
                }
                return cardViews;
            }
        }
        Debug.LogError($"Cardtype {cardType.name} not found in factory");
        return null;
    }
    public ICardPresenter[] GetCardView(ICardModel[] cardModels)
    {
        ICardPresenter[] cardViews = new ICardPresenter[cardModels.Length];
        for (int i = 0; i < cardModels.Length; i++)
        { 
            foreach (var item in prefabs)
            {
                if (item.cardType == cardModels[i].cardType)
                {
                    cardViews[i] = GetCardPresenter(item, cardModels[i]);
                    break;
                }
            }
        }
        return cardViews;
    }
    public ICardPresenter GetCardPresenter(CardType cardType, ICardModel cardModel)
    {
        var cardView = ConstructCardView(cardType);
        cardView.DisplayCard(cardModel);
        return cardView;
    }
    public ICardPresenter GetCardPresenter(CardTypePrefab cardTypePrefab, ICardModel cardModel)
    {
        var cardView = ConstructCardView(cardTypePrefab);
        cardView.DisplayCard(cardModel);
        return cardView;
    }
    ICardPresenter ConstructCardView(CardType cardType)
    {
        foreach (var item in prefabs)
        {
            if (item.cardType == cardType)
            {
                if (item.existingPresenters.TryPop(out ICardPresenter cardView))
                    return cardView;

                var newPresenter = Instantiate(item.cardViewPrefab, cardViewContainer).GetComponent<ICardPresenter>();
                return newPresenter;
            }
        }
        Debug.LogError($"Cardtype {cardType.name} not found in factory");
        return null;
    }
    ICardPresenter ConstructCardView(CardTypePrefab cardTypePrefab)
    {
        if (cardTypePrefab.existingPresenters.TryPop(out ICardPresenter cardView))
            return cardView;

        var newPresenter = Instantiate(cardTypePrefab.cardViewPrefab, cardViewContainer).GetComponent<ICardPresenter>();
        return newPresenter;
    }

    public void DeleteCard(CardType cardType, ICardPresenter cardView)
    {
        foreach (var item in prefabs)
        {
            if (item.cardType == cardType)
            {
                item.existingPresenters.Push(cardView);
                cardView.Transform.SetParent(cardViewContainer);
                cardView.Transform.localPosition = Vector3.zero;
                return;
            }
        }
        Debug.LogError($"Cardtype {cardType.name} not found in factory, When trying to delete");
    }
}
