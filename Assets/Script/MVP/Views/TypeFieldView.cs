using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
public class TypeFieldView : MonoBehaviour
{
    public Image backgroundColor;
    public TextMeshProUGUI typeFieldText;
    public TooltipTermsData termsData;
    public async Task SetupTypeField(ToolTipTerm toolTipTerm)
    {
        backgroundColor.color = toolTipTerm.highlightColor;
        typeFieldText.color = toolTipTerm.textColor;
        typeFieldText.text = termsData.ProcessText(await toolTipTerm.tooltipHeader.GetLocalizedStringAsync().Task);
    }
    public async Task SetupTypeField(Rarity rarity)
    {
        var rarityConfig = rarity.GetConfig();
        backgroundColor.color = rarityConfig.mainColor;
        typeFieldText.color = rarityConfig.textColor;
        typeFieldText.text = termsData.ProcessTextIgnoreColor(await rarity.GetLocalizedNameAsync());
    }

    #region Factory
    private static AddressablePopupPool<TypeFieldView> activePool;
    private static bool isInitializing = false;

    public static async Task PreloadAsync(Transform canvasParent = null)
    {
        if (activePool != null || isInitializing) return;

        isInitializing = true;
        activePool = new AddressablePopupPool<TypeFieldView>(AddressableKeys.TypeField_KEY, canvasParent == null ? ManagerReferences.MainCanvas.transform : canvasParent);
        await activePool.InitializeAsync(initialSize: 1);
        isInitializing = false;
    }

    public static async Task<TypeFieldView> GetPopupAsync(Transform canvasParent = null)
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
        TypeFieldView popup = activePool.GetAvailable();
        if (canvasParent != null)
            popup.transform.SetParent(canvasParent);
        return popup;
    }
    #endregion
}
