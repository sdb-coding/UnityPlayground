using UnityEngine;
using System.Collections.Generic;

public class StoreBoardGraphicsController : MonoBehaviour, IBaseEventListener
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
            DisableStoreBoardGraphics();
        }
        else if (baseEvent == switchToStoreEvent)
        {
            EnableStoreBoardGraphics();
        }
    }
    void EnableStoreBoardGraphics()
    {
        foreach (var item in listOfGraphics)
        {
            item.SetActive(true);
        }
    }
    void DisableStoreBoardGraphics()
    {
        foreach (var item in listOfGraphics)
        {
            item.SetActive(false);
        }
    }
}
