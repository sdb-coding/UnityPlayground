using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class PlayingCardPresenter : MonoBehaviour, IDraggable, IBeginDrag, IEndDrag, ICardPresenter, IModelContainer/*, IHoverable, IBeginClickDown*/
{
    #region IModelContainer

    public object GetModel() => currentCardModel;

    public void ModelRestored()
    {
        cardView.DisplayCard(currentCardModel);
        currentCardModel.suitBlueprint.Suit.onSuitGraphicsChanged += SetupSuitGraphics;
    }
    #endregion
    public PlayingCardView cardView;
    public Collider2D cardCollider;
    
    [Header("Settings")]
    public float destructionhScaleTime = .5f;
    #region ICardPresenter
    public MonoBehaviour monoBehaviour { get => this; }
    public Transform Transform { get => transform; }
    [NonSerialized] public PlayingCardModel currentCardModel;
    public ICardPresenterHolder currentCardPresenterHolder { get; set; }
    public ICardModel PresentedModel { get => currentCardModel; }
    #endregion
    
    #region IDraggable
    public GameObject GameObject => gameObject;
    public DraggableType DraggableType => draggableType;

    public CardType cardType { get; }
    #endregion

    public DraggableType draggableType;

    public void DisplayCard(ICardModel card)
    {
        if (card is PlayingCardModel cardModel)
        {
            currentCardModel = cardModel;
            cardView.DisplayCard(cardModel);
            cardModel.suitBlueprint.Suit.onSuitGraphicsChanged += SetupSuitGraphics;
        }
    }
    public int UpdateSortingOrder(int baseValue)
    {
        return cardView.UpdateSortingOrder(baseValue);
    }

    private void OnDisable()
    {
        if (currentCardModel != null)
            currentCardModel.suitBlueprint.Suit.onSuitGraphicsChanged -= SetupSuitGraphics;
    }
    void SetupSuitGraphics(SuitBlueprint.SuitGraphicsContainer suit)
    {
        cardView.SetupSuitGraphics(suit, currentCardModel.CardValue);
    }
    public void DeleteCard(bool runGraphics = true)
    {
        if (runGraphics)
            StartCoroutine(DestroyGraphics());
        else
            CardViewFactory.Instance.DeleteCard(currentCardModel.cardType, this);
    }
    IEnumerator DestroyGraphics()
    {
        transform.SetParent(null);
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = Vector3.zero;
        float progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime / destructionhScaleTime;

            transform.localScale = Vector3.Lerp(initialScale, targetScale, progress);

            yield return null; 
        }
        var explosionParticles = CardDestructionFactory.Instance.GetDestructionParticleSystem();
        explosionParticles.transform.position = transform.position;
        explosionParticles.Play();
        transform.localScale = initialScale;
        CardViewFactory.Instance.DeleteCard(currentCardModel.cardType, this);
    }

    public void BeginDrag()
    {
        cardCollider.enabled = false;
    }

    public void EndDrag()
    {
        cardCollider.enabled = true;
    }
}
public static class PlayingCardPresenterExtention
{
    public static PlayingCardModel[] GetCardModels(this PlayingCardPresenter[] array)
    {
        PlayingCardModel[] cardModels = new PlayingCardModel[array.Length];
        for (int i = 0; i <cardModels.Length; i++)
        {
            cardModels[i] = array[i].currentCardModel;
        }
        return cardModels;
    }
    public static List<PlayingCardModel> GetCardModelsList(this PlayingCardPresenter[] array)
    {
        System.Collections.Generic.List<PlayingCardModel> cardModels = new System.Collections.Generic.List<PlayingCardModel>(array.Length);
        foreach (var item in array)
        {
            cardModels.Add(item.currentCardModel);
        }
        return cardModels;
    }
    private static readonly ValueComparer ValueSortComparer = new ValueComparer();
    private static readonly SuitComparer SuitSortComparer = new SuitComparer();

    public static void SortByValue(this List<PlayingCardPresenter> cards)
    {
        if (cards == null || cards.Count <= 1) return;
        cards.Sort(ValueSortComparer);
    }
    public static void SortBySuit(this List<PlayingCardPresenter> cards)
    {
        if (cards == null || cards.Count <= 1) return;
        cards.Sort(SuitSortComparer);
    }

    public static int GetSortWeight(PlayingCardPresenter card)
    {
        return card.currentCardModel.CardValue == 1 ? 14 : card.currentCardModel.CardValue;
    }

    // --- Custom Comparers ---

    private struct ValueComparer : IComparer<PlayingCardPresenter>
    {
        public int Compare(PlayingCardPresenter x, PlayingCardPresenter y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            int valueCompare = GetSortWeight(y).CompareTo(GetSortWeight(x));
            if (valueCompare != 0) return valueCompare;

            return y.currentCardModel.suitBlueprint.suitPriority.CompareTo(x.currentCardModel.suitBlueprint.suitPriority);
        }
    }

    private struct SuitComparer : IComparer<PlayingCardPresenter>
    {
        public int Compare(PlayingCardPresenter x, PlayingCardPresenter y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            int suitCompare = y.currentCardModel.suitBlueprint.suitPriority.CompareTo(x.currentCardModel.suitBlueprint.suitPriority);
            if (suitCompare != 0) return suitCompare;

            return GetSortWeight(y).CompareTo(GetSortWeight(x));
        }
    }
}
