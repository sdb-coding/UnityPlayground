using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public static class CardAnimUtility
{
    const float distancePrecision = .01f;
    public static void MoveCardToPosition(ICardPresenter cardPresenter, float cardDelta, Vector3 endPosition, Action<ICardPresenter> callback)
    {
        cardPresenter.monoBehaviour.StartCoroutine(AnimateCardMoving(cardPresenter,cardDelta, endPosition, callback));
    }
    static IEnumerator AnimateCardMoving(ICardPresenter cardPresenter, float cardDelta, Vector3 endPosition, Action<ICardPresenter> callback)
    {
        while(Vector3.Distance(cardPresenter.Transform.position, endPosition) > distancePrecision)
        {
            cardPresenter.Transform.position = Vector3.MoveTowards(cardPresenter.Transform.position, endPosition, cardDelta);
            yield return null;
        }
        if (callback != null)
            callback(cardPresenter);
    }
}
