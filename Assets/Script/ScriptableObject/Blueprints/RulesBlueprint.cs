using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "RulesBlueprint", menuName = "Data/Blueprint/RulesBlueprint")]
public class RulesBlueprint : ScriptableObject, IBlueprint
{
    //Default values
    [Header("Scoring Rules")]
    public int defaultrequiredCardsForFlush = 5;
    public int defaultrequiredCardsForStraight = 5;
    public int defaultrequiredCardsForMatches = 1;
    [Header("Hand Rules")]
    public int defaultMaxNumberOfCardsInHand = 7;
    public int defaultMaxNumberOfJokers = 5;
    [Header("Deck Rules")]
    public int defaultMaxNumberOfdrawsAllowed = 3;
    [Header("Game Rules")]
    public int defaultNumberOfRounds = 3;
    [Header("Pot Rules")]
    public bool defaultCanAddToExistingPots = false;
    [Header("Store Rules")]
    public int defaultNumberOfCardsInStore = 3;
    public bool defaultBuyingAllowUniques = false;

    [Header("None Changeable Rules")]
    public bool automaticallyDrawCardsOnNewRound = true;
    public bool automaticallyScoreAndClearPots = true;
    public bool automaticallyDrawWhenDiscarding = true;
    public bool allowDiscardingOfCardsWhenNoDrawsAvailable = false;
    public bool allowNonScoringCardsInPot = true;

    [field: SerializeField] public string ID { get; private set; }
#if UNITY_EDITOR
    [ContextMenu("Generate ID")]
    public void GenerateID()
    {
        ID = $"RulesBlueprint_{name}";
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
