using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using System.Threading;
using System;
using Services;
public class CardPopupInfoUIPresenter : MonoBehaviour, IHoverable, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI headerField;
    public TextMeshProUGUI infoField;
    public RectTransform rootContainerRootTransform;
    public List<TypeFieldView> typeFieldViews = new List<TypeFieldView>(4);
    public TooltipTermsData termsData;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    public async Task DisplayCardAsync(ICardModel cardModel, Bounds objectBouonds)
    {
        var headerText = await cardModel.GetCardTitleAsync();
        var infoText = await cardModel.GetCardDescriptionAsync();
        headerField.text = headerText;
        infoField.text = termsData.ProcessText(infoText);

        await SetupTypeFields(cardModel);

        gameObject.SetActive(true);
        SetupPosition(objectBouonds);
        PopupController.RegisterPopup(this, Depth);
    }
    void OnDisable()
    {
        infoField.text = "";
        headerField.text = "";
        foreach (var item in typeFieldViews)
        {
            item.gameObject.SetActive(false);
            item.gameObject.transform.SetParent(null);
        }
    }
    void SetupPosition(Bounds objectBouonds)
    {
        var canvas = GetComponentInParent<Canvas>();
        switch (canvas.renderMode)
        {
            case RenderMode.ScreenSpaceOverlay:
                SetupOverlay(objectBouonds);
                break;
            case RenderMode.ScreenSpaceCamera:
                SetupCamera(objectBouonds, canvas);
                break;
            case RenderMode.WorldSpace:
                break;
            default:
                break;
        }
    }
    void SetupOverlay(Bounds objectBouonds)
    {
        var height = GetHeight();
        var topCenter = new Vector3(objectBouonds.center.x, objectBouonds.max.y, objectBouonds.center.z);
        var screenPosition = CameraController.mainCamera.WorldToScreenPoint(topCenter);
        screenPosition.y += height;
        if (screenPosition.y > Screen.height)
        {
            var bottomCenter = new Vector3(objectBouonds.center.x, objectBouonds.min.y, objectBouonds.center.z);
            screenPosition = CameraController.mainCamera.WorldToScreenPoint(bottomCenter);
        }
        transform.position = screenPosition;
    }
    void SetupCamera(Bounds objectBounds, Canvas canvas)
    {
        var height = GetHeight();

        var rectTransform = transform as RectTransform;
        var parentRect = rectTransform.parent as RectTransform;

        var topCenter = new Vector3(objectBounds.center.x, objectBounds.max.y, objectBounds.center.z);
        var screenPosition = CameraController.mainCamera.WorldToScreenPoint(topCenter);
        screenPosition.y += height;

        if (screenPosition.y > Screen.height)
        {
            var bottomCenter = new Vector3(objectBounds.center.x, objectBounds.min.y, objectBounds.center.z);
            screenPosition = CameraController.mainCamera.WorldToScreenPoint(bottomCenter);
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPosition, canvas.worldCamera, out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }
    float GetHeight()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootContainerRootTransform);
        return rootContainerRootTransform.rect.height;
    }
    public async Task SetupTypeFields(ICardModel cardModel)
    {
        typeFieldViews.Clear();
        switch (cardModel.Rarity)
        {
            default:
            case Rarity.None:
                break;
            case Rarity.Common:
            case Rarity.Uncommon:
            case Rarity.Rare:
            case Rarity.Epic:
            case Rarity.Legendary:
            case Rarity.Mythic:
                TypeFieldView typeFieldView = await TypeFieldView.GetPopupAsync(rootContainerRootTransform);
                await typeFieldView.SetupTypeField(cardModel.Rarity);
                typeFieldView.gameObject.SetActive(true);
                typeFieldViews.Add(typeFieldView);
                break;
        }

        List<Task<TypeFieldView>> tasks = new List<Task<TypeFieldView>>();
        foreach (var item in cardModel.cardConditions)
        {
            tasks.Add(CreateAndSetupSingleFieldAsync(item));
        }
        TypeFieldView[] views = await Task.WhenAll(tasks);
        typeFieldViews.AddRange(views);
    }
    async Task<TypeFieldView> CreateAndSetupSingleFieldAsync(CardCondition cardCondition)
    {
        TypeFieldView fieldView = await TypeFieldView.GetPopupAsync(rootContainerRootTransform);
        await fieldView.SetupTypeField(cardCondition.cardConditionTerm);
        fieldView.gameObject.SetActive(true);
        return fieldView;
    }
    #region IHoverable
    public CardType cardType { get; }
    public bool IsHovering { get; }
    public int Depth { get; set; }
    public RectTransform RectTransform { get => rectTransform; }
    RectTransform rectTransform;
    private CancellationTokenSource dismissCts;
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
    private static AddressablePopupPool<CardPopupInfoUIPresenter> activePool;
    private static bool isInitializing = false;

    public static async Task PreloadAsync(Transform canvasParent = null)
    {
        if (activePool != null || isInitializing) return;

        isInitializing = true;
        activePool = new AddressablePopupPool<CardPopupInfoUIPresenter>(AddressableKeys.CardInfoPopup_KEY, canvasParent == null ? ManagerReferences.MainCanvas.transform : canvasParent);
        await activePool.InitializeAsync(initialSize: 1);
        isInitializing = false;
    }

    public static async Task<CardPopupInfoUIPresenter> GetPopupAsync(Transform canvasParent = null)
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
        CardPopupInfoUIPresenter popup = activePool.GetAvailable();

        return popup;
    }
    #endregion
}
