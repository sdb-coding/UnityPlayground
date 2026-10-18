using System.Collections.Generic;
using UnityEngine;

public interface ISelectableCard
{
    public static System.Action<ISelectableCard> onNewSelectableCreated;
    public event System.Action<ISelectableCard, bool> onSelectionChanged;
    public CardType CardType { get; }
    public void OnSelect();
    public void OnDeselect();
    public GameObject GameObject { get; }
    public object AttactedElement { get; set; } //Most likely SpriteRenderer but could be anything that could be used.
    public void AttachSelectionGraphics(Transform transform);
}
public static class ISelectableCardExtention
{
    public static bool TryGetAsIBuyables(this List<ISelectableCard> list, out IBuyable[] buyables)
    {
        buyables = new IBuyable[list.Count];
        for (int i = 0; i < buyables.Length; i++)
        {
            if (list[i].GameObject.TryGetComponent(out ICardPresenter cardPresenter))
            {
                buyables[i] = cardPresenter.PresentedModel as IBuyable;
                if (buyables[i] == null)
                    return false;
            }
            else
                return false;
        }
        return true;
    }
}
