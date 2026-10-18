using UnityEngine;
using System.Collections.Generic;
using System;

public class SelectableCard : MonoBehaviour, ISelectableCard
{
    public event Action<ISelectableCard, bool> onSelectionChanged;
    public CardType CardType { get => cardType; }
    public object AttactedElement { get; set; }
    public Transform selectionGraphicsContainer;
    public GameObject GameObject {get => gameObject;}

    [SerializeField] CardType cardType;
    ICardView cardView;
    void Start()
    {
        if (ISelectableCard.onNewSelectableCreated != null)
            ISelectableCard.onNewSelectableCreated(this);
        cardView = GetComponent<ICardView>();
    }
    public void OnDeselect()
    {
        if (onSelectionChanged != null)
            onSelectionChanged(this, false);
        cardView.onSortingOrderUpdated -= OnSortingOrderUpdated;
    }

    public void OnSelect()
    {
        if (onSelectionChanged != null)
            onSelectionChanged(this, true);
    }

    public void AttachSelectionGraphics(Transform transform)
    {
        AttactedElement = transform;
        transform.gameObject.SetActive(true);
        transform.SetParent(selectionGraphicsContainer);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        if (transform.TryGetComponent<SpriteRenderer>(out SpriteRenderer sprite))
        {
            sprite.sortingOrder = cardView.lowestSortingOrder - 1;
            cardView.onSortingOrderUpdated += OnSortingOrderUpdated;
        }
    }
    void OnSortingOrderUpdated()
    {
        if (AttactedElement is Transform transform)
            if (transform.TryGetComponent<SpriteRenderer>(out SpriteRenderer sprite))
                sprite.sortingOrder = cardView.lowestSortingOrder - 1;
    }
}
