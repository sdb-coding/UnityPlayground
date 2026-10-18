using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeScreenTransitionCanvasGroup : MonoBehaviour, ISceneLifecycle
{
    [Header("Configuration")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeToClean = 0.5f;
    [SerializeField] private float fadeToColor = 0.5f;

    private void Reset()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    public async Task OnPrepareUnloadAsync()
    {
        await canvasGroup.DOFade(0, fadeToClean).AsyncWaitForCompletion();
    }

    public void ReceiveLoadPayload(object loadPayload) { }

    public async Task OnSceneLoadedAsync()
    {
        await canvasGroup.DOFade(1, fadeToColor).AsyncWaitForCompletion();
    }

    //private async Task Fade(float endAlpha, float duration)
    //{
    //    if (duration <= 0f)
    //    {
    //        canvasGroup.alpha = endAlpha;
    //        return;
    //    }

    //    float elapsed = 0f;
    //    var startAlpha = canvasGroup.alpha;

    //    while (elapsed < duration)
    //    {
    //        await Awaitable.NextFrameAsync();

    //        elapsed += Time.deltaTime;
            
    //        float progress = Mathf.Clamp01(elapsed / duration);

    //        canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
    //    }

    //    canvasGroup.alpha = endAlpha;
    //}
}
