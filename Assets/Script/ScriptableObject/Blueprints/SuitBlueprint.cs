using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "SuitBlueprint", menuName = "Data/Blueprint/SuitBlueprint")]
public class SuitBlueprint : ScriptableObject, IBlueprint
{
    public static SuitGraphics currentSuitGraphics { get; set; }
    public static DeckGraphics currentDeckGraphics { get; set; }
    public class SuitGraphicsContainer
    {
        public event System.Action<SuitGraphicsContainer> onSuitGraphicsChanged;
        public event System.Action<Sprite> onDeckGraphicsChanged;
        private SuitGraphics.SuitGraphic suitGraphic;

        public SuitGraphics.SuitGraphic SuitGraphic
        {
            get => suitGraphic; set
            {
                suitGraphic = value;
                if (onSuitGraphicsChanged != null)
                    onSuitGraphicsChanged.Invoke(this);
            }
        }
        private Sprite deckGraphic;
        public Sprite DeckGraphic
        {
            get => deckGraphic; set
            {
                deckGraphic = value;
                if (onDeckGraphicsChanged != null)
                    onDeckGraphicsChanged.Invoke(deckGraphic);
            }
        }

    }
    private SuitGraphicsContainer suit;
    public SuitGraphicsContainer Suit
    {
        get
        {
            if (suit == null)
                Initialize(currentSuitGraphics, currentDeckGraphics);
            return suit;
        }
    }
    [field: SerializeField] public string ID { get; private set; }
    public LocalizedString suitName;
    public int suitPriority;
    public string suitLetter;
    public void Initialize(SuitGraphics suitGraphics, DeckGraphics currentDeckGraphics) 
    {
        suit = new SuitGraphicsContainer();
        suit.SuitGraphic = suitGraphics.GetSuitGraphics(this);
        suit.DeckGraphic = currentDeckGraphics.deckGraphicsSprite;
    }
#if UNITY_EDITOR
    [ContextMenu("Generate ID")]
    public void GenerateID()
    {
        var tableEntry = UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetTableEntry(
            suitName.TableReference,
            suitName.TableEntryReference
        );
        ID = $"Suit_{tableEntry.Entry.SharedEntry.Key}_{suitLetter}_{suitPriority}";
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
