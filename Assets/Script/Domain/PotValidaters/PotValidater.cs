using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class PotValidater
{
    public static ScoreModel.ScoringType ValidatePot(List<PlayingCardModel> cardModels, RulesModel rulesModel)
    {
        if (cardModels.Count == 1 && rulesModel.GetRequiredCardsForMatches() >= 1)
            return new ScoreModel.ScoringType(ScoreType.HighCard, cardModels);
        return EvaluateScoreTypeLinQ(cardModels, rulesModel);
    }
    private static readonly int[] RanksBuffer = new int[5];

    public static ScoreModel.ScoringType EvaluateScoreTypeLinQ(List<PlayingCardModel> cards, RulesModel rulesModel)
    {
        if (cards == null || cards.Count == 0)
            return new ScoreModel.ScoringType(ScoreType.Nothing, new List<PlayingCardModel>());

        int count = cards.Count;

        // 1. Check Flush
        bool isFlush = rulesModel.GetRequiredCardsForFlush() <= count;
        if (isFlush)
        {
            SuitBlueprint firstSuit = cards[0].suitBlueprint;
            for (int i = 1; i < count; i++)
            {
                if (cards[i].suitBlueprint != firstSuit)
                {
                    isFlush = false;
                    break;
                }
            }
        }

        // 2. Populate and sort RanksBuffer (Insertion Sort on elements)
        for (int i = 0; i < count; i++)
        {
            RanksBuffer[i] = PlayingCardModel.GetSortWeight(cards[i]);
        }

        InsertionSort(RanksBuffer, count);

        // 3. Group cards by Rank/Sort Weight to easily slice exact scoring cards
        var rankGroups = cards
            .GroupBy(PlayingCardModel.GetSortWeight)
            .OrderByDescending(g => g.Count())
            .ThenByDescending(g => g.Key)
            .ToList();

        int maxFrequency = rankGroups.Count > 0 ? rankGroups[0].Count() : 0;
        int secondMaxFrequency = rankGroups.Count > 1 ? rankGroups[1].Count() : 0;

        bool hasFiveOfAKind = maxFrequency == 5;
        bool hasFourOfAKind = maxFrequency == 4;
        bool hasFullHouse = maxFrequency == 3 && secondMaxFrequency == 2;
        bool hasThreeOfAKind = maxFrequency == 3;
        bool hasTwoPair = maxFrequency == 2 && secondMaxFrequency == 2;
        bool hasPair = maxFrequency == 2;

        // 4. Balatro Unique Hands (All cards count towards Flush Five / Flush House / Five of a Kind)
        if (hasFiveOfAKind && isFlush) return new ScoreModel.ScoringType(ScoreType.FlushFive, cards);
        if (hasFullHouse && isFlush) return new ScoreModel.ScoringType(ScoreType.FlushHouse, cards);
        if (hasFiveOfAKind) return new ScoreModel.ScoringType(ScoreType.FiveOfAKind, cards);

        // 5. Check Straight (requires 5 cards)
        bool isStraight = false;
        if (rulesModel.GetRequiredCardsForStraight() <= count)
        {
            // Standard Straight: consecutive values
            bool isNormalStraight = true;
            for (int i = 0; i < 4; i++)
            {
                if (RanksBuffer[i + 1] - RanksBuffer[i] != 1)
                {
                    isNormalStraight = false;
                    break;
                }
            }

            // Ace-Low Straight (A-2-3-4-5) -> sorted buffer: [2, 3, 4, 5, 14]
            bool isAceLowStraight = (RanksBuffer[0] == 2 &&
                                     RanksBuffer[1] == 3 &&
                                     RanksBuffer[2] == 4 &&
                                     RanksBuffer[3] == 5 &&
                                     RanksBuffer[4] == 14);

            isStraight = isNormalStraight || isAceLowStraight;
        }

        // 6. Standard Hand Resolution (Returns ONLY scoring cards for sub-matches)
        if (isStraight && isFlush)
        {
            bool isRoyal = RanksBuffer[0] == 10;
            return isRoyal
                ? new ScoreModel.ScoringType(ScoreType.RoyalFlush, cards)
                : new ScoreModel.ScoringType(ScoreType.StraightFlush, cards);
        }

        // Four of a Kind: Only return the 4 matching cards
        if (hasFourOfAKind)
            return new ScoreModel.ScoringType(ScoreType.FourOfAKind, rankGroups[0].ToList());

        // Full House: All 5 cards (3 of a kind + Pair)
        if (hasFullHouse)
            return new ScoreModel.ScoringType(ScoreType.FullHouse, cards);

        // Flush & Straight: All cards forming the combination
        if (isFlush && count == 5)
            return new ScoreModel.ScoringType(ScoreType.Flush, cards);

        if (isStraight)
            return new ScoreModel.ScoringType(ScoreType.Straight, cards);

        // Three of a Kind: Only return the 3 matching cards
        if (hasThreeOfAKind)
            return new ScoreModel.ScoringType(ScoreType.ThreeOfAKind, rankGroups[0].ToList());

        // Two Pair: Return the 4 cards forming both pairs
        if (hasTwoPair)
        {
            List<PlayingCardModel> scoringCards = rankGroups[0].Concat(rankGroups[1]).ToList();
            return new ScoreModel.ScoringType(ScoreType.TwoPairs, scoringCards);
        }

        // Pair: Only return the 2 matching cards
        if (hasPair)
            return new ScoreModel.ScoringType(ScoreType.Pair, rankGroups[0].ToList());

        // High Card: Return only the highest value card
        PlayingCardModel highestCard = cards.OrderByDescending(PlayingCardModel.GetSortWeight).First();
        return new ScoreModel.ScoringType(ScoreType.HighCard, new List<PlayingCardModel> { highestCard });
    }

    private static void UpdateFrequencies(int freq, ref int max, ref int secondMax)
    {
        if (freq >= max)
        {
            secondMax = max;
            max = freq;
        }
        else if (freq > secondMax)
        {
            secondMax = freq;
        }
    }

    private static void InsertionSort(int[] array, int length)
    {
        for (int i = 1; i < length; i++)
        {
            int key = array[i];
            int j = i - 1;
            while (j >= 0 && array[j] > key)
            {
                array[j + 1] = array[j];
                j--;
            }
            array[j + 1] = key;
        }
    }

    //public ScoreModel.ScoringType ValidatePotSimple(List<PlayingCardModel> cardModels, RulesBlueprint rulesBlueprint)
    //{
    //    if (cardModels.Count == 1 && rulesBlueprint.GetRequiredCardsForMatches() >= 1)
    //        return new ScoreModel.ScoringType(ScoreType.HighCard, cardModels);

    //    List<PlayingCardModel> scoringCards = new List<PlayingCardModel>();
    //    if (CheckIfMatches(cardModels, rulesBlueprint, ref scoringCards))
    //    {
    //        switch (cardModels.Count)
    //        {
    //            case 2:
    //                return new ScoreModel.ScoringType(ScoreType.Pair, scoringCards);
    //            case 3:
    //                return new ScoreModel.ScoringType(ScoreType.ThreeOfAKind, scoringCards)
    //            case 4:
    //                return new ScoreModel.ScoringType(ScoreType.FourOfAKind, scoringCards)
    //            case 5:
    //                return new ScoreModel.ScoringType(ScoreType.FiveOfAKind, scoringCards)
    //            default:
    //                return new ScoreModel.ScoringType(ScoreType.Unknown, scoringCards)
    //        }
    //    }
    //    if (CheckIfSequential(cardModels, rulesBlueprint))
    //        return ScoreType.Straight;
    //    if (CheckIfSameSuit(cardModels, rulesBlueprint))
    //        return ScoreType.Flush;
    //    return ScoreType.Nothing;
    //}
    //bool CheckIfMatches(List<PlayingCardModel> cardModels, RulesBlueprint rulesBlueprint)
    //{
    //    if (cardModels.Count <= rulesBlueprint.GetRequiredCardsForMatches())
    //    {
    //        return false;
    //    }
    //    for (int i = 0; i < cardModels.Count - 1; i++)
    //    {
    //        if (cardModels[i].cardRank != cardModels[i + 1].cardRank)
    //        {
    //            return false;
    //        }
    //    }
    //    return true;
    //}
    //bool CheckIfSameSuit(List<PlayingCardModel> cardModels, RulesBlueprint rulesBlueprint)
    //{
    //    if (cardModels.Count <= rulesBlueprint.GetRequiredCardsForFlush())
    //    {
    //        return false;
    //    }
    //    for (int i = 0; i < cardModels.Count - 1; i++)
    //    {
    //        if (cardModels[i].suitBlueprint != cardModels[i + 1].suitBlueprint)
    //        {
    //            return false;
    //        }
    //    }
    //    return true;
    //}
    //bool CheckIfSequential(List<PlayingCardModel> cardModels, RulesBlueprint rulesBlueprint)
    //{
    //    if (cardModels.Count <= rulesBlueprint.GetRequiredCardsForStraight())
    //    {
    //        return false;
    //    }
    //    cardModels.SortByValue();
    //    if (cardModels[0].cardRank == 1) //we have an ace and need special attention
    //    {
    //        if (cardModels[1].cardRank == 13) //we have a king so we check downwards from queen.
    //        {
    //            int nextValue = 12;
    //            for (int i = 2; i < cardModels.Count; i++)
    //            {
    //                if (cardModels[i].cardRank != nextValue)
    //                    return false;
    //                nextValue--;
    //            }
    //            return true;
    //        } else //next wasn't a king so we assume we start from bottom 1, 2 etc
    //        {
    //            int nextValue = 2;
    //            for (int i = cardModels.Count - 1; i >= 1; i--)
    //            {
    //                if (cardModels[i].cardRank != nextValue)
    //                    return false;
    //                nextValue++;
    //            }
    //            return true;
    //        }
    //    }
    //    else //no ace present, so we just check from bottom up to see if we have a straight
    //    {
    //        for (int i = 0; i < cardModels.Count-1 ; i++)
    //        {
    //            int nextValue = cardModels[i].cardRank - 1;
    //            if (nextValue != cardModels[i + 1].cardRank)
    //            {
    //                return false;
    //            }
    //        }
    //    }
    //    return true;
    //}
    // Reusable fixed-size buffer to prevent heap allocations

    /// <summary>
    /// Evaluates the hand type without generating garbage (No LINQ).
    /// </summary>
    //public static ScoreModel.ScoringType EvaluateScoreType(List<PlayingCardModel> cards, RulesBlueprint rulesBlueprint)
    //{
    //    if (cards == null || cards.Count == 0)
    //        return new ScoreModel.ScoringType(ScoreType.Nothing, null);

    //    int count = cards.Count;

    //    // 1. Check Flush
    //    bool isFlush = rulesBlueprint.GetRequiredCardsForFlush() <= count;
    //    if (isFlush)
    //    {
    //        SuitBlueprint firstSuit = cards[0].suitBlueprint;
    //        for (int i = 1; i < count; i++)
    //        {
    //            if (cards[i].suitBlueprint != firstSuit)
    //            {
    //                isFlush = false;
    //                break;
    //            }
    //        }
    //    }

    //    // 2. Populate and sort RanksBuffer (Insertion Sort on 5 elements)
    //    for (int i = 0; i < count; i++)
    //    {
    //        RanksBuffer[i] = PlayingCardModel.GetSortWeight(cards[i]);
    //    }

    //    InsertionSort(RanksBuffer, count);

    //    // 3. Count group frequencies (e.g. 5 of a kind, 4 of a kind, pairs)
    //    int maxFrequency = 0;
    //    int secondMaxFrequency = 0;

    //    int currentFreq = 1;
    //    for (int i = 1; i < count; i++)
    //    {
    //        if (RanksBuffer[i] == RanksBuffer[i - 1])
    //        {
    //            currentFreq++;
    //        }
    //        else
    //        {
    //            UpdateFrequencies(currentFreq, ref maxFrequency, ref secondMaxFrequency);
    //            currentFreq = 1;
    //        }
    //    }
    //    UpdateFrequencies(currentFreq, ref maxFrequency, ref secondMaxFrequency);

    //    bool hasFiveOfAKind = maxFrequency == 5;
    //    bool hasFourOfAKind = maxFrequency == 4;
    //    bool hasFullHouse = maxFrequency == 3 && secondMaxFrequency == 2;
    //    bool hasThreeOfAKind = maxFrequency == 3;
    //    bool hasTwoPair = maxFrequency == 2 && secondMaxFrequency == 2;
    //    bool hasPair = maxFrequency == 2;

    //    // 4. Balatro Unique Hands
    //    if (hasFiveOfAKind && isFlush) return new ScoreModel.ScoringType(ScoreType.FlushFive, cards);
    //    if (hasFullHouse && isFlush) return new ScoreModel.ScoringType(ScoreType.FlushHouse, cards);
    //    if (hasFiveOfAKind) return new ScoreModel.ScoringType(ScoreType.FiveOfAKind, cards);

    //    // 5. Check Straight (requires 5 cards)
    //    bool isStraight = false;
    //    if (rulesBlueprint.GetRequiredCardsForStraight() <= count)
    //    {
    //        // Standard Straight: consecutive values (e.g., 2-3-4-5-6 or 10-J-Q-K-A)
    //        bool isNormalStraight = true;
    //        for (int i = 0; i < 4; i++)
    //        {
    //            if (RanksBuffer[i + 1] - RanksBuffer[i] != 1)
    //            {
    //                isNormalStraight = false;
    //                break;
    //            }
    //        }

    //        // Ace-Low Straight (A-2-3-4-5) -> sorted buffer: [2, 3, 4, 5, 14]
    //        bool isAceLowStraight = (RanksBuffer[0] == 2 &&
    //                                 RanksBuffer[1] == 3 &&
    //                                 RanksBuffer[2] == 4 &&
    //                                 RanksBuffer[3] == 5 &&
    //                                 RanksBuffer[4] == 14);

    //        isStraight = isNormalStraight || isAceLowStraight;
    //    }

    //    // 6. Standard Hand Resolution
    //    if (isStraight && isFlush)
    //    {
    //        // Royal Flush check: Ace-high straight where lowest card is 10
    //        bool isRoyal = RanksBuffer[0] == 10;
    //        return isRoyal ? new ScoreModel.ScoringType(ScoreType.RoyalFlush, cards) : new ScoreModel.ScoringType(ScoreType.StraightFlush, cards);
    //    }

    //    if (hasFourOfAKind) return new ScoreModel.ScoringType(ScoreType.FourOfAKind, );
    //    if (hasFullHouse) return new ScoreModel.ScoringType(ScoreType.FullHouse, cards);
    //    if (isFlush && count == 5) return new ScoreModel.ScoringType(ScoreType.Flush, cards);
    //    if (isStraight) return new ScoreModel.ScoringType(ScoreType.Straight, cards);
    //    if (hasThreeOfAKind) return new ScoreModel.ScoringType(ScoreType.ThreeOfAKind, );
    //    if (hasTwoPair) return new ScoreModel.ScoringType(ScoreType.TwoPairs, );
    //    if (hasPair) return new ScoreModel.ScoringType(ScoreType.Pair, );

    //    return new ScoreModel.ScoringType(ScoreType.HighCard, ;
    //}

}