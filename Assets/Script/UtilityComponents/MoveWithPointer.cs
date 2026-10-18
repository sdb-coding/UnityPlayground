using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class MoveWithPointer : MonoBehaviour
{
    public RayVariable pointerPositionRay;
    private void FixedUpdate()
    {
        transform.position = (Vector2)pointerPositionRay.Value.origin;
    }
}
