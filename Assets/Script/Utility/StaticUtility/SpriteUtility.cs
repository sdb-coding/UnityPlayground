using System.Collections.Generic;
using UnityEngine;

public static class SpriteUtility 
{
    private static Dictionary<GameObject, Coroutine> spriteColorRoutines = new Dictionary<GameObject, Coroutine>(10);
    public static void ChangeAlpha(SpriteRenderer renderer, float alpha)
    {
        var color = renderer.color;
        color.a = alpha;
        renderer.color = color;
    }
    public static void ChangeAlpha(MonoBehaviour runner, SpriteRenderer renderer, float alpha, float time, AnimationCurve changeCurve = null)
    {
        if (Mathf.Approximately(time, 0f))
        {
            var color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        } else
        {
            if (spriteColorRoutines.ContainsKey(renderer.gameObject))
            {
                runner.StopCoroutine(spriteColorRoutines[renderer.gameObject]);
                spriteColorRoutines[renderer.gameObject] = runner.StartCoroutine(ChangeAlphaEnumerator(renderer, alpha, time, changeCurve));
            } else
                spriteColorRoutines.Add(renderer.gameObject, runner.StartCoroutine(ChangeAlphaEnumerator(renderer, alpha,time,changeCurve)));
        }
    }
    public static System.Collections.IEnumerator ChangeAlphaEnumerator(SpriteRenderer renderer, float alpha, float time, AnimationCurve changeCurve = null)
    {
        var color = renderer.color;
        var beginningAlpha = color.a;
        var progress = 0f;
        if (changeCurve == null)
            changeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        while (progress < 1)
        {
            progress += Time.deltaTime / time;
            color.a = Mathf.Lerp(beginningAlpha, alpha, changeCurve.Evaluate(progress));
            renderer.color = color;
            yield return null;
        }
        spriteColorRoutines.Remove(renderer.gameObject);
    }
    public static void ChangeColorWithoutAlpha(SpriteRenderer renderer, Color color)
    {
        var originalColor = renderer.color;
        originalColor.r = color.r;
        originalColor.g = color.g;
        originalColor.b = color.b;
        renderer.color = originalColor;
    }
}
