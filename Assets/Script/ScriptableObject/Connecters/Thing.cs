using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thing : MonoBehaviour {

	public ThingSet RunTimeSet;
	public void SetThingSet(ThingSet newThingset)
	{
		RunTimeSet = newThingset;
		RunTimeSet.Add(this);
	}
	void OnEnable()
	{
		if (RunTimeSet == null)
		{
			Debug.Log("No ThingSet attached to object: " + gameObject.name);
			return;
		}
		RunTimeSet.Add(this);
	}

	void OnDisable()
	{
		if (RunTimeSet == null)
		{
			Debug.Log("No ThingSet attached to object: " + gameObject.name);
			return;
		}
		RunTimeSet.Remove(this);
	}
}
