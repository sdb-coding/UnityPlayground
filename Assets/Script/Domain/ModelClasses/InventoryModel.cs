using UnityEngine;

public class InventoryModel
{
    public InventoryModel() { }
    public InventoryModel(InventoryBlueprint inventoryBlueprint)
    {
        coins.Value = inventoryBlueprint.defaultStartingCoins;
    }
    [SaveState] public RefEvent<int> coins = new RefEvent<int>();
    [SaveState] public RefEvent<int> gems = new RefEvent<int>();
}
