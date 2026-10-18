using UnityEngine;
using System.Collections.Generic;

public interface IBuyable 
{
    public string Guid { get; }
    public StoreModel storeModel { get; set; }
    public CardType cardType { get; }
    public bool IsSellable { get; set; }
    public bool IsUnique { get; set; }

    public int GetBuyPrice(RulesModel rulesModel);
    public int GetSellPrice(RulesModel rulesModel);
    public void SetupIBuyable(StoreModel storeModel);
}
public static class IBuyableExtention
{
    public static List<string> GetGUIDs(this IBuyable[] array)
    {
        List<string> ids = new List<string>(array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            ids.Add(array[i].Guid);
        }
        return ids;
    }
    public static ICardModel[] GetCardModels(this IBuyable[] array)
    {
        ICardModel[] cardModels = new ICardModel[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            cardModels[i] = array[i] as ICardModel;
        }
        return cardModels;
    }
    public static bool IsBuyable(this IBuyable buyable)
    {
        return buyable.storeModel != null;
    }
    public static BuySell GetBuySellOfArray(this IBuyable[] buyables)
    {
        BuySell buySell = BuySell.Neither;
        foreach (var item in buyables)
        {
            if (item is IBuyable selectionBuyable)
            {
                if (selectionBuyable.IsBuyable())
                {
                    if (buySell == BuySell.Sell)
                    {
                        return BuySell.Both;
                    }
                    buySell = BuySell.Buy;
                }
                else if (selectionBuyable.IsSellable)
                {
                    if (buySell == BuySell.Buy)
                    {
                        return BuySell.Both;

                    }
                    buySell = BuySell.Sell;
                }
                else
                {
                    return BuySell.Neither;
                }
            }
            else
            {
                return BuySell.Neither;

            }
        }
        return buySell;
    }
    public static int GetTotalBuyPrice(this IBuyable[] buyables, RulesModel rulesModel)
    {
        int price = 0;
        foreach (var item in buyables)
        {
            price += item.GetBuyPrice(rulesModel);
        }
        return price;
    }
    public static int GetTotalSellPrice(this IBuyable[] buyables, RulesModel rulesModel)
    {
        int price = 0;
        foreach (var item in buyables)
        {
            price += item.GetSellPrice(rulesModel);
        }
        return price;
    }
    public static bool CanBuy(this IBuyable[] buyables, RulesModel rulesModel, InventoryModel inventoryModel)
    {
        int value = 0;
        foreach (var buyable in buyables)
        {
            if (buyable == null)
                return false;
            value += buyable.GetBuyPrice(rulesModel);
        }
        return value <= inventoryModel.coins.Value;
    }
}
