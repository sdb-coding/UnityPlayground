using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;

[Serializable]
public class StoreModel : IDisposable
{
    public StoreModel()
    {

    }
    public StoreModel(StoreBlueprint storeBlueprint)
    {
        cardsForSale = new List<string>();
        cachedCardsForSale = new List<IBuyable>();
        this.storeBlueprint = storeBlueprint;
    }
    public void Dispose()
    {
        if (findRandomStoreCardsCancellationTokenSource != null)
        {
            findRandomStoreCardsCancellationTokenSource.Cancel();
            findRandomStoreCardsCancellationTokenSource.Dispose();
        }
    }
    public event Action<IBuyable[]> onBuyableAdded;
    public event Action<IBuyable[]> onBuyableRemoved;
    [SaveState] public List<string> cardsForSale;
    [JsonIgnore] public List<IBuyable> cachedCardsForSale;
    StoreBlueprint storeBlueprint;

    System.Threading.CancellationTokenSource findRandomStoreCardsCancellationTokenSource;
    
    public async void AddRandomBuyablesForSale(int numberOfBuyables)
    {
        findRandomStoreCardsCancellationTokenSource = new System.Threading.CancellationTokenSource();
        bool allowBuyingOfUniques = RunContext.Current.rulesModel.GetAllowBuyingOfUniques();
        
        try
        {
            IBuyable[] results = await Task.Run(() => GetRandomBuyables(findRandomStoreCardsCancellationTokenSource.Token, numberOfBuyables, allowBuyingOfUniques), findRandomStoreCardsCancellationTokenSource.Token);

            AddBuyable(results);
        }
        catch (OperationCanceledException)
        {
            // Generation was cancelled before completion
        }

    }
    IBuyable[] GetRandomBuyables(CancellationToken cancellationToken, int numberOfBuyables, bool onlyUniques)
    {
        HashSet<ScriptableObject> tempHashSet = new HashSet<ScriptableObject>();
        IBuyable[] newBuyables = new IBuyable[numberOfBuyables];
        for (int i = 0; i < numberOfBuyables; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            newBuyables[i] = GetRandomBuyable(cancellationToken, onlyUniques, tempHashSet);
        }
        return newBuyables;
    }
    IBuyable GetRandomBuyable(CancellationToken cancellationToken, bool onlyUniques, HashSet<ScriptableObject> tempHashSet)
    {
        var total = storeBlueprint.buyableJokerBlueprints.Length + storeBlueprint.buyablePlayingCards.Length;
        int random = RandomUtility.GetRandomInt(0, total);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (random >= storeBlueprint.buyableJokerBlueprints.Length)
            {
                if (!onlyUniques || IsValidUnique(storeBlueprint.buyablePlayingCards[random - storeBlueprint.buyableJokerBlueprints.Length], tempHashSet))
                    return new PlayingCardModel(storeBlueprint.buyablePlayingCards[random - storeBlueprint.buyableJokerBlueprints.Length]);
            }
            else
            {
                if (!onlyUniques || IsValidUnique(storeBlueprint.buyableJokerBlueprints[random], tempHashSet))
                    return new JokerModel(storeBlueprint.buyableJokerBlueprints[random]);
            }
        }
    }
    bool IsValidUnique(ScriptableObject asset, HashSet<ScriptableObject> localSelectedUniques)
    {
        return !storeBlueprint.uniqueObjects.Contains(asset) && !localSelectedUniques.Contains(asset);
    }
    public void AddBuyable(IBuyable[] buyables)
    {
        cardsForSale.AddRange(buyables.GetGUIDs());
        cachedCardsForSale.AddRange(buyables);
        foreach (var item in buyables)
        {
            item.SetupIBuyable(this);
        }
        if (onBuyableAdded != null)
            onBuyableAdded.Invoke(buyables);
    }
    public void RemoveBuyable(IBuyable[] buyables)
    {
        foreach (var card in buyables)
        {
            cardsForSale.Remove(card.Guid);
            cachedCardsForSale.Remove(card);
        }
        if (onBuyableRemoved != null)
            onBuyableRemoved.Invoke(buyables);
    }
    public bool CanBuyCard(IBuyable buyable)
    {
        if (buyable == null || buyable.GetBuyPrice(RunContext.Current.rulesModel) > RunContext.Current.inventoryModel.coins.Value)
            return false;
        return true;
    }
    public void BuyCard(IBuyable buyable)
    {
        if (CanBuyCard(buyable))
        {
            RunContext.Current.inventoryModel.coins.Value -= buyable.GetBuyPrice(RunContext.Current.rulesModel);
            RemoveBuyable(new IBuyable[] { buyable });
            buyable.IsSellable = true;
            buyable.storeModel = null;
        }
    }
}
