using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "JokerBlueprint", menuName = "Data/JokerBlueprint")]
public class JokerBlueprint : ScriptableObject, IBlueprint
{
    [field: SerializeField] public string ID { get; private set; }
    [Header("Data")]
    public LocalizedString jokerName; 
    public LocalizedString jokerDescription; 
    public bool showJokerTitle = true;
    public Sprite jokerGraphics;
    public CardType cardType;

    [Header("Commerce")]
    public int startingBuyPrice;
    public int startingSellPrice;
    public Rarity rarity;

    [Header("Effects")]
    [SerializeReference, SelectableSubclass] public List<IRuleEventModifier> ruleEvents = new List<IRuleEventModifier>();
    public List<int> eventValues = new List<int>();
    [SerializeReference, SelectableSubclass] public List<IRuleStateModifier> ruleStates = new List<IRuleStateModifier>();
    public List<int> stateValues = new List<int>();

#if UNITY_EDITOR
    [ContextMenu("Generate ID")]
    public void GenerateID()
    {
        var tableEntry = UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableEntry(
            jokerName.TableReference,
            jokerName.TableEntryReference
        );
        ID = $"Joker_{tableEntry.Entry.SharedEntry.Key}";

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
