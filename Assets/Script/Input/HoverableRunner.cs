using UnityEngine;
using System.Collections.Generic;
public class HoverableRunner : MonoBehaviour
{
    void OnEnable()
    {
        PointerRaycaster.hoverCollider.onChange += OnHoverColliderChange;
        previousHoverables = new IHoverable[0];
    }
    private void OnDisable()
    {
        PointerRaycaster.hoverCollider.onChange -= OnHoverColliderChange;
    }
    Collider2D previousCollider;
    IHoverable[] previousHoverables; 
    void OnHoverColliderChange(RefEvent<Collider2D> newHoverCollider)
    {
        if (previousCollider == newHoverCollider.Value)
            return;
        foreach (var item in previousHoverables)
        {
            item.OnHoverExit();
        }
        if (!newHoverCollider.isNull)
        {
            previousHoverables = newHoverCollider.Value.GetComponentsInChildren<IHoverable>();
            foreach (var item in previousHoverables)
            {
                item.OnHoverEnter();
            }
        }

        previousCollider = newHoverCollider.Value;
    }
}
