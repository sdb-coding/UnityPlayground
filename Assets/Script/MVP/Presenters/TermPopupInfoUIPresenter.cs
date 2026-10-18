using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using System.Threading;
using UnityEngine.UI;

public class TermPopupInfoUIPresenter : MonoBehaviour, IHoverable, IPointerEnterHandler, IPointerExitHandler
{
    public static TermPopupInfoUIPresenter Instance;
    public TooltipTermsData termsData;
    public RectTransform rootContainerRootTransform;
    public Image termBackgroundColor;

    public TextMeshProUGUI termHeaderText;
    public TextMeshProUGUI infoFieldText;
    public RectTransform RectTransform { get => rectTransform; }
    RectTransform rectTransform;
    private void Awake()
    {
        Instance = this;
        rectTransform = GetComponent<RectTransform>();
    }
    public async Task ShowTermPopup(string termID, Vector3 mousePosition)
    {
        ToolTipTerm toolTipTerm = termsData.GetTerm(termID);
        await DisplayTermAsync(toolTipTerm);
        gameObject.SetActive(true);
        SetupPosition(mousePosition);
        PopupController.RegisterPopup(this, Depth);
    }
    void SetupPosition(Vector3 screenPosition)
    {
        var height = GetHeight();
        var bottomPoint = screenPosition.y - height;
        if (bottomPoint < 0) //here it should probably spawn above the mouse instead
        {
            screenPosition.y += height; //spawn it above
            //screenPosition.y -= bottomPoint; Make it flush with the bottom,
        }
        transform.position = screenPosition;
    }
    float GetHeight()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootContainerRootTransform);
        return rootContainerRootTransform.rect.height;
    }
    async Task DisplayTermAsync(ToolTipTerm tooltipTerm)
    {
        var toolTipHeader = await tooltipTerm.tooltipHeader.GetLocalizedStringAsync().Task;
        var toolTipBody = await tooltipTerm.tooltipBody.GetLocalizedStringAsync().Task;
        termHeaderText.text = termsData.ProcessText(toolTipHeader);
        infoFieldText.text = termsData.ProcessText(toolTipBody);
        termBackgroundColor.color = tooltipTerm.backgroundColor;
    }
    #region IHoverable
    public CardType cardType { get; }
    public bool IsHovering { get; }
    public int Depth { get; set; }

    private CancellationTokenSource dismissCts;
    public void OnHoverEnter() { throw new System.NotImplementedException(); }
    public void OnHoverExit() { throw new System.NotImplementedException(); }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        PopupController.SetHoveredDepth(Depth);
        CancelDismissTimer();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        PopupController.ClearHoveredDepth(Depth);
        // Start grace period timer before closing popups at this depth or deeper
        dismissCts = new CancellationTokenSource();
        _ = WaitAndDismissAsync(dismissCts.Token);
    }
    private async Task WaitAndDismissAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(SettingsBootstrap.settingsData.UIPopupDismissGracePeriod), token);

            // Grace period elapsed without re-entering -> dismiss all popups from this depth onwards
            PopupController.TryDismissFromDepth(Depth);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
    private void CancelDismissTimer()
    {
        if (dismissCts != null)
        {
            dismissCts.Cancel();
            dismissCts.Dispose();
            dismissCts = null;
        }
    }
    #endregion
    #region Factory
    private static AddressablePopupPool<TermPopupInfoUIPresenter> activePool;
    private static bool isInitializing = false;

    public static async Task PreloadAsync(Transform canvasParent = null)
    {
        if (activePool != null || isInitializing) return;

        isInitializing = true;
        activePool = new AddressablePopupPool<TermPopupInfoUIPresenter>(AddressableKeys.TermInfoPopup_KEY, canvasParent == null ? ManagerReferences.MainCanvas.transform : canvasParent);
        await activePool.InitializeAsync(initialSize: 1);
        isInitializing = false;
    }

    /// <summary>
    /// Displays a popup. Handles async lazy-loading automatically if not preloaded.
    /// </summary>
    public static async Task<TermPopupInfoUIPresenter> GetPopupAsync(Transform canvasParent = null)
    {
        // 1. Lazy initialization if pool hasn't preloaded yet
        if (activePool == null)
        {
            await PreloadAsync(canvasParent);
        }
        else while (isInitializing)
            {
                // Wait a frame if initialization is currently in-flight
                await Task.Yield();
            }

        // 2. Fetch from pool
        TermPopupInfoUIPresenter popup = activePool.GetAvailable();

        return popup;
    }
    #endregion
}
