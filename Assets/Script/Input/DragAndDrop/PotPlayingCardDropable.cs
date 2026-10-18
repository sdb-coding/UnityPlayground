using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class PotPlayingCardDropable : MonoBehaviour, IDropablePoint
{
#if UNITY_EDITOR
    [CustomEditor(typeof(PotPlayingCardDropable))]
    public class PotPlayingCardDropable_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!Application.isPlaying)
                return;
            if (GUILayout.Button("Add debug cards"))
            {
                var cardViewFactory = FindAnyObjectByType<CardViewFactory>(FindObjectsInactive.Include);
                var constructedCards = cardViewFactory.ConstructCardModels(((PotPlayingCardDropable)target).debugCardBlueprints);
                foreach (var item in constructedCards)
                {
                    ((PotPlayingCardDropable)target).cardPresenterHolder.AddCard(cardViewFactory.GetCardPresenter(((PlayingCardDropable)target).acceptableCardTypes[0], item));
                }
            }
        }
    }
    public PlayingCardBlueprint[] debugCardBlueprints;
#endif
    public CardType[] acceptableCardTypes;
    ICardPresenterHolder cardPresenterHolder; //Card holder is not needed as we need PotPresenter for the validater
    void Start()
    {
        cardPresenterHolder = GetComponent<ICardPresenterHolder>();
    }
    #region Dropable
    public DropableResponse IsDropAllowed(MonoBehaviour draggable)
    {
        if (draggable is CardDraggable cardDraggable)
        {
            foreach (var movedCardPresenters in cardDraggable.movedCardPresenters)
            {
                if (!movedCardPresenters.previousCardPresenterHolder.IsMoveable(movedCardPresenters.cardPresenter))
                    return DropableResponse.CantMove;
                if (!acceptableCardTypes.ContainsCardtype(movedCardPresenters.cardPresenter.PresentedModel.cardType))
                    return DropableResponse.NotValid;
            }
            if (cardPresenterHolder.ValidateCards(cardDraggable.movedCardPresenters.GetCardPresenters()))
                return DropableResponse.Allowed;
            
        }
        return DropableResponse.NotReceivingAnything;
    }

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
}
