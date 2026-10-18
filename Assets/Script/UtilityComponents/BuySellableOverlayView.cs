using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;
using TMPro;

public class BuySellableOverlayView : MonoBehaviour
{
    [Header("Setup")]
    public BuySell buyOrSell;
    public LocalizedString textBeforeCost;
    public SpriteRenderer background; 

    public Color successColor;
    public Color failureColor;

    [Header("Scene References")]
    public CardDraggable cardDraggable;
    public TextMeshPro textMeshPro;
    public GameRunner gameRunner;
    private CoinsViewModel coinViewModel;
    RulesModel rulesModel;
    private CardDraggable.MovedCardPresenters[] currentDragMovedCardPresenters;
    private void Start()
    {
        if (gameRunner == null)
        {
            gameRunner = FindAnyObjectByType<GameRunner>();
        }
        if (!gameRunner.gameModel.isNull)
            DisplayGameModel(gameRunner.gameModel);
        gameRunner.gameModel.onChange += DisplayGameModel;
    }
    public void DisplayGameModel(RefEvent<GameModel> gameModel)
    {
        rulesModel = gameModel.Value.rulesModel;
        InitViewModel(gameModel.Value.inventoryModel);
    }
    public void InitViewModel(InventoryModel inventoryModel)
    {
        coinViewModel = new CoinsViewModel(inventoryModel);
        coinViewModel.coins.onChange += EvaluateBuyingCurrentCard;
    }
    void EvaluateBuyingCurrentCard(RefEvent<int> coins)
    {
        if (currentDragMovedCardPresenters == null)
            return;
        int totalCost = 0;
        foreach (var item in currentDragMovedCardPresenters)
        {
            if (item.cardPresenter.PresentedModel is IBuyable buyable)
            {
                if (!buyable.IsBuyable())
                    return;
                totalCost += buyable.GetBuyPrice(rulesModel);
            }
        }
        background.enabled = true;
        textMeshPro.enabled = true;
        if (totalCost > coins.Value)
        {
            //Cannot purchase
            background.color = failureColor;
            textMeshPro.text = $"{textBeforeCost.GetLocalizedString()} {totalCost.ToString()}";
        }
        else
        {
            //Can purchase
            background.color = successColor;
            textMeshPro.text = $"{textBeforeCost.GetLocalizedString()} {totalCost.ToString()}";
        }
    }
    public void OnEnable()
    {
        cardDraggable.onBeginDrag += OnBeginDrag;
        cardDraggable.onEndDrag += OnEndDrag;
        background.enabled = false;
        textMeshPro.enabled = false;
    }
    private void OnDisable()
    {
        cardDraggable.onBeginDrag -= OnBeginDrag;
        cardDraggable.onEndDrag -= OnEndDrag;
    }
    void OnBeginDrag(CardDraggable.MovedCardPresenters[] movedCardPresenters)
    {
        switch (buyOrSell)
        {
            case BuySell.Buy:
            default:
                currentDragMovedCardPresenters = movedCardPresenters;
                EvaluateBuyingCurrentCard(coinViewModel.coins);
                break;
            case BuySell.Sell:
                int totalValue = 0;
                foreach (var item in movedCardPresenters)
                {
                    if (item.cardPresenter.PresentedModel is IBuyable buyable)
                    {
                        if (!buyable.IsSellable)
                            return;
                        totalValue += buyable.GetSellPrice(rulesModel);
                    }
                }
                background.enabled = true;
                textMeshPro.enabled = true;
                background.color = successColor;
                textMeshPro.text = $"{textBeforeCost.GetLocalizedString()} {totalValue.ToString()}";
                break;
        }
    }
    void OnEndDrag()
    {
        currentDragMovedCardPresenters = null;
        background.enabled = false;
        textMeshPro.enabled = false;
    }
}
