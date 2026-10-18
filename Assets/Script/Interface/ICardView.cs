using UnityEngine;
using System.Collections.Generic;

public interface ICardView
{
    public event System.Action onSortingOrderUpdated;
    public int UpdateSortingOrder(int baseValue);
    public int lowestSortingOrder { get; }
}
