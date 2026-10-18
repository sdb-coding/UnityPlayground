using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class UIUtility
{
    private static Dictionary<GameObject, Coroutine> spriteColorRoutines = new Dictionary<GameObject, Coroutine>(10);
    public static void ChangeAlpha(Image image, float alpha)
    {
        var color = image.color;
        color.a = alpha;
        image.color = color;
    }
    public static void ChangeAlpha(MonoBehaviour runner, Image image, float alpha, float time, AnimationCurve changeCurve = null, Action callback = null)
    {
        if (Mathf.Approximately(time, 0f))
        {
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }
        else
        {
            if (spriteColorRoutines.ContainsKey(image.gameObject))
            {
                runner.StopCoroutine(spriteColorRoutines[image.gameObject]);
                spriteColorRoutines[image.gameObject] = runner.StartCoroutine(ChangeAlphaEnumerator(image, alpha, time, changeCurve, callback));
            }
            else
                spriteColorRoutines.Add(image.gameObject, runner.StartCoroutine(ChangeAlphaEnumerator(image, alpha, time, changeCurve, callback)));
        }
    }
    public static System.Collections.IEnumerator ChangeAlphaEnumerator(Image canvasGroup, float alpha, float time, AnimationCurve changeCurve = null, Action callback = null)
    {
        var color = canvasGroup.color;
        var beginningAlpha = color.a;
        var progress = 0f;
        if (changeCurve == null)
            changeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        while (progress < 1)
        {
            progress += Time.deltaTime / time;
            color.a = Mathf.Lerp(beginningAlpha, alpha, changeCurve.Evaluate(progress));
            canvasGroup.color = color;
            yield return null;
        }
        spriteColorRoutines.Remove(canvasGroup.gameObject);
    }
    public static void ChangeAlpha(MonoBehaviour runner, CanvasGroup canvasGroup, float alpha, float time, AnimationCurve changeCurve = null, Action callback = null)
    {
        if (Mathf.Approximately(time, 0f))
        {
            canvasGroup.alpha = alpha;
        }
        else
        {
            if (spriteColorRoutines.ContainsKey(canvasGroup.gameObject))
            {
                runner.StopCoroutine(spriteColorRoutines[canvasGroup.gameObject]);
                spriteColorRoutines[canvasGroup.gameObject] = runner.StartCoroutine(ChangeAlphaEnumerator(canvasGroup, alpha, time, changeCurve, callback));
            }
            else
                spriteColorRoutines.Add(canvasGroup.gameObject, runner.StartCoroutine(ChangeAlphaEnumerator(canvasGroup, alpha, time, changeCurve, callback)));
        }
    }
    public static System.Collections.IEnumerator ChangeAlphaEnumerator(CanvasGroup canvasGroup, float alpha, float time, AnimationCurve changeCurve = null, Action callback = null)
    {
        var beginningAlpha = canvasGroup.alpha;
        var progress = 0f;
        if (changeCurve == null)
            changeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        while (progress < 1)
        {
            progress += Time.deltaTime / time;
            canvasGroup.alpha = Mathf.Lerp(beginningAlpha, alpha, changeCurve.Evaluate(progress));
            yield return null;
        }
        spriteColorRoutines.Remove(canvasGroup.gameObject);
        if (callback != null)
            callback.Invoke();
    }
    public static bool IsMouseOverRect(RectTransform rectTransform)
    {
        if (rectTransform == null || !rectTransform.gameObject.activeInHierarchy)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
            rectTransform,
            StaticInputReferences.PointAction.ReadValue<Vector2>(),
            null // Use null for ScreenSpaceOverlay; pass Camera for WorldSpace / Camera mode
        );
    }

    /// <summary>
    /// Checks if the mouse is currently hovering ANY UI graphic belonging to a list of popup RectTransforms.
    /// </summary>
    public static bool IsPointerOverAnyPopup(List<IHoverable> activePopupRects)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = StaticInputReferences.PointAction.ReadValue<Vector2>()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            foreach (var hoverable in activePopupRects)
            {
                if (hoverable.RectTransform != null && (result.gameObject.transform.IsChildOf(hoverable.RectTransform) || result.gameObject == hoverable.gameObject))
                    return true;
            }
        }
        return false;
    }
    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
#if UNITY_IOS || UNITY_ANDROID
        // Mobile / Touch input
        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count> 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            return true;
        }
#else
        // Standalone / Mouse input
        if (EventSystem.current.IsPointerOverGameObject()) return true;
#endif

        return false;
    }
}
