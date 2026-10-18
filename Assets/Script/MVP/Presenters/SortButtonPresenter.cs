using UnityEngine;
using System.Collections.Generic;

public class SortButtonPresenter : MonoBehaviour, IClickable
{
    public SortType sortType;
    public GameObject referenceObject;
    ICardPresenterHolder cardPresenterHolder;
    void Start()
    {
        if (referenceObject == null)
        {
            Debug.LogError($"No reference object for sort button {gameObject.name}");
            Destroy(this);
            return;
        }
        cardPresenterHolder = referenceObject.GetComponent<ICardPresenterHolder>();
        if (cardPresenterHolder == null)
        {
            Debug.LogError($"No ICardPresenterHolder found on: {referenceObject.name} for {gameObject.name}");
            Destroy(this);
            return;
        }
    }
    public void OnClick()
    {
        cardPresenterHolder.SortBy(sortType);
    }
}
