using UnityEngine;

public class PlayingCardDropable : MonoBehaviour, IDropablePoint
{
    public CardType[] acceptableCardTypes;
    ICardPresenterHolder cardPresenterHolder;
    void Start()
    {
        cardPresenterHolder = GetComponent<ICardPresenterHolder>();
    }
    #region Dropable
    public DropableResponse IsDropAllowed(MonoBehaviour draggable)
    {
        if (draggable is CardDraggable cardDraggable)
        {
            foreach (var movedCardPresenter in cardDraggable.movedCardPresenters)
            {
                if (!movedCardPresenter.previousCardPresenterHolder.IsMoveable(movedCardPresenter.cardPresenter))
                    return DropableResponse.CantMove;
                if (!acceptableCardTypes.ContainsCardtype(movedCardPresenter.cardPresenter.PresentedModel.cardType))
                    return DropableResponse.NotValid;
            }
            if (cardPresenterHolder.ValidateCards(cardDraggable.movedCardPresenters.GetCardPresenters()))
                return DropableResponse.Allowed;
        }
        return DropableResponse.NotReceivingAnything;
    }
    //public bool DropIfAllowed(MonoBehaviour draggable)
    //{
    //    if (draggable is CardDraggable cardDraggable)
    //    {
    //        foreach (var movedCardPresenter in cardDraggable.movedCardPresenters)
    //        {
    //            if (!movedCardPresenter.cardPresenter.PresentedModel.IsMoveable)
    //                return false;
    //            if (!acceptableCardTypes.ContainsCardtype(movedCardPresenter.cardPresenter.PresentedModel.cardType))
    //                return false;
    //        }
    //        if (cardPresenterHolder.ValidateCardsAndAdd(cardDraggable.movedCardPresenters.GetCardPresenters()))
    //            return true;
    //    }
    //    return false;
    //}

    public void DropObject(MonoBehaviour draggable)
    {
        if (draggable is CardDraggable cardDraggable)
        {
            cardPresenterHolder.AddCards(cardDraggable.movedCardPresenters.GetCardPresenters());
        }
        else if (draggable is PlayingCardPresenter cardPresenter)
        {
            cardPresenterHolder.AddCard(cardPresenter);
        }
    }

    public void RemoveObject(MonoBehaviour draggable)
    {
        if (draggable is PlayingCardPresenter cardPresenter)
        {
            cardPresenterHolder.RemoveCard(cardPresenter);
        }
    }
    #endregion //Dropable
    public void SortCardHolder(SortType sortType)
    {
        cardPresenterHolder.SortBy(sortType);
    }
}
