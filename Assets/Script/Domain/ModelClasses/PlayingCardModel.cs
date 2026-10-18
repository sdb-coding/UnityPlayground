using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

[Serializable]
public class PlayingCardModel : ICardModel, IBuyable
{
    public PlayingCardModel()
    {
    }
    public PlayingCardModel(PlayingCardBlueprint cardBlueprint)
    {
        SetPlayingCardBlueprint(cardBlueprint);
        Guid = System.Guid.NewGuid().ToString();
        suitBlueprint = cardBlueprint.suitBlueprint;
        suitBlueprintID = cardBlueprint.suitBlueprint.ID;
        isInitialized = true;
    }
    public void SetupFromLoad()
    {
        if (isInitialized)
            return;
        SetPlayingCardBlueprint(BlueprintBootstrap.GetBlueprint<PlayingCardBlueprint>(playingCardBlueprintID));
        suitBlueprint = BlueprintBootstrap.GetBlueprint<SuitBlueprint>(suitBlueprintID);
        isInitialized = true;
    }
    bool isInitialized;
    #region ICardModel
    string guid;
    [SaveState][JsonProperty] public string Guid 
    {
        get => guid;
        private set
        {
            guid = value;
            CardRegistryModel.RegisterModel(this);
        }
    }
    public event Action onActivation;
    [JsonIgnore] public CardType cardType => playingCardBlueprint.cardType; 
    public async Task<string> GetCardDescriptionAsync() => await playingCardBlueprint.cardDescription.GetLocalizedStringAsync().Task;
    public async Task<string> GetCardTitleAsync() => await playingCardBlueprint.GetCardTitleAsync();
    public CardCondition[] cardConditions { get; } = new CardCondition[0];
    [JsonIgnore] public Rarity Rarity { get => playingCardBlueprint.rarity; }
#if UNITY_EDITOR
    public string GetCardTitleDebug() => playingCardBlueprint.GetCardTitleDebug();
#endif 
    #endregion

    [SaveState] public int ScoringValueChanges { get; set; }
    public int CardValue => playingCardBlueprint.value;
    public string suitBlueprintID;
    [JsonIgnore][NonSerialized] public SuitBlueprint suitBlueprint;
    public string playingCardBlueprintID;
    [JsonIgnore] PlayingCardBlueprint playingCardBlueprint;
    
    #region IBuyable
    public StoreModel storeModel { get; set; }
    [SaveState] public int sellPriceChanges = 0;
    private int SellPrice { get => playingCardBlueprint.startingSellPrice + sellPriceChanges; }
    private int BuyPrice { get => playingCardBlueprint.startingBuyPrice; }
    public bool IsSellable { get; set; }
    public bool IsUnique { get; set; }

    public int GetBuyPrice(RulesModel rulesModel)
    {
        return rulesModel.GetBuyPricePlayingCardChanges(BuyPrice);
    }
    public int GetSellPrice(RulesModel rulesModel)
    {
        return rulesModel.GetSalesPricePlayingCardChanges(SellPrice);
    }

    public void SetupIBuyable(StoreModel storeModel)
    {
        IsSellable = false;
        this.storeModel = storeModel;
    }
    #endregion

    public int GetScoringValue(ScoreModel scoreModel)
    {
        return scoreModel.GetBaseCardValue(CardValue) + ScoringValueChanges;
    }
    public string GetNiceName()
    {
        return suitBlueprint.suitLetter + GetValueIndicatorFromValue(CardValue);
    }
    public void SetPlayingCardBlueprint(PlayingCardBlueprint cardBlueprint)
    {
        playingCardBlueprint = cardBlueprint;
        playingCardBlueprintID = cardBlueprint.ID;
    }
    public static string GetValueIndicatorFromValue(int value)
    {
        switch (value)
        {
            case 1:
                return "A";
            case 11:
                return "J";
            case 12:
                return "Q";
            case 13:
                return "K";
            default:
                return value.ToString();
        }
    }
    public static int GetSortWeight(PlayingCardModel card)
    {
        return card.CardValue == 1 ? 14 : card.CardValue;
    }
    public override string ToString() => GetNiceName();
}
public static class CardModelExtention
{
    public static PlayingCardModel[] GetRandomCards(this PlayingCardModel[] array, int numberOfCards)
    {
        PlayingCardModel[] result = new PlayingCardModel[numberOfCards];
        PlayingCardModel[] clone = (PlayingCardModel[])array.Clone();

        for (int i = 0; i < numberOfCards; i++)
        {
            int randomIndex = RandomUtility.GetRandomInt(i, clone.Length);

            result[i] = clone[randomIndex];

            clone[randomIndex] = clone[i];
        }

        return result;
    }
    
    private static readonly ValueComparer ValueSortComparer = new ValueComparer();
    private static readonly SuitComparer SuitSortComparer = new SuitComparer();

    public static void SortByValue(this PlayingCardModel[] cards)
    {
        if (cards == null || cards.Length <= 1) return;
        Array.Sort(cards, ValueSortComparer);
    }
    public static void SortBySuit(this PlayingCardModel[] cards)
    {
        if (cards == null || cards.Length <= 1) return;
        Array.Sort(cards, SuitSortComparer);
    }
    public static void SortByValue(this List<PlayingCardModel> cards)
    {
        if (cards == null || cards.Count <= 1) return;
        cards.Sort(ValueSortComparer);
    }
    public static void SortBySuit(this List<PlayingCardModel> cards)
    {
        if (cards == null || cards.Count <= 1) return;
        cards.Sort(SuitSortComparer);
    }
    // --- Custom Comparers ---

    private struct ValueComparer : IComparer<PlayingCardModel>
    {
        public int Compare(PlayingCardModel x, PlayingCardModel y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            // 1. Compare Card Values (Descending order: Higher weight comes first)
            int valueCompare = PlayingCardModel.GetSortWeight(y).CompareTo(PlayingCardModel.GetSortWeight(x));
            if (valueCompare != 0) return valueCompare;

            // 2. Tie-breaker: Suit Priority (Descending order: Higher priority comes first)
            return y.suitBlueprint.suitPriority.CompareTo(x.suitBlueprint.suitPriority);
        }
    }

    private struct SuitComparer : IComparer<PlayingCardModel>
    {
        public int Compare(PlayingCardModel x, PlayingCardModel y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            // 1. Compare Suit Priorities (Descending order)
            int suitCompare = y.suitBlueprint.suitPriority.CompareTo(x.suitBlueprint.suitPriority);
            if (suitCompare != 0) return suitCompare;

            // 2. Tie-breaker: Card Values (Descending order)
            return PlayingCardModel.GetSortWeight(y).CompareTo(PlayingCardModel.GetSortWeight(x));
        }
    }
}
