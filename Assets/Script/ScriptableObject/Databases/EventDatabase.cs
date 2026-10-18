using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EventDatabase", menuName = "Data/Databases/EventDatabase")]
public class EventDatabase : ScriptableObject
{
    [HideInInspector]
    public List<BaseEvent> events = new List<BaseEvent>();

    public T GetEvent<T>(string eventName) where T : BaseEvent
    {
        for (int i = 0; i < events.Count; i++)
        {
            if (events[i] != null && events[i].name == eventName)
            {
                return events[i] as T;
            }
        }
        return null;
    }
}
