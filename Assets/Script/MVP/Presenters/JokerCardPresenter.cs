using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class JokerCardPresenter : MonoBehaviour, IDraggable, IBeginDrag, IEndDrag, ICardPresenter, IModelContainer
{
    #region IModelContainer

    public object GetModel() => currentJokerModel;

    public void ModelRestored()
    {
        jokerCardView.DisplayCard(currentJokerModel);
        currentJokerModel.onActivation += CardModelActivated;
    }
    #endregion
    [NonSerialized] public JokerModel currentJokerModel;

    public JokerCardView jokerCardView;
    public Collider2D cardCollider;

    [Header("Settings")]
    public float destructionhScaleTime = .5f;

    #region CardPresenter
    public MonoBehaviour monoBehaviour { get => this; }
    public Transform Transform { get => transform; }
    public ICardModel PresentedModel { get => currentJokerModel; }
    public ICardPresenterHolder currentCardPresenterHolder { get; set; }
    #endregion

    #region IDraggable
    public GameObject GameObject => gameObject;
    public DraggableType DraggableType => draggableType;
    #endregion

    public DraggableType draggableType;

    Vector3 defaultScale;
    private void Start()
    {
        defaultScale = transform.localScale;
    }
    private void OnDisable()
    {
        transform.localScale = defaultScale;
    }
    public void DisplayCard(ICardModel card)
    {
        if (card is JokerModel jokerModel)
        {
            currentJokerModel = jokerModel;
            jokerCardView.DisplayCard(jokerModel);
            currentJokerModel.onActivation += CardModelActivated;
        }
    }

    public int UpdateSortingOrder(int baseValue)
    {
        return jokerCardView.UpdateSortingOrder(baseValue);
    }
    public void DeleteCard(bool runGraphics = true)
    {
        if (runGraphics)
            StartCoroutine(DestroyGraphics());
        else
            CardViewFactory.Instance.DeleteCard(currentJokerModel.cardType, this);
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
        transform.localScale = defaultScale;
        CardViewFactory.Instance.DeleteCard(currentJokerModel.cardType, this);
    }
    public void BeginDrag()
    {
        cardCollider.enabled = false;
    }

    public void EndDrag()
    {
        cardCollider.enabled = true;
    }

    void CardModelActivated()
    {
        StartCoroutine(CardPopRoutine(1.25f, 0.25f));
    }
    private IEnumerator CardPopRoutine(float maxScaleMultiplier, float duration)
    {
        Vector3 targetMaxScale = defaultScale * maxScaleMultiplier;

        float progress = 0f;

        while (progress < duration)
        {
            progress += Time.deltaTime / duration;

            float curveEvaluate = Mathf.Sin(progress * Mathf.PI);

            transform.localScale = Vector3.Lerp(defaultScale, targetMaxScale, curveEvaluate);

            yield return null;
        }

        transform.localScale = defaultScale;
    }
}
