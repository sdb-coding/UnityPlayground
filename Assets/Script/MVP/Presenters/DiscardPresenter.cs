using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Rendering;

public class DiscardPresenter : MonoBehaviour, IClickable, IHoverable, IDropablePoint
{
    [Header("Scene References")]
    public CardSelection cardSelection;
    [Header("Internal References")]
    public DiscardView discardView;
    public Transform endDestination;
    [Header("Settings")]
    public CardType[] acceptableCardTypes;
    public float openGraphicsTime = 0.5f;

    Coroutine closeRoutine;
    void Start()
    {
        if (cardSelection == null)
            cardSelection = FindAnyObjectByType<CardSelection>();
    }
    public void OnClick()
    {
        if(cardSelection.HaveSelection
            && RunContext.Current.CanDiscard())
        {
            ICardPresenter[] cardPresenters = new ICardPresenter[cardSelection.selection.Count];
            for (int i = 0; i < cardSelection.selection.Count; i++)
            {
                if (!acceptableCardTypes.ContainsCardtype(cardSelection.selection[i].CardType))
                    return;
                cardPresenters[i] = cardSelection.selection[i].GameObject.GetComponent<ICardPresenter>();
            }
            DestroyCards(cardPresenters, true);
            cardSelection.ClearSelection();
        }
    }
    void DestroyCards(ICardPresenter[] cardPresenters, bool moveCard)
    {
        foreach (var cardPresenter in cardPresenters)
        {
            if (cardPresenter.currentCardPresenterHolder == null)
            {
                Debug.LogError("Card presenter without ICardPresenterHolder requested for deletion");
                cardPresenter.DeleteCard();
                continue;
            }
            cardPresenter.currentCardPresenterHolder.RemoveCard(cardPresenter);
            if (moveCard)
                CardAnimUtility.MoveCardToPosition(cardPresenter, 4, endDestination.position, CardReached);
            else
                CardReached(cardPresenter);
        }
        if (cardPresenters.Length != 0 && RunContext.Current.rulesModel.rulesBlueprint.automaticallyDrawWhenDiscarding)
        {
            RunContext.Current.DrawToFull();
        }
    }
    public void CardReached(ICardPresenter cardPresenter)
    {
        cardPresenter.DeleteCard();
        if (closeRoutine != null)
            StopCoroutine(closeRoutine);
        closeRoutine = StartCoroutine(Close());
        discardView.Open();
    }
    IEnumerator Close()
    {
        yield return WaitFor.Seconds(openGraphicsTime);
        discardView.Close();
        closeRoutine = null;
    }
    #region IHoverable
    public CardType cardType { get; }
    public bool IsHovering { get; }
    public int Depth { get; set; }
    public RectTransform RectTransform { get => null; }
    public void OnHoverEnter()
    {
        discardView.EnableHover();
    }

    public void OnHoverExit()
    {
        discardView.DisableHover();
    }
    private void OnDisable()
    {
        discardView.DisableHover();
    }
    #endregion
    public DropableResponse IsDropAllowed(MonoBehaviour droppedBehaviour)
    {
        if (!RunContext.Current.CanDiscard())
            return DropableResponse.NotValid;
        if (droppedBehaviour is CardDraggable cardDraggable)
        {
            foreach (var movedCardPresenters in cardDraggable.movedCardPresenters)
            {
                if (!movedCardPresenters.previousCardPresenterHolder.IsMoveable(movedCardPresenters.cardPresenter))
                    return DropableResponse.CantMove;
                if (!acceptableCardTypes.ContainsCardtype(movedCardPresenters.cardPresenter.PresentedModel.cardType))
                    return DropableResponse.NotValid;
            }
            return DropableResponse.Allowed;
        }
        return DropableResponse.NotValid;
    }
    //public bool DropIfAllowed(MonoBehaviour droppedBehaviour)
    //{
    //    if (IsDropAllowed(droppedBehaviour)
    //        && droppedBehaviour is CardDraggable cardDraggable)
    //    {
    //        DestroyCards(cardDraggable.movedCardPresenters.GetCardPresenters(), false);
    //        return true;
    //    }
    //    return false;
    //}

    public void DropObject(MonoBehaviour droppedBehaviour)
    {
        if (droppedBehaviour is CardDraggable cardDraggable)
            DestroyCards(cardDraggable.movedCardPresenters.GetCardPresenters(), false);
    }

    public void RemoveObject(MonoBehaviour droppedBehaviour)
    {
        throw new System.NotImplementedException();
    }

    public void Hide()
    {
        throw new System.NotImplementedException();
    }
}
