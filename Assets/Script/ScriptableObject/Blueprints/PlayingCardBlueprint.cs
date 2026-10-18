using UnityEngine;
using UnityEngine.Localization;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "PlayingCardBlueprint", menuName = "Data/Blueprint/PlayingCardBlueprint")]
public class PlayingCardBlueprint : ScriptableObject, IBlueprint
{
    [field: SerializeField] public string ID { get; private set; }
    public LocalizedString cardDescription;
    public int value;
    public SuitBlueprint suitBlueprint;
    public CardType cardType;
    public int startingBuyPrice;
    public int startingSellPrice;
    public Rarity rarity = Rarity.None;
    string cachedName;

    public async Task<string> GetCardTitleAsync()
    {
        if (string.IsNullOrEmpty( cachedName ))
        {
            string localizedSuit = await suitBlueprint.suitName.GetLocalizedStringAsync().Task;
            string valueIndicator = PlayingCardModel.GetValueIndicatorFromValue(value);

            cachedName = $"{localizedSuit} {valueIndicator}";
        }
        return cachedName;
    }
#if UNITY_EDITOR
    public string GetCardTitleDebug()
    {
        if (string.IsNullOrEmpty(cachedName))
        {
            string localizedSuit = suitBlueprint.suitName.GetLocalizedString();
            string valueIndicator = PlayingCardModel.GetValueIndicatorFromValue(value);

            cachedName = $"{localizedSuit} {valueIndicator}";
        }
        return cachedName;
    }
    [ContextMenu("Generate ID")]
    public void GenerateID()
    {
        var tableEntry = UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableEntry(
            suitBlueprint.suitName.TableReference,
            suitBlueprint.suitName.TableEntryReference
        );
        ID = $"PlayingCard_{tableEntry.Entry.SharedEntry.Key}_{value.ToString()}";
        UnityEditor.EditorUtility.SetDirty( this ); 
    }
#endif
}
