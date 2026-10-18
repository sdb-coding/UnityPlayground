using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class JokerHandPresenter : MonoBehaviour, ICardPresenterHolder
{
#if UNITY_EDITOR
    [CustomEditor(typeof(JokerHandPresenter))]
    public class JokerHandPresenter_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!Application.isPlaying)
                return;
            if (GUILayout.Button("Add debug cards"))
            {
                var myTarget = (JokerHandPresenter)target;
                var cardViewFactory = FindAnyObjectByType<CardViewFactory>(FindObjectsInactive.Include);

                foreach (var item in myTarget.debugJokerBlueprints)
                {
                    var jokerModel = new JokerModel(item);
                    myTarget.AddCard(cardViewFactory.GetCardPresenter(myTarget.cardType, jokerModel));

                }
            }
        }
    }
    public JokerBlueprint[] debugJokerBlueprints;
#endif
    public JokerHandModel DisplayedJokerHandModel { get; private set; }
    public CardType cardType;
    public CardHolderView cardHolderView;
    public IDropablePoint dropablePoint;
    RulesBlueprint ruleBlueprint;
    private void Start()
    {
        dropablePoint = GetComponent<IDropablePoint>();
    }
    void OnEnable()
    {
        RunContext.onRunContextBound += RunContextBound;
        if (RunContext.Current != null)
            RunContextBound(RunContext.Current);
    }
    void OnDisable()
    {
        RunContext.onRunContextBound -= RunContextBound;
    }
    void RunContextBound(GameModel gameModel)
    {
        if (gameModel.jokerHandModel != null)
            DisplayPlayerHand(gameModel.jokerHandModel);
    }
    public void DisplayPlayerHand(JokerHandModel jokerHandModel)
    {
        if (DisplayedJokerHandModel != null)
            ClearCurrentPotModelConnections();
        DisplayedJokerHandModel = jokerHandModel;

        DisplayedJokerHandModel.onJokersAdded += CardsAddedToModel;
        DisplayedJokerHandModel.onJokersRemoved += CardRemovedFromModel;
        foreach (var item in jokerHandModel.jokersInPlay)
        {
            var model = CardRegistryModel.GetModel<JokerModel>(item);
            var cardPresenter = CardViewFactory.Instance.GetCardPresenter(cardType, model);
            AddCardSilent(cardPresenter);
        }
    }
    void ClearCurrentPotModelConnections()
    {
        DisplayedJokerHandModel.onJokersRemoved -= CardRemovedFromModel;
        DisplayedJokerHandModel.onJokersAdded -= CardsAddedToModel;
    }
    void CardsClearedFromModel()
    {
        cardHolderView.DestroyCards();
    }
    void CardsAddedToModel(List<JokerModel> jokerModels)
    {
        List<JokerModel> newAdditions = new List<JokerModel>();
        foreach (var item in jokerModels)
        {
            if (!cardHolderView.ContainsCardModel(item.Guid))
                newAdditions.Add(item);
        }
        JokerCardPresenter[] newPresenters = new JokerCardPresenter[newAdditions.Count];
        for (int i = 0; i < newPresenters.Length; i++)
        {
            newPresenters[i] = CardViewFactory.Instance.GetCardPresenter(cardType, newAdditions[i]) as JokerCardPresenter;
            newPresenters[i].currentCardPresenterHolder = this;
        }

        cardHolderView.AddCardPresenterToHolder(newPresenters);
    }
    void CardRemovedFromModel(JokerModel cardModel)
    {
        cardHolderView.RemoveCardsFromHolder(cardModel.Guid);
    }
    #region ICardHolder
    public void RemoveCard(ICardPresenter cardPresenter)
    {
        if (cardPresenter.PresentedModel is JokerModel jokerModel)
            DisplayedJokerHandModel.RemoveCards(jokerModel);
    }
    public void RemoveCardSilent(ICardPresenter cardPresenter)
    {
        cardHolderView.RemoveCardsFromHolder(cardPresenter);
    }
    public void AddCard(ICardPresenter cardPresenter)
    {
        AddCardSilent(cardPresenter);

        DisplayedJokerHandModel.AddCards(new List<JokerModel>() { cardPresenter.PresentedModel as JokerModel});
        cardPresenter.currentCardPresenterHolder = this;
    }
    public void AddCards(ICardPresenter[] cardPresenters)
    {
        var cardModels = cardPresenters.GetCardModelsList();
        foreach (var item in cardPresenters) //TODO: Find a more elegant and less jank solution.
        {
            AddCardSilent(item);
        }
        DisplayedJokerHandModel.AddCards(cardPresenters.GetJokerModelList());
    }
    public void AddCardSilent(ICardPresenter cardPresenter)
    {
        cardHolderView.AddCardPresenterToHolder(cardPresenter);
        cardPresenter.currentCardPresenterHolder = this;
    }

    public bool ValidateCards(ICardPresenter[] cardPresenters)
    {
        foreach (var item in cardPresenters)
        {
            if (item is JokerCardPresenter jokerCardPresenter)
            {
                if ((ICardPresenterHolder)this == jokerCardPresenter.currentCardPresenterHolder) //Should maybe ask about the presenter instead of the model
                {
                    //TODO: Rearrange cards
                    return false;
                } else if (jokerCardPresenter.currentJokerModel.IsBuyable() && jokerCardPresenter.currentJokerModel.storeModel.CanBuyCard(jokerCardPresenter.currentJokerModel))
                {
                    jokerCardPresenter.currentJokerModel.storeModel.BuyCard(jokerCardPresenter.currentJokerModel);
                    return true;
                }
            } else 
                return false;
        }
        return true;
    }
    public bool IsMoveable(ICardPresenter cardPresenter)
    {
        return false;
    }
    public void MoveAttempted(ICardPresenter[] cardPresenter)
    {
        throw new System.NotImplementedException();
    }
    public void SortBy(SortType sortType)
    {
        cardHolderView.SortBy(sortType);
    }
    #endregion
}
