using UnityEngine;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

public class PotModel
{
    [SaveState] public ScoreModel.ScoringType currentScoreType { get; private set; }
    //Callbacks
    public event Action<ScoreModel.ScoringResult> onNewPotScoreResult;
    public event Action<List<PlayingCardModel>> onCardsAdded;
    public event Action<List<PlayingCardModel>> onCardsRemoved;
    public event Action onCardsCleared;

    //CurrentState
    [SaveState] public ScoreModel.ScoringResult potResult;
    [SaveState] public List<string> cardsInPot;
    [JsonIgnore] public List<PlayingCardModel> potPlayingCardCache;

    [SaveState] public string guid;

    //Rules
    public bool cardsMoveable;

    ScoreModel.ScoringType cachedScoring;
    public PotModel()
    {
        cardsInPot = new List<string>();
        potPlayingCardCache = new List<PlayingCardModel>();
        potResult = new ScoreModel.ScoringResult(ScoreType.Nothing);
    }
    public PotModel(string guid)
    {
        cardsInPot = new List<string>();
        potPlayingCardCache = new List<PlayingCardModel>();
        potResult = new ScoreModel.ScoringResult(ScoreType.Nothing);
        this.guid = guid;
        RunContext.onRunContextBound += RuncontextBindChanged;
        if (RunContext.Current != null)
            RuncontextBindChanged(RunContext.Current);
    }
    void RuncontextBindChanged(GameModel gameModel)
    {
        RunContext.Current.nextRoundHappening += NextRoundHappening;
    }
    public void AddCards(List<PlayingCardModel> cards)
    {
        DestroyCurrentPotCards();
        cardsInPot.AddRange(cards.GetGUIDs());
        potPlayingCardCache.AddRange(cards);

        AfterAddingCards();
    }
    public void AddCards(PlayingCardModel[] cards)
    {
        DestroyCurrentPotCards();
        cardsInPot.AddRange(cards.GetGUIDs());
        potPlayingCardCache.AddRange(cards);

        AfterAddingCards();
    }
    void AfterAddingCards()
    {
        if (onCardsAdded != null)
            onCardsAdded(potPlayingCardCache); 
        currentScoreType = PotValidater.ValidatePot(potPlayingCardCache, RunContext.Current.rulesModel);
        ScorePot();
    }
    public void PopulatePlayingCardCache(List<PlayingCardModel> cards)
    {
        potPlayingCardCache = cards;
    }
    public void RemoveCards(List<PlayingCardModel> cards)
    {
        foreach (PlayingCardModel card in cards)
        {
            cardsInPot.Remove(card.Guid);
            potPlayingCardCache.Remove(card);
        }

        if (onCardsRemoved != null)
            onCardsRemoved(cards);
    }
    public void DestroyCurrentPotCards()
    {
        if (cardsInPot.Count != 0
            && onCardsCleared != null)
            onCardsCleared.Invoke();
        cardsInPot.Clear();
        potPlayingCardCache.Clear();
    }
    public void ScorePot()
    {
        potResult = ScoreCalculator.ScorePot(potPlayingCardCache, RunContext.Current.rulesModel, RunContext.Current.scoreModel, RunContext.Current.scoreModel.GetCurrentScore(currentScoreType));
        if (RunContext.Current.potUpdatedScore != null)
            RunContext.Current.potUpdatedScore(new PotScorePayload(guid, potResult));
        if (onNewPotScoreResult != null)
            onNewPotScoreResult(potResult);
    }

    public void NextRoundHappening()
    {
        if (RunContext.Current.rulesModel.rulesBlueprint.automaticallyScoreAndClearPots)
            DestroyCurrentPotCards();
    }

    public ScoreModel.ScoringType ValidateCards(List<PlayingCardModel> cardModels)
    {
        if (RunContext.Current.rulesModel.GetCanAddToExistingPots())
        {
            List<PlayingCardModel> tempPotList = new List<PlayingCardModel>(potPlayingCardCache);
            tempPotList.AddRange(cardModels);
            cachedScoring = PotValidater.ValidatePot(tempPotList, RunContext.Current.rulesModel);
            return cachedScoring;
        }
        else
        {
            cachedScoring = PotValidater.ValidatePot(cardModels, RunContext.Current.rulesModel);
            return cachedScoring;
        }
    }
}
