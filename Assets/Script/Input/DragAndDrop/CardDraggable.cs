using UnityEngine;
using System.Collections.Generic;

public class CardDraggable : MonoBehaviour, IOnDrag, IEndDrag
{
    public struct MovedCardPresenters
    {
        public MovedCardPresenters(ICardPresenter presenter)
        {
            this.cardPresenter = presenter;
            previousCardPresenterHolder = presenter.currentCardPresenterHolder;
        }
        public ICardPresenter cardPresenter;
        public ICardPresenterHolder previousCardPresenterHolder;
        
    }
    public event System.Action<MovedCardPresenters[]> onBeginDrag;
    public event System.Action onEndDrag;
    const int draggedStartingSortingOrder = 10000;
    static bool isContactFilterInitialized;
    static ContactFilter2D endDragContactFilter2D;

    public MovedCardPresenters[] movedCardPresenters { get; private set; }

    [Header("Scene References")]
    public BoxCollider2D cardCollider2D;
    [Header("Settings")]
    public float maxWidthMultipleDrags = 4;
    public float maxDistanceBetweenCardsMultipleDrags = 2;

    void Start()
    {
        if (!isContactFilterInitialized)
        {
            endDragContactFilter2D = new ContactFilter2D()
            {
                useLayerMask = true,
                layerMask = LayerMask.GetMask(new string[] { LayerUtility.cardHoldersLayer, LayerUtility.uiLayer})
            };
            isContactFilterInitialized = true;
        }
    }
    public void BeginDrag(IDraggable[] draggables)
    {
        List<MovedCardPresenters> movedCardPresenters = new List<MovedCardPresenters>(draggables.Length);

        foreach (var item in draggables)
        {
            var connectedCardPresenters = item.GameObject.GetComponentsInChildren<ICardPresenter>();

            for (int i = 0; i < connectedCardPresenters.Length; i++)
            {
                movedCardPresenters.Add(new MovedCardPresenters(connectedCardPresenters[i]));
            }
        }
        this.movedCardPresenters = movedCardPresenters.ToArray();
        transform.SetParent(null);
        int sortingOrder = draggedStartingSortingOrder;
        foreach (var movedCardPresenter in movedCardPresenters)
        {
            movedCardPresenter.cardPresenter.Transform.SetParent(transform);
            sortingOrder = movedCardPresenter.cardPresenter.UpdateSortingOrder(sortingOrder);
        }
        if (onBeginDrag != null)
            onBeginDrag(this.movedCardPresenters);
    }
    #region IDraggable
    public void EndDrag()
    {
        int hits = Physics2D.OverlapBox(transform.position, cardCollider2D.bounds.size, 0, endDragContactFilter2D, Physics2DUtility.mainLoopCollider2Ds);
        for (int i = 0; i < hits; i++)
        {
            var droppable = Physics2DUtility.mainLoopCollider2Ds[i].GetComponent<IDropablePoint>();
            if (droppable != null)
            {
                switch (droppable.IsDropAllowed(this))
                {
                    case DropableResponse.None:
                        break;
                    case DropableResponse.Allowed:
                        foreach (var cardPresenter in movedCardPresenters)
                        {
                            cardPresenter.previousCardPresenterHolder.RemoveCard(cardPresenter.cardPresenter);
                        }
                        droppable.DropObject(this);
                        if (onEndDrag != null)
                            onEndDrag();
                        return;
                    case DropableResponse.CantMove:
                        //TODO: Implement callback to rearrange cards in their respective places.
                        break;
                    case DropableResponse.NotValid:
                        break;
                    case DropableResponse.NotReceivingAnything:
                        break;
                    default:
                        break;
                }
            }
        }
        if (onEndDrag != null)
            onEndDrag();
        ResetCardPresenters();
    }
    void ResetCardPresenters()
    {
        foreach (var cardPresenter in movedCardPresenters)
        {
            cardPresenter.previousCardPresenterHolder.AddCardSilent(cardPresenter.cardPresenter);
        }
        movedCardPresenters = null;
    }

    public void OnDrag(Vector3 delta)
    {
        //Debug.Log("OnDrag");
        transform.position += delta;
    }
    #endregion
}
public static class MovedCardPresentersExtention
{
    //public static PlayingCardModel[] GetCardModels(this CardDraggable.MovedCardPresenters[] array)
    //{
    //    PlayingCardModel[] cardModels = new PlayingCardModel[array.Length];
    //    for (int i = 0; i < array.Length; i++)
    //    {
    //        cardModels[i] = array[i].cardPresenter.currentCardModel;
    //    }
    //    return cardModels;
    //}
    public static ICardPresenter[] GetCardPresenters(this CardDraggable.MovedCardPresenters[] array)
    {
        ICardPresenter[] cardPresenters = new ICardPresenter[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            cardPresenters[i] = array[i].cardPresenter as ICardPresenter;
        }
        return cardPresenters;
    }
}

