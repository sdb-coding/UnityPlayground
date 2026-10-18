using UnityEngine;
using System.Collections.Generic;

public interface ICardPresenter
{
    public MonoBehaviour monoBehaviour { get; }
    public Transform Transform { get; }
    public ICardModel PresentedModel { get; }
    public void DisplayCard(ICardModel card);
    public void DeleteCard(bool runGraphics = true);
    public int UpdateSortingOrder(int sortingOrder);
    public ICardPresenterHolder currentCardPresenterHolder { get; set; }

}
public static class ICardPresenterExtention
{
    public static PlayingCardModel[] GetCardModels(this ICardPresenter[] array)
    {
        PlayingCardModel[] cardModels = new PlayingCardModel[array.Length];
        for (int i = 0; i < cardModels.Length; i++)
        {
            if (array[i] is PlayingCardPresenter playingCardPresenter)
                cardModels[i] = playingCardPresenter.currentCardModel;
        }
        return cardModels;
    }
    public static List<PlayingCardModel> GetCardModelsList(this ICardPresenter[] array)
    {
        List<PlayingCardModel> cardModels = new List<PlayingCardModel>(array.Length);
        foreach (var item in array)
        {
            if (item is PlayingCardPresenter playingCardPresenter)
                cardModels.Add(playingCardPresenter.currentCardModel);
        }
        return cardModels;
    }
    public static bool TryGetPlayingCardModel(this ICardPresenter[] array, out List<PlayingCardModel> playingCardModels)
    {
        playingCardModels = new List<PlayingCardModel>(array.Length);
        foreach (var item in array)
        {
            if (item is PlayingCardPresenter playingCardPresenter)
                playingCardModels.Add(playingCardPresenter.currentCardModel);
        }
        return playingCardModels.Count != 0;
    }
    public static PlayingCardPresenter[] GetPlayingCardPresenters(this ICardPresenter[] array)
    {
        PlayingCardPresenter[] cardModels = new PlayingCardPresenter[array.Length];
        for (int i = 0; i < cardModels.Length; i++)
        {
            if (array[i] is PlayingCardPresenter playingCardPresenter)
                cardModels[i] = playingCardPresenter;
        }
        return cardModels;
    }
    public static List<JokerModel> GetJokerModelList(this ICardPresenter[] array)
    {
        System.Collections.Generic.List<JokerModel> cardModels = new System.Collections.Generic.List<JokerModel>(array.Length);
        foreach (var item in array)
        {
            if (item is JokerCardPresenter jokerCardPresenter)
                cardModels.Add(jokerCardPresenter.currentJokerModel);
        }
        return cardModels;
    }
    public static bool TryGetIBuyables(this ICardPresenter[] array, out IBuyable[] iBuyables)
    {
        List<IBuyable> iBuyablesList = new List<IBuyable>(array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] is IBuyable iBuyable)
                iBuyablesList.Add(iBuyable);
        }
        if (iBuyablesList.Count != 0)
        {
            iBuyables = iBuyablesList.ToArray();
            return true;
        }
        iBuyables = null;
        return false;
    }

    private static readonly ValueComparer ValueSortComparer = new ValueComparer();
    private static readonly SuitComparer SuitSortComparer = new SuitComparer();

    public static void SortByValue(this List<ICardPresenter> cards)
    {
        if (cards == null || cards.Count <= 1) return;
        cards.Sort(ValueSortComparer);
    }
    public static void SortBySuit(this List<ICardPresenter> cards)
    {
        if (cards == null || cards.Count <= 1) return;
        cards.Sort(SuitSortComparer);
    }

    private struct ValueComparer : IComparer<ICardPresenter>
    {
        public int Compare(ICardPresenter x, ICardPresenter y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            if (x is PlayingCardPresenter xPlayingCardPresenter && y is PlayingCardPresenter yPlayingCardPresenter)
            {
                int valueCompare = PlayingCardPresenterExtention.GetSortWeight(yPlayingCardPresenter).CompareTo(PlayingCardPresenterExtention.GetSortWeight(xPlayingCardPresenter));
                if (valueCompare != 0) return valueCompare;

                return yPlayingCardPresenter.currentCardModel.suitBlueprint.suitPriority.CompareTo(xPlayingCardPresenter.currentCardModel.suitBlueprint.suitPriority);
            }
            return 0;
        }
    }

    private struct SuitComparer : IComparer<ICardPresenter>
    {
        public int Compare(ICardPresenter x, ICardPresenter y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            if (x is PlayingCardPresenter xPlayingCardPresenter && y is PlayingCardPresenter yPlayingCardPresenter)
            {
                int suitCompare = yPlayingCardPresenter.currentCardModel.suitBlueprint.suitPriority.CompareTo(xPlayingCardPresenter.currentCardModel.suitBlueprint.suitPriority);
                if (suitCompare != 0) return suitCompare;

                return PlayingCardPresenterExtention.GetSortWeight(yPlayingCardPresenter).CompareTo(PlayingCardPresenterExtention.GetSortWeight(xPlayingCardPresenter));
            }
            return 0;
        }
    }
}
