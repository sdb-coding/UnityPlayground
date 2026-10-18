using UnityEngine;
using System.Collections.Generic;
using System;

public class HashSetBase<T> : ScriptableObject
{
    [NonSerialized] public HashSet<T> Items = new HashSet<T>();
    /// <summary>
    /// Event that happens when an object is removed or added to the set (ObjectInQuestion, Removed)
    /// </summary>
    public event Action<T, bool> changed;
    public virtual void Add(T t)
    {
        if (!Items.Contains(t))
        {
            Items.Add(t);
            changed?.Invoke(t, false);
        }
    }

    public virtual void Remove(T t)
    {
        if (Items.Contains(t))
        {
            Items.Remove(t);
            changed?.Invoke(t, true);
        }
    }
    public virtual bool Contains(T t)
    {
        return Items.Contains(t);
    }
}
