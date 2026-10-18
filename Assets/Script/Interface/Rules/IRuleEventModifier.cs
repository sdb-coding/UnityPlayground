using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public abstract class IRuleEventModifier
{
    public event Action onActivation;
    public abstract void Init(object variable);
    public abstract IRuleEventModifier Clone();
}
public static class IRuleEventModifierExtention
{
    public static List<IRuleEventModifier> GetClones(this List<IRuleEventModifier> list)
    {
        System.Collections.Generic.List<IRuleEventModifier> clones = new System.Collections.Generic.List<IRuleEventModifier>(list.Count);
        foreach (var item in list)
        {
            clones.Add(item.Clone());
        }
        return clones;
    }
}
public interface IRuleOnBeforeScoring
{
    void OnBeforeScore(ScoreModel.ScoringResult currentScore);
}
public interface IRuleOnScoring
{ 
    void OnScoring(ScoreModel.ScoringResult currentScore);
}
public interface IRuleOnAfterScoring
{
    void OnAfterScoring(ScoreModel.ScoringResult currentScore);
}
public interface IRuleOnScoringCard
{ 
    void OnScoringCard(ScoreModel.ScoringResult currentScore, PlayingCardModel playingCardModel);
}


