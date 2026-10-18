using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public static class PointerRaycaster
{
    public static RefEvent<Collider2D> hitCollider { get; private set; } = new RefEvent<Collider2D>();
    public static RefEvent<Collider2D> hoverCollider { get; private set; } = new RefEvent<Collider2D>();
    private static readonly List<RaycastResult> s_RaycastResults = new List<RaycastResult>();
    private static PointerEventData eventData;

    public static void Init()
    {
        eventData = new PointerEventData(EventSystem.current);
    }
    public static void DoRaycast(Vector2 pointerPosition, Ray ray, LayerMask detectableLayers)
    {
        if (IsPositionOverUI(pointerPosition, eventData))
        {
            hoverCollider.Set(null);
            return;
        }
        var raycastHit2D = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, detectableLayers);
        hoverCollider.Set(raycastHit2D.collider);
    }
    public static void SaveRaycast()
    {
        hitCollider.Set(hoverCollider.Value);
    }
    
    public static bool IsPositionOverUI(Vector2 screenPosition, PointerEventData eventData)
    {
        if (EventSystem.current == null) return false;

        eventData.position = screenPosition;

        s_RaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, s_RaycastResults);

        return s_RaycastResults.Count > 0;
    }
}
