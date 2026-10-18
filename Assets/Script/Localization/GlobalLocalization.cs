using UnityEngine.Localization;

public static class GlobalLocalization
{
    private static readonly LocalizedString CurrencyFormat = new LocalizedString(LocalizationStrings.UiText, LocalizationStrings.CurrencySymbol);

    public static string ToCurrencyString(this int amount)
    {
        // Passes the int directly into Smart Format placeholder {0}
        return CurrencyFormat.GetLocalizedString(amount);
    }

}
