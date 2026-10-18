using UnityEngine;
using System.Collections.Generic;
using System;

public class CardHolderView : MonoBehaviour
{
    public event Action onCardsChanged;
    public Transform container;
    List<ICardPresenter> currentCardsPresenters;
    public int startingSortingOrder = 50;
    SortType lastRequestedSort;
    int currentSortingOrder;
    private void Awake()
    {
        currentCardsPresenters = new List<ICardPresenter>();
        currentSortingOrder = startingSortingOrder;
        lastRequestedSort = SortType.Rank;
    }
    private void OnDisable()
    {
        DestroyCards(false);
    }

    public void DestroyCards(bool runGraphics = true)
    {
        foreach (var item in currentCardsPresenters)
        {
            item.DeleteCard(runGraphics);
        }
        currentCardsPresenters.Clear();
        currentSortingOrder = startingSortingOrder;
    }
    public void AddCardPresenterToHolder(List<ICardPresenter> addedCardPresenters)
    {
        foreach (var cardPresenter in addedCardPresenters)
        {
            cardPresenter.Transform.SetParent(container);
            currentSortingOrder = cardPresenter.UpdateSortingOrder(currentSortingOrder);
            currentCardsPresenters.Add(cardPresenter);
        }
        if (onCardsChanged != null)
            onCardsChanged();
        SortBy(lastRequestedSort);
    }
    public void AddCardPresenterToHolder(ICardPresenter[] addedCardPresenters, bool doSorting = true)
    {
        foreach (var cardPresenter in addedCardPresenters)
        {
            cardPresenter.Transform.SetParent(container);
            currentSortingOrder = cardPresenter.UpdateSortingOrder(currentSortingOrder);
            currentCardsPresenters.Add(cardPresenter);
        }
        if (onCardsChanged != null)
            onCardsChanged();
        if (doSorting)
            SortBy(lastRequestedSort);
    }
    public void AddCardPresenterToHolder(ICardPresenter addedCardPresenter)
    {
        addedCardPresenter.Transform.SetParent(container);
        currentSortingOrder = addedCardPresenter.UpdateSortingOrder(currentSortingOrder);
        currentCardsPresenters.Add(addedCardPresenter);
        if (onCardsChanged != null)
            onCardsChanged();
        SortBy(lastRequestedSort);
    }
    public void RemoveCardsFromHolder(ICardModel[] removedCardModels, bool doSorting = true)
    {
        foreach (var cardModel in removedCardModels)
        {
            for (int i = currentCardsPresenters.Count-1; i >= 0; i--)
            {
                if (currentCardsPresenters[i].PresentedModel.Guid == cardModel.Guid)
                    RemoveCardsFromHolder(currentCardsPresenters[i], false);
            }
        }
        if (onCardsChanged != null)
            onCardsChanged();
        if (doSorting)
            SortBy(lastRequestedSort);
    }
    public void RemoveCardsFromHolder(string removedCardModelGUID, bool doSorting = true)
    {
        foreach (var item in currentCardsPresenters)
        {
            if (item.PresentedModel != null && item.PresentedModel.Guid == removedCardModelGUID)
            {
                currentCardsPresenters.Remove(item);
                if (doSorting)
                {
                    if (onCardsChanged != null)
                        onCardsChanged();
                    SortBy(lastRequestedSort);
                }
            return;
            }
        }
        Debug.LogError($"Asked to remove {removedCardModelGUID} that isn't in holder: {gameObject.name}");
    }
    public void RemoveCardsFromHolder(ICardPresenter removedCardPresenter, bool doSorting = true)
    {
        if (currentCardsPresenters.Contains(removedCardPresenter))
        {
            currentCardsPresenters.Remove(removedCardPresenter);
            if (doSorting)
            {
                if (onCardsChanged != null)
                    onCardsChanged();
                SortBy(lastRequestedSort);
            }
        }
        else
            Debug.LogError($"Asked to remove {removedCardPresenter.Transform.gameObject.name} that isn't in holder: {gameObject.name}");
    }
    void UpdateSorting()
    {
        currentSortingOrder = startingSortingOrder;
        foreach (var cardPresenter in currentCardsPresenters)
            currentSortingOrder = cardPresenter.UpdateSortingOrder(currentSortingOrder);
    }
    public bool ContainsCardModel(string cardGuid)
    {
        foreach (var item in currentCardsPresenters)
        {
            if (item.PresentedModel != null && item.PresentedModel.Guid == cardGuid)
                return true;
        }
        return false;
    }

    public void SortBy(SortType sortType)
    {
        switch (sortType)
        {
            case SortType.Suit:
                currentCardsPresenters.SortBySuit();
                break;
            default:
            case SortType.Rank:
                currentCardsPresenters.SortByValue();
                break;
        }
        for (int i = 0; i < currentCardsPresenters.Count; i++)
        {
            currentCardsPresenters[i].Transform.SetSiblingIndex(i);
        }
        UpdateSorting();
        lastRequestedSort = sortType;
    }
}
