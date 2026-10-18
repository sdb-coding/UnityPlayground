using UnityEngine;
using System.Collections.Generic;

public class PlayHandButtonPresenter : MonoBehaviour, IClickable
{
    [Header("Scene References")]
    public CardSelection cardSelection;
    public PotPresenter potPresenter;
    public GameObject[] graphics;
    [Header("Setup")]
    public CardType[] acceptableCardTypes;
    void Start()
    {
        if (cardSelection == null)
            cardSelection = FindAnyObjectByType<CardSelection>();
        cardSelection.onSelectionChanged += SelectionChanges;
        DisableGraphics();
    }
    public void OnClick()
    {
        if (cardSelection.HaveSelection)
        {
            ICardPresenter[] cardPresenters = new ICardPresenter[cardSelection.selection.Count];
            for (int i = 0; i < cardSelection.selection.Count; i++)
            {
                if (!acceptableCardTypes.ContainsCardtype(cardSelection.selection[i].CardType))
                    return;
                cardPresenters[i] = cardSelection.selection[i].GameObject.GetComponent<ICardPresenter>();
            }
            if (potPresenter.ValidateCards(cardPresenters))
            {

                foreach (var item in cardPresenters)
                {
                    item.currentCardPresenterHolder.RemoveCard(item);
                }
                potPresenter.AddCards(cardPresenters);
            }
            
            cardSelection.ClearSelection();
        }
    }
    void SelectionChanges(bool added, ISelectableCard selectableCard)
    {
        if (added)
        {
            if (acceptableCardTypes.ContainsCardtype(selectableCard.CardType))
                EnableGraphics();
            else
                DisableGraphics();
        } else
        {
            if (cardSelection.HaveSelection && acceptableCardTypes.ContainsCardtype(CardSelection.currentSelectedCardType))
                EnableGraphics();
            else
                DisableGraphics();
        }
    }
    void EnableGraphics()
    {
        foreach (var item in graphics)
        {
            item.SetActive(true);
        }
    }
    void DisableGraphics()
    {
        foreach (var item in graphics)
        {
            item.SetActive(false);
        }
    }
}
