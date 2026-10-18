using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Physics2DUtility
{
    public static RaycastHit2D[] mainLoopRaycastHit2Ds = new RaycastHit2D[8];
    public static List<UnityEngine.EventSystems.RaycastResult> graphicHits = new List<UnityEngine.EventSystems.RaycastResult>();
    public static Collider2D[] mainLoopCollider2Ds = new Collider2D[8];
}
