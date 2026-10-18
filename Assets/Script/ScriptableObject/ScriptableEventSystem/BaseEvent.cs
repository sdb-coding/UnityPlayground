using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseEvent", menuName = "Events/BaseEvent", order = 0)]
public class BaseEvent : ScriptableObject
{
#if UNITY_EDITOR
    [Multiline]
    public string DeveloperDescription = "";
#endif
    protected List<IBaseEventListener> listeners = new List<IBaseEventListener>();

    private void Awake()
    {
        listeners = new List<IBaseEventListener>();
    }
    [ContextMenu("Raise")]
    protected void Raise()
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventRaised(this);
        }
    }

    public void RegisterListener(IBaseEventListener listener)
    {
        if (!listeners.Contains(listener))
            listeners.Add(listener);
    }
    public void UnRegisterListener(IBaseEventListener listener)
    {
        if (listeners.Contains(listener))
            listeners.Remove(listener);
    }

    public int GetListenerListSize()
    {
        CheckListenersForNull();
        return listeners.Count;
    }
    protected void CheckListenersForNull()
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            if (listeners[i] == null)
            {
                listeners.RemoveAt(i);
            }
        }
    }
    public static void Raise(BaseEvent baseEvent)
    {
        if (baseEvent != null)
            baseEvent.Raise();
    }

}
public abstract class BaseEvent<T> : BaseEvent
{
    protected readonly List<IBaseEventListener<T>> typedListeners = new List<IBaseEventListener<T>>();

    public virtual void Raise(T value)
    {
        // 1. Trigger non-generic listeners if any exist
        base.Raise();

        // 2. Trigger typed payload listeners
        for (int i = typedListeners.Count - 1; i >= 0; i--)
        {
            if (typedListeners[i] != null)
            {
                typedListeners[i].OnEventRaised(value);
            }
        }
    }

    public void RegisterListener(IBaseEventListener<T> listener)
    {
        if (listener != null && !typedListeners.Contains(listener))
            typedListeners.Add(listener);
    }

    public void UnRegisterListener(IBaseEventListener<T> listener)
    {
        if (listener != null && typedListeners.Contains(listener))
            typedListeners.Remove(listener);
    }
}

public interface IBaseEventListener<T>
{
    void OnEventRaised(T value);
}
public class IntEvent : BaseEvent<int> { }

public class FloatEvent : BaseEvent<float> { }

public class StringEvent : BaseEvent<string> { }
public struct PotScorePayload
{
    public string potGuid;
    public ScoreModel.ScoringResult currentScore;

    public PotScorePayload(string potGuid, ScoreModel.ScoringResult currentScore)
    {
        this.potGuid = potGuid;
        this.currentScore = currentScore;
    }
}

// Concrete Event Class
public class PotScoreEvent : BaseEvent<PotScorePayload> { }
