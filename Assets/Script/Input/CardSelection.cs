using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CardSelection : MonoBehaviour
{
    [Header("Scene References")]
    public PointerInputDetector pointerInputDetector;

    private void OnEnable()
    {
        pointerInputDetector.onClick += OnClick;
    }
    private void OnDisable()
    {
        pointerInputDetector.onClick -= OnClick;
    }

    void OnClick(Collider2D clickedCollider)
    {
        if (clickedCollider == null)
            return;
        foreach (var item in clickedCollider.GetComponents<ISelectableCard>())
        {
            if (selection.Contains(item))
                RemoveFromSelection(item);
            else
                AddToSelection(item);
        }
    }

    public List<ISelectableCard> selection = new List<ISelectableCard>();
    public bool HaveSelection => selection.Count > 0;
    public event System.Action<bool, ISelectableCard> onSelectionChanged;
    public static CardType currentSelectedCardType;
    public void AddToSelection(ISelectableCard selectable)
    {
        if (selection.Contains(selectable))
        {
            selection.Remove(selectable);
            selectable.OnSelect();
            onSelectionChanged?.Invoke(false, selectable);
        }
        else
        {
            if (selectable.CardType != currentSelectedCardType)
                ClearSelection();
            selection.Add(selectable);
            selectable.OnSelect();
            currentSelectedCardType = selectable.CardType;
            onSelectionChanged?.Invoke(true, selectable);
        }
    }
    public bool CurrentSelectionContainsCurrentTarget(Collider2D connectedCollider)
    {
        foreach (var item in connectedCollider.GetComponents<ISelectableCard>())
        {
            if (selection.Contains(item))
                return true;
        }
        return false;
    }
    public void RemoveFromSelection(ISelectableCard selectable)
    {
        if (selection.Contains(selectable))
        {
            selection.Remove(selectable);
            selectable.OnDeselect();
            
            if (selection.Count == 0)
                currentSelectedCardType = null;

            onSelectionChanged?.Invoke(false, selectable);

        }
    }
    public void ClearSelection()
    {
        for (int i = selection.Count - 1; i >= 0; i--)
        {
            RemoveFromSelection(selection[i]);
        }
    }
}
