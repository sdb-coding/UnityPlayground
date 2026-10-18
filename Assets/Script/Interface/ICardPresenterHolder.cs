using UnityEngine;
using System.Collections.Generic;

public interface ICardPresenterHolder
{
    public void AddCard(ICardPresenter cardPresenter);
    public void AddCards(ICardPresenter[] cardPresenter);
    public void AddCardSilent(ICardPresenter cardPresenter);
    public void RemoveCard(ICardPresenter cardPresenter); 
    public void RemoveCardSilent(ICardPresenter cardPresenter);
    public bool ValidateCards(ICardPresenter[] cardModels);
    //public bool ValidateCardsAndAdd(ICardPresenter[] cardModels);
    public void SortBy(SortType sortType);
    public bool IsMoveable(ICardPresenter cardPresenter);
    public void MoveAttempted(ICardPresenter[] cardPresenter);
}
