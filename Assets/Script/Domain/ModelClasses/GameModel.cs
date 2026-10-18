using UnityEngine;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

public class GameModel : IDisposable
{
    public GameModel()
    {
        CardRegistryModel.Bind(this);
        Debug.Log("Binding gameModel");
    }
    public GameModel(GameBlueprint gameBlueprint)
    {
        Debug.Log("Binding gameModel;");
        CardRegistryModel.Bind(this);
        this.gameBlueprint = gameBlueprint;
        gameBlueprintID = gameBlueprint.ID;
        //Setup Models
        rulesModel = new RulesModel(gameBlueprint.rulesBlueprint);
        jokerHandModel = new JokerHandModel();
        deckModel = new DeckModel(gameBlueprint.cardCollection.GetCardModels());
        playerHandModel = new PlayerHandModel();
        scoreModel = new ScoreModel(gameBlueprint.scoreBlueprint.scoreSettings);
        inventoryModel = new InventoryModel(gameBlueprint.inventoryBlueprint);
        //Setup ref
        currentAnte = new RefEvent<int>(0);
        numberOfDrawsInCurrentAnte = new RefEvent<int>(0);
        anteModel = new AnteModel(this);

        RunContext.Bind(this);
    }
    public void SetupFromLoad()
    {
        gameBlueprint = BlueprintBootstrap.GetBlueprint<GameBlueprint>(gameBlueprintID);
        rulesModel.rulesBlueprint = BlueprintBootstrap.GetBlueprint<RulesBlueprint>(rulesModel.rulesBlueprintID);
        anteModel.SetupAfterLoad();
        foreach (var card in cardRegistry)
            card.Value.SetupFromLoad();

        RunContext.Bind(this);
    }
    public void Dispose()
    {
        Debug.Log("Unbinding gameModel;");
        CardRegistryModel.Unbind(this);
        RunContext.Unbind(this);
    }

    //Events
    public Action onAnteComplete;
    public Action onStoreComplete;
    public Action onGameOver;
    public Action<PotScorePayload> potUpdatedScore;
    public Action nextRoundRequested;
    public Action nextRoundHappening;

    [SaveState] public string gameBlueprintID;
    [JsonIgnore] GameBlueprint gameBlueprint;

    //Blueprints
    public StoreBlueprint storeBlueprint => gameBlueprint.storeBlueprint;
    public CardCollection cardCollection => gameBlueprint.cardCollection;
    public AnteTargetScoreBlueprint anteScoreBlueprint => gameBlueprint.anteScoreBlueprint;

    //Models
    // -- NonSerialized
    [JsonIgnore] public ScoreModel scoreModel;
    public RulesModel rulesModel;

    //-- Serialized
    [SaveState] public InventoryModel inventoryModel;
    [SaveState] public PlayerHandModel playerHandModel;
    [SaveState] public DeckModel deckModel;
    [SaveState] public JokerHandModel jokerHandModel;
    [SaveState] public AnteModel anteModel;

    // Current ante values
    [SaveState] public RefEvent<int> currentAnte;
    [SaveState] public RefEvent<int> numberOfDrawsInCurrentAnte;

    [SaveState] public GameState currentGameState;
    [SaveState] public uint randomState;

    //Quick calls
    public int DrawsLeft() => rulesModel.GetMaxNumberOfDraws() - numberOfDrawsInCurrentAnte.Value;
    public bool CanDiscard() => RunContext.Current.rulesModel.rulesBlueprint.allowDiscardingOfCardsWhenNoDrawsAvailable || RunContext.Current.DrawsLeft() > 0;

    //Use cases
    public void StartNewAnte()
    {
        PrepNewAnte();
        anteModel.StartGame(anteScoreBlueprint.GetTargetScore(currentAnte));
    }
    public void PrepNewAnte()
    {
        deckModel.PrepNewAnte();
        deckModel.ShuffleDeck();
        DrawToFull();
    }
    public void DrawToFull()
    {
        var newCards = deckModel.DrawCardsToFull(playerHandModel.cardsInHand.Count);
        if (newCards != null)
            playerHandModel.AddCards(newCards);
    }

    #region CardRegistry
    [SaveState] public Dictionary<string, ICardModel> cardRegistry = new Dictionary<string, ICardModel>();
    public T GetModel<T>(string id) where T : class, ICardModel
    {
        if (!string.IsNullOrEmpty(id) && cardRegistry.TryGetValue(id, out var model))
        {
            return model as T;
        }
        return null;
    }
    public void RegisterModel(ICardModel cardModel)
    {
        if (!cardRegistry.ContainsKey(cardModel.Guid))
        {
            cardRegistry.Add(cardModel.Guid, cardModel);
        } else
        {
            Debug.LogError($"Tried to add two models with the same GUID: {cardModel.Guid} - {cardModel.GetCardTitleAsync()}");
        }
    }

    public void UnRegisterModel(ICardModel cardModel)
    {
        if (cardRegistry.ContainsKey(cardModel.Guid))
        {
            cardRegistry.Remove(cardModel.Guid);
        }
        else
        {
            Debug.LogError($"Tried to add two models with the same GUID: {cardModel.Guid} - {cardModel.GetCardTitleAsync()}");
        }
    }
    public void ClearCardRegistry()
    {
        cardRegistry.Clear();
    }
    #endregion
}
