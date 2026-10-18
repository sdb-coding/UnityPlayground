using UnityEngine;
using System.Collections.Generic;

public class ClickableRunner : MonoBehaviour
{
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
        foreach (var item in clickedCollider.GetComponents<IClickable>())
        {
            item.OnClick();
        }
    }
}
