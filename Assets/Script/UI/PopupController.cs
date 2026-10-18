using UnityEngine;
using System.Collections.Generic;

public static class PopupController
{
    public static readonly List<IHoverable> activeHoverables = new List<IHoverable>(8);

    static int CurrentHoveredDepth { get; set; } = -1;
    public static void RegisterPopup(IHoverable hoverable, int depth)
    {
        DismissPopupsFromDepth(depth);
        activeHoverables.Add(hoverable);
    }
    static void DismissPopupsFromDepth(int depth)
    {
        for (int i = activeHoverables.Count - 1; i >= 0; i--)
        {
            if (activeHoverables[i].Depth >= depth)
            {
                activeHoverables[i].Hide();
                activeHoverables.RemoveAt(i);
            }
        }
    }
    public static void SetHoveredDepth(int depth)
    {
        CurrentHoveredDepth = depth;
    }
    public static void ClearHoveredDepth(int depth)
    {
        // If we are exiting the element that currently holds active hover status,
        // provisionally set depth to -1 (grace period will determine if another took over).
        if (CurrentHoveredDepth == depth)
        {
            CurrentHoveredDepth = -1;
        }
    }
    public static bool TryDismissFromDepth(int depth)
    {
        if (CurrentHoveredDepth >= depth)
        {
            return false;
        }

        if (!UIUtility.IsPointerOverAnyPopup(activeHoverables) 
            && (PointerRaycaster.hoverCollider.Value == null || activeHoverables.Count == 0 || PointerRaycaster.hoverCollider.Value.GetComponent<IHoverable>() != activeHoverables[0]))
        {
            DismissAll();
            return true;
        } else
        {
            DismissPopupsFromDepth(depth);
        }
        //CurrentHoveredDepth = depth -1;

        return true;
    }
    public static void DismissAll()
    {
        DismissPopupsFromDepth(0);
    }
}

