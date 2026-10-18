using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;
using TMPro;
public class BuySellButtonPresenter : MonoBehaviour, IClickable
{
    [Header("Scene References")]
    public JokerHandPresenter jokerHandPresenter;
    public PlayerHandPresenter playerHandPresenter;
    public CardSelection cardSelection;
    public GameObject graphicsRoot;
    public TextMeshPro buttonText;
    [Header("Settings")]
    public LocalizedString buyLocalizedString;
    public LocalizedString sellLocalizedString;
    public CardType jokerCardType;
    public CardType playingCardType;
    public CardType[] sellableCardTypes;

    IBuyable[] cachedIBuyables;
    void Start()
    {
        if (cardSelection == null)
            cardSelection = FindAnyObjectByType<CardSelection>();
        cardSelection.onSelectionChanged += SelectionChanges;
        DisableButton();
    }
    public void OnClick()
    {
        if (cardSelection.HaveSelection && cardSelection.selection.TryGetAsIBuyables(out IBuyable[] buyables))
        {
            switch (buyables.GetBuySellOfArray())
            {
                case BuySell.Buy:
                    if (buyables.CanBuy(RunContext.Current.rulesModel, RunContext.Current.inventoryModel))
                    {
                        for (int i = 0; i < buyables.Length; i++)
                        {
                            buyables[i].storeModel.BuyCard(buyables[i]);
                            if (buyables[i].cardType.Equals(jokerCardType))
                            {
                                var presenter = cardSelection.selection[i].GameObject.GetComponent<ICardPresenter>();

                                jokerHandPresenter.AddCard(presenter);
                            } else if (buyables[i].cardType.Equals(playingCardType))
                            {
                                var presenter = cardSelection.selection[i].GameObject.GetComponent<ICardPresenter>();
                                playerHandPresenter.AddCard(presenter);
                            }
                        }
                        DisableButton();
                        cardSelection.ClearSelection();
                    }
                    break;
                case BuySell.Sell:
                    int salesValue = 0;
                    foreach (var item in buyables)
                    {
                        salesValue += item.GetSellPrice(RunContext.Current.rulesModel);
                    }
                    foreach (var item in cardSelection.selection)
                    {
                        if (item.GameObject.TryGetComponent(out ICardPresenter presenter))
                        {
                            presenter.currentCardPresenterHolder.RemoveCard(presenter);
                            presenter.DeleteCard(false);
                        }
                    }
                    RunContext.Current.inventoryModel.coins.Value += salesValue;
                    DisableButton();
                    cardSelection.ClearSelection();
                    break;
                case BuySell.Neither:
                case BuySell.Both:
                default:
                    DisableButton();
                    break;
            }

            cardSelection.ClearSelection();
        }
    }
    void SelectionChanges(bool added, ISelectableCard selectableCard)
    {
        if (!cardSelection.HaveSelection)
        {
            DisableButton();
        } else if (cardSelection.selection.TryGetAsIBuyables(out IBuyable[] buyables))
        {
            switch (buyables.GetBuySellOfArray())
            {
                case BuySell.Buy:
                    EnableButton($"{buyLocalizedString.GetLocalizedString()} {GlobalLocalization.ToCurrencyString(buyables.GetTotalBuyPrice(RunContext.Current.rulesModel))}");
                    break;
                case BuySell.Sell:
                    if (sellableCardTypes.ContainsCardtype(CardSelection.currentSelectedCardType))
                        EnableButton($"{sellLocalizedString.GetLocalizedString()} {GlobalLocalization.ToCurrencyString(buyables.GetTotalSellPrice(RunContext.Current.rulesModel))}");
                    else
                        DisableButton();
                    break;
                case BuySell.Neither:
                case BuySell.Both:
                default:
                    DisableButton();
                    break;
            }
        } else
            DisableButton();
    }
    void DisableButton()
    {
        graphicsRoot.SetActive(false);
    }
    void EnableButton(string text)
    {
        graphicsRoot.SetActive(true);
        buttonText.text = text;
    }
}
