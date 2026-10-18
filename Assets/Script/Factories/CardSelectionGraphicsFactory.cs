using UnityEngine;
using System.Collections.Generic;

public class CardSelectionGraphicsFactory : MonoBehaviour
{
    Stack<Transform> existingSelectionGraphics;

    public Transform selectionGraphicsPrefab;
    public Transform unusedGraphicsContainer;

    public CardType[] cardTypes;

    private void Start()
    {
        ISelectableCard.onNewSelectableCreated += NewCardViewCreated;
        existingSelectionGraphics = new Stack<Transform>();
    }
    private void OnDestroy()
    {
        ISelectableCard.onNewSelectableCreated -= NewCardViewCreated;
    }
    void NewCardViewCreated(ISelectableCard iSelectable)
    {
        if (cardTypes.ContainsCardtype(iSelectable.CardType))
        {
            iSelectable.onSelectionChanged += ISelectionSelectionChanged;
        }
    }

    void ISelectionSelectionChanged(ISelectableCard selectable, bool isSelected)
    {
        if (isSelected)
        {
            AttachGraphics(selectable);
        } else
        {
            DisableGraphics(selectable);
        }
    }
    void AttachGraphics(ISelectableCard selectable)
    {
        var graphics = ConstructGraphics();
        selectable.AttachSelectionGraphics(graphics);
    }
    void DisableGraphics(ISelectableCard selectable)
    {
        if (selectable.AttactedElement is Transform graphics)
        {
            graphics.SetParent(unusedGraphicsContainer);
            graphics.localPosition = Vector3.zero;
            existingSelectionGraphics.Push(graphics);
            selectable.AttactedElement = null;
        }
    }
    Transform ConstructGraphics() //Could be expanded to include a custom class if selection graphics are expanded
    {
        if (existingSelectionGraphics.TryPop(out Transform graphics))
            return graphics;

        var newGraphics = Instantiate(selectionGraphicsPrefab, unusedGraphicsContainer);
        return newGraphics;
    }
}
