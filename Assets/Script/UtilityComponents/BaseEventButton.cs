using UnityEngine;
using System.Collections.Generic;

public class BaseEventButton : MonoBehaviour, IClickable
{
    public BaseEvent baseEvent;
    public void OnClick()
    {
        BaseEvent.Raise(baseEvent);
    }
}
