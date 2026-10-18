using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class RunTimeSets<T> : ScriptableObject
{
	[NonSerialized] public List<T> Items = new List<T>();
	/// <summary>
	/// Event that happens when an object is removed or added to the set (ObjectInQuestion, Removed)
	/// </summary>
    public event Action<T,bool> changed;
    public virtual void Add(T t)
	{
		if (!Items.Contains(t)) 
		{
			Items.Add(t);
			changed?.Invoke(t,false);
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

	public List<T> GetItems()
	{
		CheckItemsForNull();
		return Items;
	}
	public T GetItemsAtIndex(int index)
	{
		return Items[index];
	}
	public void SetItemAtIndex(int index,T value)
	{
		Items[index] = value;
	}
	public void SetItems(List<T> newItems)
	{
		for (int i = 0; i < Items.Count; i++)
		{
			changed?.Invoke(Items[i], true);
        }
        Items = newItems;
        for (int i = 0; i < Items.Count; i++)
        {
            changed?.Invoke(Items[i], false);
        }
    }
    public int GetItemListSize()
    {
        CheckItemsForNull();
        return Items.Count;
    }

	void CheckItemsForNull()
	{
		for (int i = Items.Count - 1; i >=0 ; i--)
        {
            if (Items[i] == null || Items[i].ToString() == "null")
            {
                Items.RemoveAt(i);
            }
        }
	}
}
