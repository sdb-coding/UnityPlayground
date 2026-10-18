using UnityEngine;
using System.Collections.Generic;
using System;

public class StoreRunner : MonoBehaviour, IBaseEventListener
{
    public StoreCardHandPresenter storeCardPresenter;
    public BaseEvent finishStoreBaseEvent;
    GameModel gameModel;
    private void OnEnable()
    {
        finishStoreBaseEvent.RegisterListener(this);
    }
    private void OnDisable()
    {
        finishStoreBaseEvent.UnRegisterListener(this);
    }
    public void StartNewStore(GameModel gameModel)
    {
        StoreModel storeModel = new StoreModel(gameModel.storeBlueprint);
        storeCardPresenter.DisplayModel(storeModel);
        storeModel.AddRandomBuyablesForSale(gameModel.rulesModel.GetNumberOfCardsInStore());
        this.gameModel = gameModel;
    }

    public void OnEventRaised(BaseEvent baseEvent)
    {
        if (gameModel.onStoreComplete != null)
            gameModel.onStoreComplete();
    }
}
