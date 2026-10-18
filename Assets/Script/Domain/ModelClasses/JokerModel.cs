using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;

public class JokerModel : ICardModel, IBuyable
{
    public JokerModel()
    {
    }
    public JokerModel(JokerBlueprint jokerBlueprint)
    {
        Guid = System.Guid.NewGuid().ToString();
        this.JokerBlueprint = jokerBlueprint;
        jokerBlueprintID = jokerBlueprint.ID;
        isInitialized = true;
    }
    public void SetupFromLoad()
    {
        if (isInitialized)
            return;
        JokerBlueprint = BlueprintBootstrap.GetBlueprint<JokerBlueprint>(jokerBlueprintID);
        isInitialized = true;
    }
    bool isInitialized;
    #region ICardModel
    string guid; 
    public string Guid
    {
        get => guid;
        set
        {
            guid = value;
            CardRegistryModel.RegisterModel(this);
        }
    }
    public event Action onActivation;
    [JsonIgnore] public CardType cardType => jokerBlueprint.cardType;
    public async Task<string> GetCardDescriptionAsync() => await JokerBlueprint.jokerDescription.GetLocalizedStringAsync().Task;
    public async Task<string> GetCardTitleAsync() => await JokerBlueprint.jokerName.GetLocalizedStringAsync().Task;
    public Rarity Rarity { get => JokerBlueprint.rarity; }
    public CardCondition[] cardConditions { get; } = new CardCondition[0];
#if UNITY_EDITOR
    public string GetCardTitleDebug() => JokerBlueprint.jokerName.GetLocalizedString();
#endif
#endregion //ICardModel

    public string jokerBlueprintID;

    [JsonIgnore] JokerBlueprint jokerBlueprint;
    [JsonIgnore] public JokerBlueprint JokerBlueprint
    {
        get => jokerBlueprint; 
        set
        {
            jokerBlueprint = value;
            Init(jokerBlueprint);
        }
    }

    [JsonIgnore] public List<IRuleEventModifier> ruleEvents;
    [JsonIgnore] public List<IRuleStateModifier> ruleStates;
    #region IBuyable
    [JsonIgnore] public StoreModel storeModel { get; set; }
    [SaveState] public int sellPriceChanges = 0;
    public int SellPrice { get => JokerBlueprint.startingSellPrice + sellPriceChanges; }
    public int BuyPrice { get => JokerBlueprint.startingBuyPrice; }
    public bool IsSellable { get; set; }
    public bool IsUnique { get; set; }
    

    public int GetBuyPrice(RulesModel rulesModel)
    {
        return rulesModel.GetBuyPriceJokerChanges(BuyPrice);
    }
    public int GetSellPrice(RulesModel rulesModel)
    {
        return rulesModel.GetSalesPriceJokerChanges(SellPrice);
    }

    public void SetupIBuyable(StoreModel storeModel)
    {
        IsSellable = false;
        this.storeModel = storeModel;
    }
    #endregion
    public void Init(JokerBlueprint jokerBlueprint)
    {
        if (ruleEvents != null)
        {
            foreach (IRuleEventModifier modifier in ruleEvents)
            {
                modifier.onActivation -= onActivation;
            }
        }
        ruleEvents = jokerBlueprint.ruleEvents.GetClones();
        for (int i = 0; i < jokerBlueprint.ruleEvents.Count; i++)
        {
            ruleEvents[i].Init(jokerBlueprint.eventValues[i]);
            ruleEvents[i].onActivation += onActivation;
        }
        ruleStates = jokerBlueprint.ruleStates.GetClones();
        for (int i = 0; i < ruleStates.Count; i++)
        {
            ruleStates[i].Init(jokerBlueprint.stateValues[i]);
        }
    }
    
    public void ActivateJoker(RulesModel rulesModel, bool rebuildBlueprint)
    {
        foreach (var effect in ruleStates)
        {
            effect.Register(rulesModel);
        }
        rulesModel.AddRuleEventModifier(ruleEvents, rebuildBlueprint);

        rulesModel.RebuildPipelines();
    }

    public void DeactivateJoker(RulesModel rulesModel, bool rebuildBlueprint)
    {
        foreach (var effect in ruleStates)
        {
            effect.Unregister(rulesModel);
        }
        rulesModel.RemoveRuleEventModfier(ruleEvents, rebuildBlueprint);

        rulesModel.RebuildPipelines();
    }
}