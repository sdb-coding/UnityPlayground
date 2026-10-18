using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "InventoryBlueprint", menuName = "Data/Blueprint/InventoryBlueprint")]
public class InventoryBlueprint : ScriptableObject
{
    public int defaultStartingCoins = 4;
}
