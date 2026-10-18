using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class RulesModel
{
    public delegate void MaxIntDelegate(ref int value);
    public delegate void MaxBoolDelegate(ref bool value);
    public RulesModel() { }
    public RulesModel(RulesBlueprint rulesBlueprint)
    {
        this.rulesBlueprint = rulesBlueprint;
        rulesBlueprintID = rulesBlueprint.ID;
    }

    //--- States ---
    public MaxIntDelegate maxNumberOfCardsInHand;
    public MaxIntDelegate maxNumberOfDraws;
    public MaxIntDelegate numberOfRounds;
    public MaxIntDelegate maxNumberOfJokers;
    public MaxBoolDelegate canAddToExitingPots;

    //Scoring
    public MaxIntDelegate requiredCardsForFlush;
    public MaxIntDelegate requiredCardsForStraight;
    public MaxIntDelegate requiredCardsForMatches;

    //StoreChanges
    public MaxIntDelegate numberOfCardsInStore;
    public MaxBoolDelegate allowBuyingOfUniques;
    public MaxIntDelegate playingCardBuyPrice;
    public MaxIntDelegate playingCardSellPrice;
    public MaxIntDelegate jokerCardBuyPrice;
    public MaxIntDelegate jokerCardSellPrice;

    //Events
    private readonly List<IRuleOnBeforeScoring> onBeforeScoringPipeline = new();
    private readonly List<IRuleOnScoring> onScoringPipeline = new();
    private readonly List<IRuleOnAfterScoring> onAfterScoringPipeline = new();
    public List<IRuleEventModifier> ruleEventModifiers = new List<IRuleEventModifier>();
    public RulesBlueprint rulesBlueprint;
    public string rulesBlueprintID;
    public void RebuildPipelines()
    {
        onBeforeScoringPipeline.Clear();
        onScoringPipeline.Clear();

        foreach (var ruleEventModifier in ruleEventModifiers)
        {
            if (ruleEventModifier is IRuleOnBeforeScoring ruleOnBeforeScoring) onBeforeScoringPipeline.Add(ruleOnBeforeScoring);
            if (ruleEventModifier is IRuleOnScoring ruleOnScoring) onScoringPipeline.Add(ruleOnScoring);
            if (ruleEventModifier is IRuleOnAfterScoring ruleOnAfterScoring) onAfterScoringPipeline.Add(ruleOnAfterScoring);
        }
    }
    //Rule Event modifiers
    public void AddRuleEventModifier(IRuleEventModifier ruleEventModifier, bool rebuildPipeline = true)
    {
        ruleEventModifiers.Add(ruleEventModifier);
        if (rebuildPipeline)
            RebuildPipelines();
    }
    public void AddRuleEventModifier(List<IRuleEventModifier> ruleEventModifier, bool rebuildPipeline = true)
    {
        foreach (var item in ruleEventModifier)
        {
            ruleEventModifiers.Add(item);
        }
        if (rebuildPipeline)
            RebuildPipelines();
    }
    public void RemoveRuleEventModfier(IRuleEventModifier ruleEventModifier, bool rebuildPipeline = true)
    {
        ruleEventModifiers.Remove(ruleEventModifier);
        if (rebuildPipeline)
            RebuildPipelines();
    }
    internal void RemoveRuleEventModfier(List<IRuleEventModifier> ruleEventModifier, bool rebuildPipeline = true)
    {
        foreach (var item in ruleEventModifier)
        {
            ruleEventModifiers.Remove(item);
        }
        if (rebuildPipeline)
            RebuildPipelines();
    }

    //Events
    internal void OnBeforeScoring(ScoreModel.ScoringResult currentScore)
    {
        for (int i = 0; i < onBeforeScoringPipeline.Count; i++)
        {
            onBeforeScoringPipeline[i].OnBeforeScore(currentScore);
        }
    }

    internal void OnScoring(ScoreModel.ScoringResult currentScore)
    {
        for (int i = 0; i < onScoringPipeline.Count; i++)
        {
            onScoringPipeline[i].OnScoring(currentScore);
        }
    }

    internal void OnAfterScoring(ScoreModel.ScoringResult currentScore)
    {
        for (int i = 0; i < onAfterScoringPipeline.Count; i++)
        {
            onAfterScoringPipeline[i].OnAfterScoring(currentScore);
        }
    }

    //States
    public int GetMaxNumberOfCardsInHand()
    {
        int maxNumberOfCardsInHand = rulesBlueprint.defaultMaxNumberOfCardsInHand;
        if (this.maxNumberOfCardsInHand != null)
            this.maxNumberOfCardsInHand(ref maxNumberOfCardsInHand);
        return maxNumberOfCardsInHand;
    }
    public int GetMaxNumberOfJokers()
    {
        int numberOfJokers = rulesBlueprint.defaultMaxNumberOfJokers;
        if (this.maxNumberOfJokers != null)
            this.maxNumberOfJokers(ref numberOfJokers);
        return numberOfJokers;
    }
    public int GetMaxNumberOfDraws()
    {
        int maxNumberOfDraws = rulesBlueprint.defaultMaxNumberOfdrawsAllowed;
        if (this.maxNumberOfDraws != null)
            this.maxNumberOfDraws(ref maxNumberOfDraws);
        return maxNumberOfDraws;
    }
    public int GetNumberOfRounds()
    {
        int numberOfRounds = rulesBlueprint.defaultNumberOfRounds;
        if (this.numberOfRounds != null)
            this.numberOfRounds(ref numberOfRounds);
        return numberOfRounds;
    }
    //Scoring
    public int GetRequiredCardsForFlush()
    {
        int requiredCards = rulesBlueprint.defaultrequiredCardsForFlush;
        if (this.requiredCardsForFlush != null)
            this.requiredCardsForFlush(ref requiredCards);
        return requiredCards;
    }
    public int GetRequiredCardsForStraight()
    {
        int requiredCards = rulesBlueprint.defaultrequiredCardsForStraight;
        if (this.requiredCardsForStraight != null)
            this.requiredCardsForStraight(ref requiredCards);
        return requiredCards;
    }
    public int GetRequiredCardsForMatches()
    {
        int requiredCards = rulesBlueprint.defaultrequiredCardsForMatches;
        if (this.requiredCardsForMatches != null)
            this.requiredCardsForMatches(ref requiredCards);
        return requiredCards;
    }
    //Pots
    public bool GetCanAddToExistingPots()
    {
        bool canAddToExistingPots = rulesBlueprint.defaultCanAddToExistingPots;
        if (this.canAddToExitingPots != null)
            this.canAddToExitingPots(ref canAddToExistingPots);
        return canAddToExistingPots;
    }
    //Store
    public int GetNumberOfCardsInStore()
    {
        int numberOfcardsInStore = rulesBlueprint.defaultNumberOfCardsInStore;
        if (this.numberOfCardsInStore != null)
            this.numberOfCardsInStore(ref numberOfcardsInStore);
        return numberOfcardsInStore;
    }
    public bool GetAllowBuyingOfUniques()
    {
        bool allowBuyingOfUniques = rulesBlueprint.defaultBuyingAllowUniques;
        if (this.allowBuyingOfUniques != null)
            this.allowBuyingOfUniques(ref allowBuyingOfUniques);
        return allowBuyingOfUniques;
    }
    public int GetBuyPricePlayingCardChanges(int currentCardPrice)
    {
        if (this.playingCardBuyPrice != null)
            this.playingCardBuyPrice(ref currentCardPrice);
        return currentCardPrice;
    }
    public int GetSalesPricePlayingCardChanges(int currentCardPrice)
    {
        if (this.playingCardSellPrice != null)
            this.playingCardSellPrice(ref currentCardPrice);
        return currentCardPrice;
    }
    public int GetBuyPriceJokerChanges(int currentCardPrice)
    {
        if (this.jokerCardBuyPrice != null)
            this.jokerCardBuyPrice(ref currentCardPrice);
        return currentCardPrice;
    }
    public int GetSalesPriceJokerChanges(int currentCardPrice)
    {
        if (this.jokerCardSellPrice != null)
            this.jokerCardSellPrice(ref currentCardPrice);
        return currentCardPrice;
    }
}
