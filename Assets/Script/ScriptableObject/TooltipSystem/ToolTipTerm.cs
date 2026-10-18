using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
public class ToolTipTerm : ScriptableObject
{
    public string termID;              // e.g. "king", "feudal", "piety"
    public LocalizedString defaultDisplayName;  // e.g. "King"
    public Color highlightColor = Color.gold;
    public Color textColor = Color.black;
    public Color backgroundColor = Color.white;

    public LocalizedString tooltipHeader;
    public LocalizedString tooltipBody;
    public Sprite icon;
    public string GetFormattedDisplayName(string grammarForm = null)
    {
        if (defaultDisplayName == null || defaultDisplayName.IsEmpty)
            return termID;

        // If a grammar parameter is provided, pass it as a Smart String argument named 'form'
        if (!string.IsNullOrEmpty(grammarForm))
        {
            // Pass dictionary arguments anonymously as an object
            return defaultDisplayName.GetLocalizedString(new Dictionary<string, object>
                {
                    { "form", grammarForm }
                });
        }

        return defaultDisplayName.GetLocalizedString();
    }
}