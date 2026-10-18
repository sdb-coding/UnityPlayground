using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class StoreCardHandPresenter : MonoBehaviour, ICardPresenterHolder, IModelContainer
{
    //#if UNITY_EDITOR
    //    [CustomEditor(typeof(StoreCardHandPresenter))]
    //    public class StoreCardPresenter_Editor : Editor
    //    {
    //        public override void OnInspectorGUI()
    //        {
    //            base.OnInspectorGUI();
    //            if (!Application.isPlaying)
    //                return;
    //            if (GUILayout.Button("Spawn Cards"))
    //            {
    //                if (((StoreCardHandPresenter)target).DisplayedStoreModel == null)
    //                {
    //                    var anteRunner = FindAnyObjectByType<AnteRunner>(FindObjectsInactive.Exclude);
    //                    StoreModel storeModel = new StoreModel(((StoreCardHandPresenter)target).debugStoreBlueprint, anteRunner.rulesBlueprint,anteRunner.inventoryBlueprint);
    //                    ((StoreCardHandPresenter)target).DisplayModel(storeModel);
    //                }
    //                ((StoreCardHandPresenter)target).DisplayedStoreModel.AddRandomBuyablesForSale(3);
    //            }
    //        }
    //    }
    //    public StoreBlueprint debugStoreBlueprint;
    //#endif
    #region IModelContainer

    public object GetModel() => DisplayedStoreModel;

    public void ModelRestored()
    {
       
    }
    #endregion
    public StoreModel DisplayedStoreModel { get; private set; }
    public CardHolderView cardHolderView;
    public void DisplayModel(StoreModel storeModel)
    {
        ResetStoreModel();
        DisplayedStoreModel = storeModel;
        DisplayedStoreModel.onBuyableAdded += BuyablesAdded;
        DisplayedStoreModel.onBuyableRemoved += BuyablesRemoved;
    }
    void ResetStoreModel()
    {
        if (DisplayedStoreModel != null)
        {
            DisplayedStoreModel.onBuyableAdded -= BuyablesAdded;
            DisplayedStoreModel.onBuyableRemoved -= BuyablesRemoved;
            DisplayedStoreModel = null;
        }
    }
    void BuyablesAdded(IBuyable[] buyables)
    {
        var cardPresenters = CardViewFactory.Instance.GetCardView(buyables.GetCardModels());
        foreach (var item in cardPresenters)
        {
            AddCardSilent(item);
        }
    }
    void BuyablesRemoved(IBuyable[] buyables)
    {
        cardHolderView.RemoveCardsFromHolder(buyables.GetCardModels(), false);
    }
    public void RemoveCardSilent(ICardPresenter cardPresenter)
    {
        cardHolderView.RemoveCardsFromHolder(cardPresenter);
    }
    #region ICardPresenterHolder
    public void AddCard(ICardPresenter cardPresenter)
    {
        if (cardPresenter.PresentedModel is IBuyable iBuyable)
        {
            DisplayedStoreModel.AddBuyable(new IBuyable[] { iBuyable });
            cardPresenter.currentCardPresenterHolder = this;
        }
    }
    public void AddCards(ICardPresenter[] cardPresenters)
    {
        if (cardPresenters.TryGetIBuyables(out IBuyable[] buyables))
        {
            DisplayedStoreModel.AddBuyable(buyables);
            foreach (var item in cardPresenters)
            {
                item.currentCardPresenterHolder = this;
            }
        }
    }
    public void AddCardSilent(ICardPresenter cardPresenter)
    {
        cardPresenter.currentCardPresenterHolder = this;
        cardHolderView.AddCardPresenterToHolder(cardPresenter);
    }

    public void RemoveCard(ICardPresenter cardPresenter)
    {
        if (cardPresenter.PresentedModel is IBuyable iBuyable)
            DisplayedStoreModel.RemoveBuyable(new IBuyable[] { iBuyable });
    }

    public bool ValidateCards(ICardPresenter[] cardModels)
    {
        return false;
    }
    public bool IsMoveable(ICardPresenter cardPresenter)
    {
        return true;
    }
    public void MoveAttempted(ICardPresenter[] cardPresenter)
    {
        throw new System.NotImplementedException();
    }

    public void SortBy(SortType sortType)
    {
        return;
    }
    #endregion
}
