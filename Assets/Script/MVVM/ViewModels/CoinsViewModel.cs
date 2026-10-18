using UnityEngine;

public class CoinsViewModel 
{
    public CoinsViewModel(InventoryModel inventoryModel)
    {
        coins = inventoryModel.coins;
        inventoryModel.coins.onChange += _ => coins.Set(inventoryModel.coins);
    }
    public RefEvent<int> coins = new RefEvent<int>();
}
