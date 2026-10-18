using UnityEngine;
using System.Collections.Generic;
using System;
public interface IBuyableScriptableObject
{
    public int StartingPrice { get; }
    public int StartingSellValue { get; }
    public Rarity Rarity { get; }
}
