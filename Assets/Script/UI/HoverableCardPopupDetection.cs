using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

public class HoverableCardPopupDetection : MonoBehaviour, IHoverable, IBeginClickDown
{
    public CardType cardType { get => cardPresenter.PresentedModel.cardType; }
    public RectTransform RectTransform { get => null; }
    public bool IsHovering { get; }
    public int Depth { get; set; } = 0;
    private CancellationTokenSource hoverCts;
    private CancellationTokenSource dismissCts;
    ICardPresenter cardPresenter;
    Collider2D cardCollider;
    void Awake()
    {
        cardPresenter = GetComponent<ICardPresenter>();
        cardCollider = GetComponent<Collider2D>();
    }
    void OnDisable()
    {
        CancelHoverTimer();
        CancelDismissTimer();
    }
    public void OnBeginClickDown()
    {
        CancelHoverTimer();
        CancelDismissTimer();
        PopupController.DismissAll();
    }
    #region Hoverable
    [ContextMenu("Debug test")]
    public void OnHoverEnter()
    {
        PopupController.SetHoveredDepth(Depth);
        CancelHoverTimer();

        hoverCts = new CancellationTokenSource();
        _ = WaitAndShowPopupAsync(hoverCts.Token);
    }

    public void OnHoverExit()
    {
        CancelHoverTimer();
        PopupController.ClearHoveredDepth(Depth);

        dismissCts = new CancellationTokenSource();
        _ = WaitAndDismissAsync(dismissCts.Token);
    }
    private async Task ShowPopupAsync()
    {
        PopupController.RegisterPopup(this, 0);
        CardPopupInfoUIPresenter popup = await CardPopupInfoUIPresenter.GetPopupAsync();
        popup.Depth = 1;
        await popup.DisplayCardAsync(cardPresenter.PresentedModel, cardCollider.bounds);
    }
    private async Task WaitAndShowPopupAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(SettingsBootstrap.settingsData.UIHoverForPopuptime), token);
            token.ThrowIfCancellationRequested();
            await ShowPopupAsync();
        }
        catch (OperationCanceledException)
        {
            // Hover was cancelled before time elapsed — do nothing
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
    private void CancelHoverTimer()
    {
        if (hoverCts != null)
        {
            hoverCts.Cancel();
            hoverCts.Dispose();
            hoverCts = null;
        }
    }
    private async Task WaitAndDismissAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(SettingsBootstrap.settingsData.UIPopupDismissGracePeriod), token);

            PopupController.TryDismissFromDepth(0);
        }
        catch (OperationCanceledException) { }
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
    public void Hide()
    {
        //throw new NotImplementedException();
    }
    #endregion
}
