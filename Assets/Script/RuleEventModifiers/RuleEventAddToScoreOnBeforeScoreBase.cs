using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public abstract class RuleEventAddToScoreOnBeforeScoreBase : IRuleEventModifier, IRuleOnBeforeScoring
{
    protected int value;

    public override IRuleEventModifier Clone()
    {
        return (IRuleEventModifier)this.MemberwiseClone();
    }

    public override void Init(object variable)
    {
        if (variable is int value)
            this.value = value;
    }
    public abstract void OnBeforeScore(ScoreModel.ScoringResult currentScore);
}
[Serializable]
public class RuleEventAddFlushChips : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.Flush) s.chipScore += value; } }

[Serializable]
public class RuleEventAddFlushMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.Flush) s.mulScore += value; } }

[Serializable]
public class RuleEventAddFlushCardMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.Flush) s.cardMulScore += value; } }

[Serializable]
public class RuleEventAddStraightChips : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.Straight) s.chipScore += value; } }

[Serializable]
public class RuleEventAddStraightMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.Straight) s.mulScore += value; } }

[Serializable]
public class RuleEventAddStraightCardMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.Straight) s.cardMulScore += value; } }

#region HighCard
[Serializable]
public class RuleEventAddHighCardChips : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.HighCard) s.chipScore += value; } }

[Serializable]
public class RuleEventAddHighCardMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.HighCard) s.mulScore += value; } }

[Serializable]
public class RuleEventAddHighCardCardMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.HighCard) s.cardMulScore += value; } }
#endregion

#region Pair
[Serializable]
public class RuleEventAddPairChips : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.Pair) s.chipScore += value; } }

[Serializable]
public class RuleEventAddPairMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.Pair) s.mulScore += value; } }

[Serializable]
public class RuleEventAddPairCardMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.Pair) s.cardMulScore += value; } }
#endregion

#region TwoPairs
[Serializable]
public class RuleEventAddTwoPairsChips : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.TwoPairs) s.chipScore += value; } }

[Serializable]
public class RuleEventAddTwoPairsMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.TwoPairs) s.mulScore += value; } }

[Serializable]
public class RuleEventAddTwoPairsCardMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.TwoPairs) s.cardMulScore += value; } }
#endregion

#region ThreeOfAKind
[Serializable]
public class RuleEventAddThreeOfAKindChips : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.ThreeOfAKind) s.chipScore += value; } }

[Serializable]
public class RuleEventAddThreeOfAKindMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.ThreeOfAKind) s.mulScore += value; } }

[Serializable]
public class RuleEventAddThreeOfAKindCardMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.ThreeOfAKind) s.cardMulScore += value; } }
#endregion

#region FourOfAKind
[Serializable]
public class RuleEventAddFourOfAKindChips : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.FourOfAKind) s.chipScore += value; } }

[Serializable]
public class RuleEventAddFourOfAKindMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.FourOfAKind) s.mulScore += value; } }

[Serializable]
public class RuleEventAddFourOfAKindCardMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.FourOfAKind) s.cardMulScore += value; } }
#endregion

#region FiveOfAKind
[Serializable]
public class RuleEventAddFiveOfAKindChips : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.FiveOfAKind) s.chipScore += value; } }

[Serializable]
public class RuleEventAddFiveOfAKindMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.FiveOfAKind) s.mulScore += value; } }

[Serializable]
public class RuleEventAddFiveOfAKindCardMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.FiveOfAKind) s.cardMulScore += value; } }
#endregion

#region FullHouse
[Serializable]
public class RuleEventAddFullHouseChips : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.FullHouse) s.chipScore += value; } }

[Serializable]
public class RuleEventAddFullHouseMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.FullHouse) s.mulScore += value; } }

[Serializable]
public class RuleEventAddFullHouseCardMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.FullHouse) s.cardMulScore += value; } }
#endregion

#region StraightFlush
[Serializable]
public class RuleEventAddStraightFlushChips : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.StraightFlush) s.chipScore += value; } }

[Serializable]
public class RuleEventAddStraightFlushMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.StraightFlush) s.mulScore += value; } }

[Serializable]
public class RuleEventAddStraightFlushCardMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.StraightFlush) s.cardMulScore += value; } }
#endregion

#region RoyalFlush
[Serializable]
public class RuleEventAddRoyalFlushChips : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.RoyalFlush) s.chipScore += value; } }

[Serializable]
public class RuleEventAddRoyalFlushMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.RoyalFlush) s.mulScore += value; } }

[Serializable]
public class RuleEventAddRoyalFlushCardMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.RoyalFlush) s.cardMulScore += value; } }
#endregion

#region FlushHouse
[Serializable]
public class RuleEventAddFlushHouseChips : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.FlushHouse) s.chipScore += value; } }

[Serializable]
public class RuleEventAddFlushHouseMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.FlushHouse) s.mulScore += value; } }

[Serializable]
public class RuleEventAddFlushHouseCardMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.FlushHouse) s.cardMulScore += value; } }
#endregion

#region FlushFive
[Serializable]
public class RuleEventAddFlushFiveChips : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.FlushFive) s.chipScore += value; } }

[Serializable]
public class RuleEventAddFlushFiveMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.FlushFive) s.mulScore += value; } }

[Serializable]
public class RuleEventAddFlushFiveCardMult : RuleEventAddToScoreOnBeforeScoreBase
{ public override void OnBeforeScore(ScoreModel.ScoringResult s) { if (s.scoringType.scoreType == ScoreType.FlushFive) s.cardMulScore += value; } }
#endregion

