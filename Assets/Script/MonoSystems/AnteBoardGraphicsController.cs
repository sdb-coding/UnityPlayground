using UnityEngine;
using System.Collections.Generic;

public class AnteBoardGraphicsController : MonoBehaviour, IBaseEventListener
{
    public BaseEvent switchToAnteEvent;
    public BaseEvent switchToStoreEvent;
    public GameObject[] listOfGraphics;
    private void OnEnable()
    {
        switchToAnteEvent.RegisterListener(this);
        switchToStoreEvent.RegisterListener(this);
    }
    private void OnDisable()
    {
        switchToAnteEvent.UnRegisterListener(this);
        switchToStoreEvent.UnRegisterListener(this);
    }
    public void OnEventRaised(BaseEvent baseEvent)
    {
        if (baseEvent == switchToAnteEvent)
        {
            EnableAnteBoardGraphics();
        } else if (baseEvent == switchToStoreEvent)
        {
            DisableAnteBoardGraphics();
        }
    }
    void EnableAnteBoardGraphics()
    {
        foreach (var item in listOfGraphics)
        {
            item.SetActive(true);
        }
    }
    void DisableAnteBoardGraphics()
    {
        foreach (var item in listOfGraphics)
        {
            item.SetActive(false);
        }
    }
}
