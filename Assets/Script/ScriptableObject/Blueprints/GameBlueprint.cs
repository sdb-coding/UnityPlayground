using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameBlueprint", menuName = "Data/Blueprint/GameBlueprint")]
public class GameBlueprint : ScriptableObject, IBlueprint
{
    [field: SerializeField] public string ID { get; private set; }
    public string gameBlueprintName;
    [Header("Project References")]
    public ScoreBlueprint scoreBlueprint;
    public AnteTargetScoreBlueprint anteScoreBlueprint;
    public RulesBlueprint rulesBlueprint;
    public StoreBlueprint storeBlueprint;
    [Space()]
    public InventoryBlueprint inventoryBlueprint;
    public CardCollection cardCollection;


#if UNITY_EDITOR
    [ContextMenu("Generate ID")]
    public void GenerateID()
    {
        //var tableEntry = UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableEntry(
        //    suitBlueprint.suitName.TableReference,
        //    suitBlueprint.suitName.TableEntryReference
        //);
        //ID = $"PlayingCard_{tableEntry.Entry.SharedEntry.Key}_{value.ToString()}";
        ID = $"GameBlueprint_{gameBlueprintName}";
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
