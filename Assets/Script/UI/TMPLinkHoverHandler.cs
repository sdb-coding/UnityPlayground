using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Threading;
using System.Threading.Tasks;
using System;
using UnityEngine.InputSystem;

public class TMPLinkHoverHandler : MonoBehaviour, IHoverable, IPointerEnterHandler, IPointerExitHandler
{
    public InputActionReference pointerInputActionReference;
    private TextMeshProUGUI textMeshPro;
    private Canvas parentCanvas;
    private Camera uiCamera;

    private int currentHoveredLinkIndex = -1;
    private IHoverable parentHoverable;
    private void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        parentCanvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        parentHoverable = transform.parent.GetComponentInParent<IHoverable>();
        // Set camera depending on Canvas Render Mode
        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            uiCamera = null; // Overlay mode uses null for screen position checks
        else
            uiCamera = parentCanvas.worldCamera; //
    }
    #region IHoverable
    public CardType cardType { get; }
    public bool IsHovering { get; }
    public int Depth { get => parentHoverable.Depth + 1; }
    public RectTransform RectTransform { get => rectTransform; }
    RectTransform rectTransform;
    private CancellationTokenSource dismissCts;
    private CancellationTokenSource hoverCts;
    private CancellationTokenSource linkCheckCts;
    public void OnHoverEnter()
    {
        throw new System.NotImplementedException();
    }

    public void OnHoverExit()
    {
        throw new System.NotImplementedException();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Stop the link-checking loop
        StopLinkChecking();
        CancelHoverTimer();
        CancelDismissTimer();
        currentHoveredLinkIndex = -1;
        // Clear hover status for this depth
        PopupController.SetHoveredDepth(Depth);

        // Start grace period timer before closing popups at this depth or deeper
        StartLinkChecking();
    }

    private void StartLinkChecking()
    {
        StopLinkChecking();
        linkCheckCts = new CancellationTokenSource();
        _ = TrackHoveredLinkAsync(linkCheckCts.Token);
    }
    private async Task TrackHoveredLinkAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                // Check intersecting link index at current mouse position
                int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, Input.mousePosition, uiCamera);

                if (linkIndex != -1 && linkIndex != currentHoveredLinkIndex)
                {
                    currentHoveredLinkIndex = linkIndex;
                    TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
                    string linkID = linkInfo.GetLinkID();
                    Vector3 linkScreenPos = GetLinkCenterScreenPosition(linkIndex);
                    // Trigger the tooltip popup
                    hoverCts = new CancellationTokenSource();
                    _ = WaitAndShowPopupAsync(linkID, linkScreenPos, hoverCts.Token);
                }
                else if (linkIndex == -1 && currentHoveredLinkIndex != -1)
                {
                    currentHoveredLinkIndex = -1;
                    CancelHoverTimer();
                    // Note: Mouse is still over the TMP text component, but off the link target
                }

                // Poll every 30ms (~33 times per second) instead of running on Update every frame
                await Task.Delay(TimeSpan.FromMilliseconds(30), token);
            }
        }
        catch (OperationCanceledException) { }
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
    private void StopLinkChecking()
    {
        if (linkCheckCts != null)
        {
            linkCheckCts.Cancel();
            linkCheckCts.Dispose();
            linkCheckCts = null;
        }
    }
    private async Task WaitAndShowPopupAsync(string linkID, Vector3 spawnPosition, CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(SettingsBootstrap.settingsData.UIHoverForPopuptime), token);

            StopLinkChecking();

            await ShowPopupAsync(linkID, spawnPosition);
        } catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
    private async Task ShowPopupAsync(string linkID, Vector3 spawnPosition)
    {
        TermPopupInfoUIPresenter popup = await TermPopupInfoUIPresenter.GetPopupAsync();
        popup.Depth = Depth +1;
        await popup.ShowTermPopup(linkID, spawnPosition);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        StopLinkChecking();
        CancelHoverTimer();
        currentHoveredLinkIndex = -1;
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

            if (UIUtility.IsMouseOverRect(parentHoverable.gameObject.GetComponent<RectTransform>()))
            {
                return;
            }
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
    public Vector3 GetLinkCenterScreenPosition(int linkIndex)
    {
        if (textMeshPro == null || textMeshPro.textInfo == null)
            return pointerInputActionReference.action.ReadValue<Vector2>(); // Fallback to mouse position if text info is unavailable

        TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];

        Bounds linkBounds = new Bounds();
        bool boundsInitialized = false;

        // Loop through all characters that make up this link tag
        for (int i = linkInfo.linkTextfirstCharacterIndex; i < linkInfo.linkTextfirstCharacterIndex + linkInfo.linkTextLength; i++)
        {
            TMP_CharacterInfo charInfo = textMeshPro.textInfo.characterInfo[i];

            // Skip invisible characters (spaces, line breaks, zero-width chars)
            if (!charInfo.isVisible)
                continue;

            // Obtain bottom-left and top-right vertices of the character in TMP local space
            Vector3 bottomLeft = charInfo.bottomLeft;
            Vector3 topRight = charInfo.topRight;

            Bounds charBounds = new Bounds();
            charBounds.SetMinMax(bottomLeft, topRight);

            if (!boundsInitialized)
            {
                linkBounds = charBounds;
                boundsInitialized = true;
            }
            else
            {
                linkBounds.Encapsulate(charBounds); // Expand bounds to include this character
            }
        }

        if (!boundsInitialized)
            return pointerInputActionReference.action.ReadValue<Vector2>();

        // Transform local link center to World Space
        Vector3 localCenter = linkBounds.center;
        Vector3 worldCenter = textMeshPro.transform.TransformPoint(localCenter);

        // Transform World Space center to Screen Space
        Camera cameraForConversion = (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : uiCamera;
        Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(cameraForConversion, worldCenter);

        return screenPosition;
    }
    #endregion
}
