using System.Collections.Generic;
using UnityEngine.Localization;

public static class LocalizationExtensions
{
    private const string DefaultForm = "noun";

    public static string GetLocalizedWithDefault(this LocalizedString localizedString, Dictionary<string, object> args = null)
    {
        // 1. If no dictionary was passed, initialize a new one
        if (args == null)
        {
            args = new Dictionary<string, object>();
        }

        // 2. Safely inject the default "form" if it's missing
        if (!args.ContainsKey("form"))
        {
            args.Add("form", DefaultForm);
        }

        return localizedString.GetLocalizedString(args);
    }
}