using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;
using System.Text.RegularExpressions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(fileName = "TooltipTermsData", menuName = "Data/TooltipTermsData")]
public class TooltipTermsData : ScriptableObject
#if !UNITY_EDITOR
    , ISerializationCallbackReceiver
#endif
{
    [SerializeField] List<ToolTipTerm> allTerms = new List<ToolTipTerm>();
    private Dictionary<string, ToolTipTerm> termLookup = new Dictionary<string, ToolTipTerm>();
    public List<ToolTipTerm> AllTerms => allTerms;
    public void OnAfterDeserialize()
    {
        RebuildDictionary();
    }
    public void OnBeforeSerialize() { }
    public void RebuildDictionary()
    {
        termLookup.Clear();

        foreach (ToolTipTerm term in allTerms)
        {
            if (term == null || string.IsNullOrEmpty(term.termID))
                continue;

            if (!termLookup.ContainsKey(term.termID))
            {
                termLookup.Add(term.termID, term);
            }
            else
            {
                Debug.LogWarning($"Duplicate termID found in TermsDatabase: '{term.termID}' on term asset '{term.name}'", this);
            }
        }
    }


    public ToolTipTerm GetTerm(string termID)
    {
        if (string.IsNullOrEmpty(termID))
            return null;
#if UNITY_EDITOR

        // Ensure dictionary is populated if running in the Unity Editor
        if (termLookup.Count == 0 && allTerms.Count > 0)
        {
            RebuildDictionary();
        }
#endif
        if (termLookup.TryGetValue(termID, out ToolTipTerm term))
        {
            return term;
        }

        Debug.LogWarning($"GameTerm with ID '{termID}' was not found in the database.");
        Debug.LogWarning($"Total database size: {termLookup.Count}");
        return null;
    }
    private static readonly Regex TermRegex = new Regex(@"\[term:([^\|\]]+)(?:\|([^\]]+))?\]");

    public string ProcessText(string rawText)
    {
        if (string.IsNullOrEmpty(rawText))
            return rawText;

        return TermRegex.Replace(rawText, match =>
        {
            string termID = match.Groups[1].Value.Trim();
            string modifier = match.Groups[2].Success ? match.Groups[2].Value.Trim() : null;

            ToolTipTerm term = GetTerm(termID);
            if (term == null)
                return match.Value;

            string displayText;

            if (string.IsNullOrEmpty(modifier))
            {
                // Default term display name
                displayText = term.GetFormattedDisplayName();
            }
            else
            {
                // Check if the modifier is a recognized grammar key (e.g., adj, poss, plural)
                // GetFormattedDisplayName will pass it to Smart Strings
                displayText = term.GetFormattedDisplayName(modifier);
            }

            string hexColor = ColorUtility.ToHtmlStringRGB(term.highlightColor);

            return $"<color=#{hexColor}><link=\"{term.termID}\">{displayText}</link></color>";
        });
    }
    public string ProcessTextIgnoreColor(string rawText)
    {
        if (string.IsNullOrEmpty(rawText))
            return rawText;

        return TermRegex.Replace(rawText, match =>
        {
            string termID = match.Groups[1].Value.Trim();
            string modifier = match.Groups[2].Success ? match.Groups[2].Value.Trim() : null;

            ToolTipTerm term = GetTerm(termID);
            if (term == null)
                return match.Value;

            string displayText;

            if (string.IsNullOrEmpty(modifier))
            {
                displayText = term.GetFormattedDisplayName();
            }
            else
            {
                displayText = term.GetFormattedDisplayName(modifier);
            }

            string hexColor = ColorUtility.ToHtmlStringRGB(term.highlightColor);

            return $"<link=\"{term.termID}\">{displayText}</link>";
        });
    }
#if UNITY_EDITOR
    public ToolTipTerm CreateNewTerm(string termID, LocalizedString displayName)
    {
        ToolTipTerm newTerm = CreateInstance<ToolTipTerm>();
        newTerm.name = $"Term_{termID}";
        newTerm.termID = termID;
        newTerm.defaultDisplayName = displayName;

        AssetDatabase.AddObjectToAsset(newTerm, this);
        allTerms.Add(newTerm);

        RebuildDictionary();
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        return newTerm;
    }

    public void DeleteTerm(ToolTipTerm term)
    {
        if (allTerms.Contains(term))
        {
            allTerms.Remove(term);
            AssetDatabase.RemoveObjectFromAsset(term);
            DestroyImmediate(term, true);

            RebuildDictionary();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
#endif
}
